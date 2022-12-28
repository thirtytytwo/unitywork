Shader "Fog/Blend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CurTex ("Current RT", 2D) = "white"{}
    }
    SubShader
    {
        
        Pass{
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            struct a2v{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f{
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            CBUFFER_END

            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_CurTex);
            TEXTURE2D(_CurTex);

            v2f vert(a2v i){
                v2f o;
                o.vertex = TransformObjectToHClip(i.vertex);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                return o;
            }
            half4 frag(v2f i): SV_TARGET{
                half4 col1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 col2 = SAMPLE_TEXTURE2D(_CurTex, sampler_CurTex, i.uv);
                half4 final = saturate(col1 + col2);
                return final;
            }
            ENDHLSL
        }
    }
}
