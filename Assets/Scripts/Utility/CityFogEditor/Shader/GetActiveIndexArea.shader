// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Fog/GetActiveIndexArea"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LastTex ("LstTex", 2D) = "black" {}
        _Channel ("Channel", int) = 0
        _Index ("Index", int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
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
            int _Channel;
            int _Index;
            CBUFFER_END

            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_LastTex);
            TEXTURE2D(_LastTex);


            v2f vert(a2v i){
                v2f o;
                o.vertex = TransformObjectToHClip(i.vertex.xyz);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                return o;
            }
            half4 frag(v2f i) : SV_TARGET{
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half last = SAMPLE_TEXTURE2D(_LastTex, sampler_LastTex, i.uv).r;
                int index = (_Channel == 1) ? (int)(col.g * 255) : (int)(col.r * 255);
                half4 ret = half4(0,0,0,1);
                if(index == _Index && last == 0){
                    ret = half4(1,0,0,1);
                }
                return ret;
            }
        ENDHLSL  
        }

    }
}
