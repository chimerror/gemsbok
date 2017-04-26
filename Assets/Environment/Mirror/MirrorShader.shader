// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MirrorShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        [HideInInspector] _ReflectionTex ("", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct vertexOutput
            {
                float2 uv : TEXCOORD0;
                float4 refl : TEXCOORD1;
                float4 pos : SV_POSITION;
            };

            float4 _MainTex_ST;

            vertexOutput vert(float4 pos : POSITION, float2 uv : TEXCOORD0)
            {
                vertexOutput output;
                output.pos = UnityObjectToClipPos(pos);
                output.uv = TRANSFORM_TEX(uv, _MainTex);
                output.refl = ComputeScreenPos(output.pos);
                return output;
            }

            sampler2D _MainTex;
            sampler2D _ReflectionTex;
            fixed4 frag(vertexOutput i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 refl = tex2Dproj(_ReflectionTex, UNITY_PROJ_COORD(i.refl));
                return tex * refl;
            }

            ENDCG
        }
    }
}
