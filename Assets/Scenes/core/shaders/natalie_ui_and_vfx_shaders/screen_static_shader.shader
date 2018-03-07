Shader "Sprites/Sprite masked static"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Mask("Mask Texture",2D) = "white"{}
		_Color("Tint", Color) = (1,1,1,1)
		_Static("Detail Texture",2D)="black"{}
		_XScroll("Scroll Speed (X)",float) = 0
		_YScroll("ScrollSpeed (Y)",float) = 2
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
	[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		BlendOp Max
		Blend One OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment SpriteFrag
			#pragma target 2.0
			#pragma multi_compile_instancing
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnityCG.cginc"
#ifdef UNITY_INSTANCING_ENABLED

			UNITY_INSTANCING_CBUFFER_START(PerDrawSprite)
		// SpriteRenderer.Color while Non-Batched/Instanced.
		fixed4 unity_SpriteRendererColorArray[UNITY_INSTANCED_ARRAY_SIZE];
	// this could be smaller but that's how bit each entry is regardless of type
	float4 unity_SpriteFlipArray[UNITY_INSTANCED_ARRAY_SIZE];
	UNITY_INSTANCING_CBUFFER_END

#define _RendererColor unity_SpriteRendererColorArray[unity_InstanceID]
#define _Flip unity_SpriteFlipArray[unity_InstanceID]

#endif // instancing

		CBUFFER_START(UnityPerDrawSprite)
#ifndef UNITY_INSTANCING_ENABLED
		fixed4 _RendererColor;
	float4 _Flip;
#endif
	float _EnableExternalAlpha;
	CBUFFER_END
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 stexcoord : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			fixed4 _Color;
			fixed _XScroll;
			fixed _YScroll;
			float4 _Static_ST;
			v2f SpriteVert(appdata_t IN)
			{
				v2f OUT;

				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

				#ifdef UNITY_INSTANCING_ENABLED
				IN.vertex.xy *= _Flip.xy;
				#endif
				fixed xScroll = _XScroll*_Time;
				fixed yScroll = _YScroll*_Time;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color * _RendererColor;
				OUT.stexcoord = TRANSFORM_TEX(IN.texcoord, _Static);
				OUT.stexcoord += fixed2(xScroll, yScroll);
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif

				return OUT;
			}
			sampler2D _MainTex;
			sampler2D _AlphaTex;
			sampler2D _Mask;
			sampler2D _Static;
			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);

				#if ETC1_EXTERNAL_ALPHA
				fixed4 alpha = tex2D(_AlphaTex, uv);
				color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
				#endif

				return color;
			}
			fixed4 SpriteFrag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				fixed4 msk= tex2D(_Mask, IN.texcoord);
				fixed4 st = tex2D(_Static, IN.stexcoord);
				c.a *= msk.a * 2;
				c.rgb *= c.a;
				c.rgb *= st.a;
				return c;
			}
			ENDCG
		}
	}
}