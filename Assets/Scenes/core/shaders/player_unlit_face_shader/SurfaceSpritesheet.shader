Shader "Custom/Emissive Spritesheet" 
{
	Properties{
		_MainTex ("Texture", 2D) = "white" {}
		_EmissionColor("Color Overlay", Color) = (1,1,1)
		_ColorMaskColor("Color Mask Color", Color) = (0,0,0)
		_TexWidth("Width", int) = 3
		_TexHeight("Height", int) = 3
		_CurrentSprite("Current Sprite", int) = 0
		_EmissiveScale("Emissive Scale", float) = 1
	}
		
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		int _TexWidth;
		int _TexHeight;
		int _CurrentSprite;
		half4 _EmissionColor;
		float _EmissiveScale;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 uv = IN.uv_MainTex;
				
			// modify the uv coordinate based on the input to sample the correct "sprite"
			#if !UNITY_UV_STARTS_AT_TOP
				uv.y = 1.0 - uv.y;
			#endif

			// Modify our UV coordinate based on the number of sprites, and our current sprite
			float newRangeX = 1.0f / float(_TexWidth);
			float newRangeY = 1.0f / float(_TexHeight);

			// Find the x and y position of _CurrentSprite
			uint xPos = _CurrentSprite % (uint)_TexWidth;
			uint yPos = (_TexHeight - 1) - (_CurrentSprite / (uint)_TexHeight);

			// Adjust our uv coordinate based on the sprite range and position
			uv.x = (uv.x) * newRangeX + (xPos * newRangeX);
			uv.y = (uv.y) * newRangeY + (yPos * newRangeY);
			half3 col = tex2D(_MainTex, uv);

			o.Albedo.rgb = col.rgb;
			o.Emission.rgb = col.rgb * _EmissiveScale * _EmissionColor;
		}
		ENDCG
	}
	
	FallBack "Diffuse"
}
