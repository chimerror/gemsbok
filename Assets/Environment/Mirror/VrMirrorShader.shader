Shader "Custom/VrMirrorShader"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _LeftEyeTexture("Left Eye Texture", 2D) = "white" {}
        _RightEyeTexture("Right Eye Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags{ "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ STEREO_RENDER

            #include "UnityCG.cginc"

            struct vertexOutput
            {
                float2 uv : TEXCOORD0;
            };

            float4 _MainTex_ST;

            vertexOutput vert(float4 pos : POSITION, float2 uv : TEXCOORD0, out float4 outPos : SV_POSITION)
            {
                vertexOutput output;
                outPos = mul(UNITY_MATRIX_MVP, pos);
                output.uv = TRANSFORM_TEX(uv, _MainTex);
                return output;
            }

            sampler2D _MainTex;
            sampler2D _LeftEyeTexture;
            sampler2D _RightEyeTexture;
            fixed4 frag(vertexOutput i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                float2 sUV = screenPos.xy / _ScreenParams.xy;
                fixed4 refl = fixed4(0.0, 0.0, 0.0, 0.0);
                if (unity_CameraProjection[0][2] < 0)
                {
                    refl = tex2D(_LeftEyeTexture, sUV);
                }
                else
                {
                    refl = tex2D(_RightEyeTexture, sUV);
                }
                return tex * refl;
            }

            ENDCG
        }
    }

    Fallback "Diffuse"
}
