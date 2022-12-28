// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Fog/Blur"
{
    Properties
    {
        [HideInInspector]_MainTex ("Texture", 2D) = "white" {}
        _BlurRadius("_BlurRadius", float) = 3
    }
    SubShader
    {
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct a2v{
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };
        struct v2f{
            float4 vertex : SV_POSITION;
            float2 uvs[5] : TEXCOORD1;
        };

        CBUFFER_START(UnityPerMaterial)
        float4 _MainTex_ST;
        uniform float4 _TexSize;
        float _BlurRadius;
        CBUFFER_END

        SAMPLER(sampler_MainTex);
        TEXTURE2D(_MainTex);

        v2f vert_Vertical_Blur(a2v i){
            v2f o;
            o.vertex = TransformObjectToHClip(i.vertex.xyz);
            float2 texSize = i.uv * _TexSize.xy;
            o.uvs[0] = i.uv;
            o.uvs[1] = (texSize + float2(0,1.5) * _BlurRadius)/_TexSize;
            o.uvs[2] = (texSize + float2(0,-1.5) * _BlurRadius)/_TexSize;
            o.uvs[3] = (texSize + float2(0,2.5) * _BlurRadius)/_TexSize;
            o.uvs[4] = (texSize + float2(0,-2.5) * _BlurRadius)/_TexSize;
            return o;
        }

        v2f vert_Horizontal_Blur(a2v i){
            v2f o;
            o.vertex = TransformObjectToHClip(i.vertex.xyz);
            float2 texSize = i.uv * _TexSize;
            o.uvs[0] = i.uv;
            o.uvs[1] = (texSize + float2(1.5,0) * _BlurRadius)/_TexSize;
            o.uvs[2] = (texSize + float2(-1.5,0) * _BlurRadius)/_TexSize;
            o.uvs[3] = (texSize + float2(2.5,0) * _BlurRadius)/_TexSize;
            o.uvs[4] = (texSize + float2(-2.5,0) * _BlurRadius)/_TexSize;
            return o;
        }

        half4 frag_Blur(v2f i):SV_TARGET{
            half weight[3] = {0.4026, 0.2442, 0.0545};

            half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvs[0]);
            col = col * weight[0];

            for (int j = 1; j < 3; j++)
            {
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvs[2 * j - 1]) * weight[j];
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvs[2 * j]) * weight[j];
            }
            return col;
        }
    ENDHLSL
        ZTest Always
        ZWrite off
        Cull off

        //Pass1
        Pass
        {
            NAME "GAUSSIAN_BLUR_VERTICAL"

            HLSLPROGRAM
            #pragma vertex vert_Vertical_Blur
            #pragma fragment frag_Blur
            ENDHLSL
        }

        //Pass2
        Pass
        {
            NAME "GAUSSIAN_BLUR_HORIZONTAL"

            HLSLPROGRAM
            #pragma vertex vert_Horizontal_Blur
            #pragma fragment frag_Blur
            ENDHLSL
        }
    }
}
