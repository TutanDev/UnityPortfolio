Shader "TutanDev/TransparentChroma"
{
	Properties
	{
		_MainTex("Video Texture", 2D) = "white" {}
		[ColorDebug] _RemoveColor("Remove Color", Color) = (1, 1, 1, 1)
		_HueTolerance("HueTolerance", Range(0, 0.2)) = 0.01
		_HueBlend("HueBlend", Range(0, 1)) = 0
		_SaturationValueRanges("SaturationValueRanges", Vector) = (0, 1, 0, 1)
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex	: POSITION;
					float2 texcoord	: TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex	: SV_POSITION;
					half2 texcoord	: TEXCOORD0;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed4 _RemoveColor;
				float _HueTolerance;
				float _HueBlend;
				float4 _SaturationValueRanges;



				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					return o;
				}

				float3 rgbTohsv(float3 In)
				{
					float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
					float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
					float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
					float D = Q.x - min(Q.w, Q.y);
					float  E = 1e-10;
					return float3(abs(Q.z + (Q.w - Q.y) / (6.0 * D + E)), D / (Q.x + E), Q.x);
				}
				float3 hsvTorgb(float3 In)
				{
					float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
					float3 P = abs(frac(In.xxx + K.xyz) * 6.0 - K.www);
					return In.z * lerp(K.xxx, saturate(P - K.xxx), In.y);
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float4 textColor = tex2D(_MainTex, float2(i.texcoord.x, i.texcoord.y));

					float3 textColorHSV = rgbTohsv(textColor);
					float3 removeColorHSV = rgbTohsv(_RemoveColor);

					float delta = abs(textColorHSV.x - removeColorHSV.x);
					bool inSVRange = textColorHSV.y >= _SaturationValueRanges.x && textColorHSV.y <= _SaturationValueRanges.y && textColorHSV.z >= _SaturationValueRanges.z && textColorHSV.z <= _SaturationValueRanges.w;

					if (delta <= _HueTolerance && inSVRange)
					{
						textColor.a = 0;
					}

					if (textColor.a != 0 && delta <= _HueTolerance + _HueBlend && inSVRange)
					{
						textColor.a = abs(delta - _HueTolerance) / _HueBlend;
					}

					return textColor;
			   }
		   ENDCG
		   }
		}
	
	CustomEditor "TransparencyShaderEditor"
}