Shader "Fog/FogShowAndHideWithIndex"
{
	Properties
	{
		_Color("主颜色", color) = (1,1,1,1)
		_HighLightColor("高亮颜色", color) = (1,1,1,1)
		_HightLightSpeed("高光闪烁速度", float) = 0
		_CloudTex("云雾图", 2D) = "white"{}
		_FlowTex("云雾扰动图",2D) = "white"{}
		_Speed("扰动速度", float) = 0
		_BlurTexRuntime("解锁区域实时模糊", 2D) = "black"{}
		_BlurTexRuntime_Limit("未解锁区域高光模糊", 2D) = "black"{}
		_Fade("雾消散系数", float) = 0
		_FadeSpeed("解锁时雾消散速度", float) = 0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent+100"
		}
		LOD 100
		Cull off
		ZWrite off
		blend SrcAlpha OneMinusSrcAlpha

		HLSLINCLUDE
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct a2v
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 positionSS : TEXCOORD2;
			};

			cbuffer addonData
			{
				uint indexData[256];//已经解锁的区域
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _CloudTex_ST, _BlurTexRuntime_ST;
			float4 _Color;
			float4 _HighLightColor;
			float _HightLightSpeed;
			float _Speed;
			float _Fade;
			float _FadeSpeed;
			CBUFFER_END

			TEXTURE2D_X_FLOAT(_CameraDepthTexture);
			SAMPLER(sampler_CameraDepthTexture);
			TEXTURE2D(_CloudTex);
			SAMPLER(sampler_CloudTex);
			TEXTURE2D(_FlowTex);
			SAMPLER(sampler_FlowTex);
			TEXTURE2D(_BlurTexRuntime);
			SAMPLER(sampler_BlurTexRuntime);
			TEXTURE2D(_BlurTexRuntime_Limit);
			SAMPLER(sampler_BlurTexRuntime_Limit);

			v2f vert(a2v i)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(i.positionOS.xyz);
				o.uv = TRANSFORM_TEX(i.uv, _BlurTexRuntime);
				o.uv2 = TRANSFORM_TEX(i.uv2, _CloudTex);
				o.positionSS = ComputeScreenPos(o.positionCS);
				return o;
			}
		ENDHLSL

		Pass
		{
			HLSLPROGRAM

			half4 frag(v2f i) : SV_TARGET
			{
				float2 screenPos = i.positionSS.xy / i.positionSS.w;
				float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenPos).r;
				float4 depth0 = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenPos);
				float depthValue = LinearEyeDepth(depth, _ZBufferParams);
				float z = i.positionSS.w;
				z = abs(depthValue - z);
				z *= _Fade;

				//flow map
				float3 flowVal = (SAMPLE_TEXTURE2D(_FlowTex, sampler_FlowTex, i.uv)).xyz;
				float dif1 = frac(_Time.y * _Speed * 0.25);
				float dif2 = frac(_Time.y * _Speed * 0.25 + 0.5);
				half lerpVal = abs((0.5 - dif2) / 0.5);

				half4 col1 = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, i.uv2 - flowVal.xy * dif1);
				half4 col2 = SAMPLE_TEXTURE2D(_CloudTex, sampler_CloudTex, i.uv2 - flowVal.xy * dif2);
				float4 blur_col = SAMPLE_TEXTURE2D(_BlurTexRuntime, sampler_BlurTexRuntime, i.uv);
				float4 blur_col_limit = SAMPLE_TEXTURE2D(_BlurTexRuntime_Limit, sampler_BlurTexRuntime_Limit, i.uv);
				float4 final = saturate(lerp(col2, col1, lerpVal) * _Color + (cos(_Time.y * _HightLightSpeed) + 1) * 0.5f * _HighLightColor * blur_col_limit.r);
				final.a = saturate(col1.a * min(1, col2.a)*_Color.a * z) * (1 - blur_col.r);
				return final;
			}
			ENDHLSL
		}
	}
}
