Shader "Shaders/GeoShader"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MaskTex("Mask texture", 2D) = "white" {}
		_maskBlend("Mask blending", Float) = 0.5
		_maskSize("Mask Size", Float) = 1
	}
		SubShader{
			Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geo
			#include "UnityCG.cginc"

			//float total_grey;
			//groupshared float tab[1];
		shared float res = 0.0f;	
		uniform sampler2D _MainTex;
		uniform sampler2D _MaskTex;
		uniform float type;
		uniform float time;
		uniform float height;
		uniform float width;


		struct vertexInput {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			half2 uv : TEXCOORD0;
		};

		struct vertexOutput{
			float4 vertex : SV_POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			half2 uv : TEXCOORD0;
		};

		struct geometryOutput{
			float4 pos : SV_POSITION;
		};

		/*geometryOuput VertexOuput(float3 pos) {
			geometryOutpur o;
			o.pos = UnityObjectToClipPos(pos);
			return o;
		}*/

		fixed _maskBlend;
		fixed _maskSize;

		float rand(float3 myVector) {
			return frac(sin(dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
		}

		//const ne fonctionne pas !!!!!!!

		//static partout !!!!!!
		static float offset = 1.0f / 300.0f;

		//static partout !!!!!!
		static half2 offsets[9] = { half2(-offset, offset), half2(0.0f, offset), half2(offset, offset),
		half2(-offset, 0.0f), half2(0.0f, 0.0f), half2(offset, 0.0f),
		half2(-offset, -offset), half2(0.0f, -offset), half2(offset, -offset)};

		//static partout !!!!!!
		static float kernel[] = { 1.0f,1.0f,1.0f,1.0f,-8.0f,1.0f,1.0f,1.0f,1.0f };

		static float kernel2[] = { -1.0f,-1.0f,-1.0f,-1.0f,8.0f,-1.0f,-1.0f,-1.0f,-1.0f };

		static float kernelCell[] = { -1.0f,-2.0f,-1.0f,0.0f,0.0f,0.0f,1.0f,2.0f,1.0f };


		/*v2f_img vert(appdata v)
		{
			v2f_img o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			//UNITY_TRANSFER_FOG(o, o.pos);
			return o;
		}*/

		//v2f_img vert(float4 pos : POSITION,	float2 uv : TEXCOORD0)
		vertexOutput vert(vertexInput v)
		{
			vertexOutput o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.normal = v.normal;
			o.tangent = v.tangent;
			o.uv = v.uv;//TRANSFORM_TEX(uv, _MainTex);
			//UNITY_TRANSFER_FOG(o, o.pos);
			return o;
		}

		
		//void geo(triangle vertexOuput IN[3], inout TriangleStream<geometryOutput> triStream) {
		[maxvertexcount(3)]
		void geo(triangle vertexOutput IN[3] : SV_POSITION, inout TriangleStream<vertexOutput> triStream) {
			geometryOutput o;
			//o.pos = IN[0].vertex;//float4(0.5f, 0, 0, 1);
			triStream.Append(IN[0]);

			o.pos = IN[1].vertex;//float4(-0.5f, 0, 0, 1);
			triStream.Append(IN[1]);

			o.pos = IN[2].vertex;//float4(0, 1, 0, 1);
			triStream.Append(IN[2]);
		}


		fixed4 frag(vertexOutput im) : COLOR{
			fixed4 mask = tex2D(_MaskTex, im.uv * _maskSize);
			fixed4 base = tex2D(_MainTex, im.uv);
			int i;
			float4 sampleTex[9];
			for (i = 0; i < 9; i++) {
				float2 uv = im.uv;
				uv.x += offsets[i].x;
				uv.y += offsets[i].y;
				sampleTex[i] = tex2D(_MainTex, uv);
			}

			float4 col = float4(0.0f, 0.0f, 0.0f, 1.0f);
			for (i = 0; i < 9; i++) {
				col += (sampleTex[i] * kernel[i]);
			}

			base = saturate(base);
			col = float4(1.0f - col.r, 1.0f - col.g, 1.0f - col.b, 1.0f);
			col = float4(col.r * base.r, col.g * base.g, col.b * base.b, 1.0f);
			

			return col;		
		}
		ENDCG
	}
	}
}
