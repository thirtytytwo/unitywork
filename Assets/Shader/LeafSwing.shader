Shader "Unlit/LeafSwing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FXTex01("FXTex01", 2D) = "white"{}
        _AmplitudeMin("AmplitudeMin", float) = 10
        _Amplitude("Amplitude", float) = 20
        _FXAnim01("FXAnim01", vector) = (0.06,-0.03,0,0)//xy控制和时间联系的主体， zw是一个偏移量
        _FXAnim02("FXAnim02", vector) = (0.02,0.06,0,0)//同上，好像是两条动画曲线的样子
        _FXScale01("FXScale01", float) = 9.4
        _FXScale02("FXScale02", float) = 2.4
        _FXAmp02("FXAmp02", float) = 0.54
        _FXAmp01("FXAmp01", float) = 0.26
        _Direction("Direction", vector) = (0.4,0.2,0.4,0)
        _CycleTime("CycleTime", float) = 0
        _Variation("Variation", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

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
                float3 normal : NORMAL;
                float4 vertexColor : COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half4 vertexColor : COLOR0;
            };
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST, _FXAnim02, _FXAnim01;
            half4 _Direction;
            float _AmplitudeMin, _Amplitude, _FXScale02, _FXScale01,_FXAmp01, _FXAmp02, _CycleTime, _Variation;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_FXTex01);
            SAMPLER(sampler_FXTex01);
            

            float3 BendingByWind(float CycleTime, float3 wpos, float channel)
            {
                float4 pos;
                float cycTime = 1.0 / CycleTime;
                float time = (-trunc(_Time.y * 0.01)) * 100.0 + _Time.y;//对时间做了操作
                float2 sample_uv_y;
                float2 sample_uv_x;
                float cycTo01Sin = sin(cycTime * time * 2 * PI) * 0.5 + 0.5;//改成01sin波
                pos.x = cycTo01Sin * (_Amplitude - _AmplitudeMin) + _AmplitudeMin;//lerp操作类似
                pos.x = (_CycleTime > 0.0) ? pos.x : _Amplitude;
                float2 fx_curve_02 = _FXAnim02.xy * float2(time, time) + _FXAnim02.zw;
                float2 fx_curve_01 = _FXAnim01.xy * float2(time, time) + _FXAnim01.zw;
                float tmp21;
                tmp21 = wpos.x * (-0.1);
                sample_uv_y = float2(tmp21, tmp21) * float2(_FXScale02,_FXScale02)+ fx_curve_02;
                sample_uv_x = float2(tmp21,tmp21) * float2(_FXScale01, _FXScale01) + fx_curve_01;
                float sampleRet_x, sampleRet_y;
                sampleRet_x = SAMPLE_TEXTURE2D_LOD(_FXTex01, sampler_FXTex01, sample_uv_x, 0.0).x - 0.5;
                sampleRet_y = SAMPLE_TEXTURE2D_LOD(_FXTex01, sampler_FXTex01, sample_uv_y, 0.0).y - 0.5;
                float ret = sampleRet_x * _FXAmp01 + sampleRet_y * _FXAmp02;
                float temp = wpos.x + wpos.y + wpos.z;
                ret += frac(temp) * _Variation;
                pos.x *= ret;
                float vcolor = (1.0f - channel) * (1.0f - channel);
                pos.x *= vcolor;
                pos.xzw = pos.xxx * _Direction.xyz + wpos.xyz;
                return pos.xzw;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformWorldToHClip(BendingByWind(_CycleTime, TransformObjectToWorld(v.vertex.xyz),v.vertexColor.b));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vertexColor = v.vertexColor;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {

                half4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv);
                clip(col.a - 0.001);
                return col;
            }
            ENDHLSL
        }
    }
}
