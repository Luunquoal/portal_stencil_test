using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class portal_events : MonoBehaviour
{
    private Collider portal_collider;
    public world_manager wm;

    private void Awake()
    {
        portal_collider = GetComponent<Collider>();
        Debug.Log("Portal Collider" + portal_collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        Debug.Log(other.gameObject);
        Debug.Log("tags: " + other.gameObject.tag);
        if (other.gameObject.CompareTag("MainCamera"))
        {
            wm.enter_portal();
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("MainCamera"))
        {
            wm.exit_portal();
        }
    }
}
