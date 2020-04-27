Shader "Shaders/GrassPlaneShader"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MaskTex("Mask texture", 2D) = "white" {}
		_maskBlend("Mask blending", Float) = 0.5
		_maskSize("Mask Size", Float) = 1
		_Color("Color",Color) = (0.24,0.69,0.55,1.0)
	}
		SubShader{
			//ordre face impacté par geoshader ?
			//rendu avec ordre point 1 -> 2 -> 3 etc...
			Cull Off
			Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

		uniform sampler2D _MainTex;
		uniform sampler2D _MaskTex;



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
		float4 _Color;

		float rand(float3 myVector) {
			return frac(sin(dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
		}

		

		
		//v2f_img vert(float4 pos : POSITION,	float2 uv : TEXCOORD0)
		vertexOutput vert(vertexInput v)
		{
			vertexOutput o;
			//sans clipping tout l'espace car non clip en localToWorld
			//o.vertex = v.vertex;//UnityObjectToClipPos(v.vertex);
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.normal = v.normal;
			o.tangent = v.tangent;
			o.uv = v.uv;
			return o;
		}


		fixed4 frag(vertexOutput im) : COLOR{
			fixed4 mask = tex2D(_MaskTex, im.uv * _maskSize);
			fixed4 base = tex2D(_MainTex, im.uv);
			
			return _Color;
		}
		ENDCG
	}
	}
}
