Shader "PostEffect/XRayScan"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanDistance("扫描距离(随时间)", float) = 1
        _ScanColor("扫描颜色", color) = (1,1,1,1)
        _ScanRange("扫描范围", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry+600" }
        LOD 100
        ZWrite off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos :TEXCOORD1;
            };
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float _ScanDistance, _ScanRange;
            half4 _ScanColor;
            CBUFFER_END

            TEXTURE2D_X_FLOAT(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float2 screenPos = i.screenPos.xy / i.screenPos.w;
                float depth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenPos).r;
                float depth_value = Linear01Depth(depth, _ZBufferParams);
                if(depth_value < _ScanDistance && depth_value < 1 && _ScanDistance - depth_value < _ScanRange)
                {
                    float diff = 1 - (_ScanDistance - depth_value) / _ScanRange;
                    col = lerp(col, _ScanColor, diff);
                }
                return col;
            }
            ENDHLSL
        }
    }
}
