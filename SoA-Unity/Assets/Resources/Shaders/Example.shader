Shader "Unlit/Example"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		CGINCLUDE
			#include "UnityCG.cginc"
			#include "Autolight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2g
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};
			struct g2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				unityShadowCoord4 _ShadowCoord : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2g vert(appdata v)
			{
				v2g o;

				//move my verts
				float4 position = v.vertex;
				position.xz *= 2 - (abs(sin(25.13 * v.uv.x)));

				o.vertex = position;
				o.normal = v.normal;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			[maxvertexcount(3)]
			void geom(triangle v2g input[3], inout TriangleStream&lt; g2f > triStream)
			{
				g2f o;
				float3 normal = normalize(cross(input[1].vertex - input[0].vertex, input[2].vertex - input[0].vertex));

				for (int i = 0; i &lt; 3; i++)
				{
					float4 vert = input[i].vertex;
					o.vertex = UnityObjectToClipPos(vert);
					UNITY_TRANSFER_FOG(o,o.vertex);
					o.uv = input[i].uv;
					o.normal = UnityObjectToWorldNormal((normal));
					o._ShadowCoord = ComputeScreenPos(o.vertex);
					#if UNITY_PASS_SHADOWCASTER
					o.vertex = UnityApplyLinearShadowBias(o.vertex);
					#endif
					triStream.Append(o);
				}

				triStream.RestartStrip();
			}
		ENDCG

		Pass
		{
			Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase"}
			LOD 100
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase
			#pragma shader_feature IS_LIT

			fixed4 frag(g2f i) : SV_Target
			{
			// orangy color
			fixed4 col = fixed4(0.9,0.7,0.1,1);
		//lighting
		fixed light = saturate(dot(normalize(_WorldSpaceLightPos0), i.normal));
		float shadow = SHADOW_ATTENUATION(i);
		col.rgb *= light * shadow + float4(ShadeSH9(float4(i.normal, 1)), 1.0);
		// apply fog
		UNITY_APPLY_FOG(i.fogCoord, col);
		return col;
	}
	ENDCG
}

Pass
{
Tags { "RenderType" = "Opaque" "LightMode" = "ShadowCaster" }
LOD 100
CGPROGRAM
	#pragma vertex vert
	#pragma geometry geom
	#pragma fragment fragShadow
	#pragma target 4.6
	#pragma multi_compile_shadowcaster
	float4 fragShadow(g2f i) : SV_Target
	{
		SHADOW_CASTER_FRAGMENT(i)
	}
ENDCG
}
	}
}