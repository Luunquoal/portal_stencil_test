using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;

public class world_manager : MonoBehaviour
{
    public GameObject portal;
    public GameObject i_portal_showing_pos;
    public GameObject i_portal_showing_neg;
    public GameObject t_portal_showing_pos;
    public GameObject t_portal_showing_neg;

    public GameObject center_eye_cam;
    public GameObject right_hand; // use this in update to spawn the throwable cubes close to it, then handle the cubes world (maybe track the world the hand is in aswell to handle cases where a cube is spawend through a portal)

    public Material pos_showing_mat;
    public Material neg_showing_mat;

    public List<GameObject> worlds;

    public float current_world = 0;

    // list of materials 
    private List<world_material_list> material_lists = new List<world_material_list>();

    struct world_material_list
    {
        public int world_id;
        public List<Material> mats;

        public world_material_list(int _id)
        {
            world_id = _id;
            mats = new List<Material>();
        }
    }

    private void Awake()
    {
        // make new list of world id + mat list pairs
        for(int i = 0;  i < worlds.Count; i++)
        {
            //create pair for each world entry
            material_lists.Add(new world_material_list(i));
            //fill the list of materials in that pair
            get_material_repeat_in_children(worlds[i], i);
        }

        // set all stencilrefs to their world id + 1 to make 0 free for the current worlds stencil
        foreach(world_material_list wml in material_lists)
        {
            foreach(Material mat in wml.mats)
            {
                int prop_id = Shader.PropertyToID("_StencilRef");
                mat.SetFloat(prop_id, (int)(wml.world_id + 1));
            }
        }

        // set the materials on the seperate materials for the different direction on the two setups
        i_portal_showing_neg.GetComponent<Renderer>().material = neg_showing_mat;
        t_portal_showing_neg.GetComponent<Renderer>().material = neg_showing_mat;

        i_portal_showing_pos.GetComponent<Renderer>().material = pos_showing_mat;
        t_portal_showing_pos.GetComponent<Renderer>().material = pos_showing_mat;

        //set the visibility of the inside and through portal setups to the through one
        use_inside_portal_setup(false);

        // set the stencilref of the portal materials to the one above and the one below the current world + 1
        pos_showing_mat.SetFloat("_StencilRef", (((int) current_world + worlds.Count + 1) % worlds.Count) + 1);
        neg_showing_mat.SetFloat("_StencilRef", (((int) current_world + worlds.Count - 1) % worlds.Count) + 1);


        foreach(Material m in material_lists[(int)current_world].mats)
        {
            m.SetFloat("_StencilRef", 0);
        }
    }

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            // do the spawning stuff somewhere here
        }
    }


    // ADDING 1 TO ALL THE STENCIL REFS OF MATERIALS NOT SHOWN AROUND YOU WHEN OUTSIDE THE PORTAL TO STORE AWAY OBJECTS USING INDICES 1 TO N+1 AND WRITE 0 TO ALL STENCILS IN THE WORLD AROUND THE USER WHEN OUT OF THE PORTAL

    public void enter_portal()
    {
        // sign returns 1 if 0 or bigger and -1 below 0, no need for conditions to handle this, i looked it up, trust past simon
        // get direction your entering the portal in
        float enter_dir = -1 * Mathf.Sign(Vector3.Dot(
            (center_eye_cam.transform.position - portal.transform.position).Multiply(new Vector3(1,0,1)),
            portal.transform.forward.Multiply(new Vector3(1,0,1))));

        // change the stencil_value of the world you leave back to its index+1
        int previous_world_id = Mathf.RoundToInt(current_world);
        foreach(Material mat in material_lists[previous_world_id].mats)
        {
            mat.SetInt("_StencilRef", material_lists[previous_world_id].world_id + 1);
        }

        // update world selection float to be inbetween the values of the worlds on both sides of the portal
        current_world = (current_world + (enter_dir * 0.5f)) % worlds.Count;

        // enable the in portal setup
        use_inside_portal_setup(true);

        //update the stencils to the 
        pos_showing_mat.SetFloat("_StencilRef", Mathf.CeilToInt(current_world) + 1);
        neg_showing_mat.SetFloat("_StencilRef", Mathf.FloorToInt(current_world) + 1);
    }

    public void exit_portal()
    {
        // get direction when leaving portal
        float exit_dir = Mathf.Sign(Vector3.Dot(
            (center_eye_cam.transform.position - portal.transform.position).Multiply(new Vector3(1, 0, 1)),
            portal.transform.forward.Multiply(new Vector3(1, 0, 1))));

        // calculate next world you enter as float and int
        current_world = (current_world + worlds.Count + (exit_dir * 0.5f)) % worlds.Count;
        int current_world_id = Mathf.RoundToInt(current_world);

        // activate needed configuration for portal
        use_inside_portal_setup(false);

        // set _StencilRef of all materials in that world to 0 to make it visible as
        foreach(Material mat in material_lists[current_world_id].mats)
        {
            mat.SetInt("_StencilRef", 0);
        }

        // set stencilRef in portal materials to fit new worlds
        pos_showing_mat.SetFloat("_StencilRef", ((current_world_id + worlds.Count + 1) % worlds.Count) + 1);
        neg_showing_mat.SetFloat("_StencilRef", ((current_world_id + worlds.Count - 1) % worlds.Count) + 1);
    }

    private void use_inside_portal_setup(bool inside)
    {
        i_portal_showing_neg.SetActive(inside);
        i_portal_showing_pos.SetActive(inside);
        t_portal_showing_neg.SetActive(!inside);
        t_portal_showing_pos.SetActive(!inside);
    }
    
    //uiiiii lets do some recursion \(°_°)/
    // recursively adds materials in children of the given gameobject to the list in the pair with the given world index
    private void get_material_repeat_in_children(GameObject _go, int _world_index)
    {
        int cc = _go.transform.childCount;
        for(int i = 0; i < cc; i++)
        {
            get_material_repeat_in_children(_go.transform.GetChild(i).gameObject, _world_index);
        }
        
        Renderer r = _go.GetComponent<Renderer>();
        if(r != null)
        {
            Material[] found_materials = _go.GetComponent<Renderer>().materials;
            foreach (Material found_mat in found_materials)
            {
                if (found_mat != null /*&& found_mat.shader.pro HasInteger("_StencilRef")*/)
                {
                    Debug.Log(found_mat);
                    material_lists[_world_index].mats.Add(found_mat);
                };
            }
        }
        
    }
}
