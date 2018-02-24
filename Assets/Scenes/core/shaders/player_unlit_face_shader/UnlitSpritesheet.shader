Shader "Unlit/UnlitSpritesheet"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TexWidth("Width", int) = 3
		_TexHeight("Height", int) = 3
		_CurrentSprite("Current Sprite", int) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			int _TexWidth;
			int _TexHeight;
			int _CurrentSprite;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// modify the uv coordinate based on the input to sample the correct "sprite"
				#if !UNITY_UV_STARTS_AT_TOP
					i.uv.y = 1.0 - i.uv.y;
				#endif

				// Modify our UV coordinate based on the number of sprites, and our current sprite
				float newRangeX = 1.0f / float(_TexWidth);
				float newRangeY = 1.0f / float(_TexHeight);

				// Find the x and y position of _CurrentSprite
				uint xPos = _CurrentSprite % (uint)_TexWidth;
				uint yPos = (_TexHeight - 1) - (_CurrentSprite / (uint)_TexHeight);

				// Adjust our uv coordinate based on the sprite range and position
				i.uv.x = (i.uv.x) * newRangeX + (xPos * newRangeX);
				i.uv.y = (i.uv.y) * newRangeY + (yPos * newRangeY);
				fixed4 col = tex2D(_MainTex, i.uv);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
