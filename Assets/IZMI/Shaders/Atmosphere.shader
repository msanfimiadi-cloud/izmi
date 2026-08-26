Shader "IZMI/Atmosphere"
{
    Properties
    {
        _Color ("Atmosphere Color", Color) = (0.12, 0.5, 1.0, 0.72)
        _Power ("Rim Power", Range(0.5, 8.0)) = 2.6
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Cull Front
        ZWrite Off
        Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct AppData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct VertexToFragment
            {
                float4 position : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDirection : TEXCOORD1;
            };

            fixed4 _Color;
            float _Power;

            VertexToFragment Vert(AppData input)
            {
                VertexToFragment output;
                output.position = UnityObjectToClipPos(input.vertex);

                float3 worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                output.viewDirection = normalize(_WorldSpaceCameraPos.xyz - worldPosition);
                return output;
            }

            fixed4 Frag(VertexToFragment input) : SV_Target
            {
                float facing = saturate(dot(normalize(input.worldNormal), normalize(input.viewDirection)));
                float rim = pow(1.0 - facing, _Power);
                return fixed4(_Color.rgb * rim, _Color.a * rim);
            }
            ENDCG
        }
    }

    Fallback Off
}
