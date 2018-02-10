Shader "Custom/StandardCustomColormask"
{
	Properties
	{
		_MainTex("Albedo (RGB), Alpha (A)", 2D) = "white" {}
		_ColorMask("Color Mask (RGB), Alpha (A)", 2D) = "black" {}
		_ColorMaskColor("Color", Color) = (0,0,0)
		_MetallicGlossMap("Metallic (R), Smoothness (A)", 2D) = "black" {}
		_BumpMap("Normal (RGB)", 2D) = "bump" {}
		_EmissionMap("Emissive Map (RGB)", 2D) = "white" {}
		_EmissionColor("Emissive Color", Color) = (0,0,0)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}

		CGINCLUDE
		#define _GLOSSYENV 1
		ENDCG

		CGPROGRAM
			#pragma target 3.0
			#include "UnityPBSLighting.cginc"
			#pragma surface surf Standard
			#pragma exclude_renderers gles

			struct Input
			{
				float2 uv_MainTex;
			};

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _ColorMask;
			sampler2D _MetallicGlossMap;
			sampler2D _EmissionMap;
			fixed4 _EmissionColor;
			fixed3 _ColorMaskColor;

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				fixed4 albedo = tex2D(_MainTex, IN.uv_MainTex);
				fixed4 metallic = tex2D(_MetallicGlossMap, IN.uv_MainTex);
				fixed4 emission = tex2D(_EmissionMap, IN.uv_MainTex);
				fixed3 normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), 1);

				fixed3 mask = tex2D(_ColorMask, IN.uv_MainTex).rgb;
				
				// Invert the mask and add the color, then clamp.
				// Result is what used to be black is now white,
				// and what used to be white is now the overlay color.
				mask = fixed3(1, 1, 1) - mask + _ColorMaskColor;
				mask = clamp(mask, fixed3(0, 0, 0), fixed3(1, 1, 1));

				// ... which means we can just multiply!
				albedo.rgb = albedo.rgb * mask;

				o.Albedo = albedo.rgb;
				o.Normal = normal;
				o.Emission = emission.rgb * _EmissionColor.rgb;
				o.Metallic = metallic.r;
				o.Smoothness = metallic.a;
				o.Alpha = albedo.a;
				//o.Occlusion = metallic.g;
			}
		ENDCG
	}

	FallBack "Diffuse"
}
