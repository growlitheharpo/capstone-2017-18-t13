Shader "Custom/Panning Standard Shader (Alpha Fade)" 
{
	Properties 
	{
		_Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 1.0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 1.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        _BumpMap("Normal Map", 2D) = "bump" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        [HDR] _EmissionColor("Color", Color) = (0,0,0)
		_EmissionStrength("Emission Strength", float) = 1.0
        _EmissionMap("Emission", 2D) = "white" {}

		_PanSpeedX("Pan Speed X", float) = 0
		_PanSpeedY("Pan Speed Y", float) = 0
	}
	SubShader 
	{
		Tags 
		{ 
			"RenderType"="Transparent"
			"Queue" = "Transparent"
		}
		LOD 200
		
		CGPROGRAM
		
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input 
		{
			float2 uv_MainTex;
		};


		half _Glossiness;
		half _Metallic;
		half _EmissionStrength;
		half _OcclusionStrength;

		half _PanSpeedX;
		half _PanSpeedY;

		fixed4 _Color;
		fixed4 _EmissionColor;

		sampler2D _MainTex;
		sampler2D _MetallicGlossMap;
		sampler2D _BumpMap;
		sampler2D _OcclusionMap;
		sampler2D _EmissionMap;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float2 uv = IN.uv_MainTex;
			uv.x += _Time * _PanSpeedX;
			uv.y += _Time * _PanSpeedY;

			uv = frac(uv);

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, uv) * _Color;
			fixed4 metallic = tex2D(_MetallicGlossMap, uv);
			fixed3 normal = UnpackScaleNormal(tex2D(_BumpMap, uv), 1);
			fixed occlusion = tex2D(_OcclusionMap, uv);
			fixed4 emission = tex2D(_EmissionMap, uv);

			o.Albedo = c.rgb;
			o.Normal = normal;
			o.Emission = emission.rgb * _EmissionColor.rgb * _EmissionStrength;
			o.Metallic = metallic.r * _Metallic;
			o.Smoothness = metallic.a * _Glossiness;
			o.Occlusion = occlusion * _OcclusionStrength;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
