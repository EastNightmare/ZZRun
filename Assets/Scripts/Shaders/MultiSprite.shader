Shader "Unlit/MultiSprite"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_XTile ("Tile X", float) = 1
		_YTile ("Tile Y", float) = 1
		_YOffset ("Y Offset", float) = 0.7
	}
	SubShader
	{
		Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="true" }
		LOD 100
		cull off
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
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _XTile;
			float _YTile;
			float _YOffset;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float2 uv = v.uv;
				o.uv = TRANSFORM_TEX(uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float y = i.uv.y;
				i.uv = float2(i.uv.x * _XTile, i.uv.y * _YTile);
				i.uv = float2(fmod(i.uv.x, 1.0), fmod((fmod(i.uv.y, 1.0) + (1 - fmod(_YTile, 1.0))), 1.0));
				i.uv.y = y < (1 - ((1 - _YOffset) / _YTile))  ? fmod(i.uv.y, _YOffset) : i.uv.y;
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
