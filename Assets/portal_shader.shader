Shader "Unlit/portal_shader"
{
    Properties
    {
        [IntegerRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry+1" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            Blend Zero One
            ZWrite Off

            Stencil
            {
                Ref[_StencilRef]
                Comp Always
                Pass Replace
            }
        }
    }
}