// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*
* Many, many, many thanks to the author of this blog: http://xroft666.blogspot.com/2015/07/glow-highlighting-in-unity.html
*/

Shader "Hidden/Custom/Highlight" {
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "black" {}
		_OccludeMap("Occlusion Map", 2D) = "black" {}
	}

	SubShader
	{
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }

		// OVERLAY GLOW		
		Pass 
		{
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _OccludeMap;

				fixed4 _Color;

				fixed4 frag(v2f_img IN) : COLOR
				{
					// invert for OPENGL
					#if !UNITY_UV_STARTS_AT_TOP
						IN.uv.y = 1.0 - IN.uv.y;
					#endif

					fixed4 mCol = tex2D(_MainTex, IN.uv);
					fixed3 overCol = tex2D(_OccludeMap, IN.uv).r * _Color.rgb * _Color.a;
					return mCol + fixed4(overCol, 1.0);
				}
			ENDCG
		}

		// ADDITIVE OVERLAY GLOW		
		Pass 
		{
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _OccludeMap;

				fixed4 _Color;

				fixed4 frag(v2f_img IN) : COLOR
				{
					// invert for OPENGL
					#if !UNITY_UV_STARTS_AT_TOP
						IN.uv.y = 1.0 - IN.uv.y;
					#endif

					fixed4 mCol = tex2D(_MainTex, IN.uv);
					fixed3 overCol = tex2D(_OccludeMap, IN.uv).r * _Color.rgb * _Color.a;

					// Additive: (overCol × Blend.One) + (mCol × Blend.One)
					return mCol + fixed4(overCol.rgb, length(overCol));
				}
			ENDCG
		}
					
		// ALPHA-BLENDED OVERLAY GLOW		
		Pass 
		{
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _OccludeMap;

				fixed4 _Color;

				fixed4 frag(v2f_img IN) : COLOR
				{
					// invert for OPENGL
					#if !UNITY_UV_STARTS_AT_TOP
						IN.uv.y = 1.0 - IN.uv.y;
					#endif

					fixed4 mCol = tex2D(_MainTex, IN.uv);
					fixed3 s = tex2D(_OccludeMap, IN.uv).r * _Color.rgb * _Color.a;
					fixed4 overCol = fixed4(s.rgb, length(s));
					//return mCol + fixed4(overCol, mCol.a + length(overCol));

					// (overCol × Blend.SourceAlpha) + (mCol × Blend.InvSourceAlpha)
					fixed sourceAlpha = overCol.a;
					fixed4 invSourceAlpha = fixed4(1.0f,1,1,1) * (1.0f - sourceAlpha);
					
					return overCol  + mCol * invSourceAlpha;
				}
			ENDCG
		}


		// OCCLUSION	
		Pass 
		{
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _OccludeMap;

				// Note: Android platforms do not support "fixed", has to be fixed4
				#if (SHADER_API_GLES | SHADER_API_GLES3)

				fixed4 frag(v2f_img IN) : COLOR
				{
					return tex2D(_MainTex, IN.uv) - tex2D(_OccludeMap, IN.uv);
				}

				#else

				fixed frag(v2f_img IN) : COLOR
				{
					return tex2D(_MainTex, IN.uv).r - tex2D(_OccludeMap, IN.uv).r;
				}

				#endif
			ENDCG
		}

		// Depth sort and draw to render texture
		Pass 
		{
			Tags {"Queue" = "Transparent"}
			Cull Back
			Lighting Off
			ZWrite Off
			ZTest LEqual
			ColorMask RGBA
			Blend OneMinusDstColor One


			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				sampler2D _CameraDepthTexture;

				struct v2f 
				{
					float4 vertex : POSITION;
					float4 projPos : TEXCOORD1;
				};

				v2f vert(float4 v : POSITION) 
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v);
					o.projPos = ComputeScreenPos(o.vertex);
					return o;
				}

				// Note: Android platforms do not support "fixed", has to be fixed4
				#if (SHADER_API_GLES | SHADER_API_GLES3)

				fixed4 frag(v2f i) : COLOR 
				{
					float depthVal = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r);
					float zPos = i.projPos.z;

					return step(zPos, depthVal);
				}

				#else

				fixed frag(v2f i) : COLOR 
				{
					float depthVal = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r);
					float zPos = i.projPos.z;

					return step(zPos, depthVal);
				}

				#endif
			ENDCG
		}
	}
}
