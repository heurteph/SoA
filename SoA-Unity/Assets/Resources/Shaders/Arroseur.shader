Shader "Shaders/Arroseur"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MaskTex("Mask texture", 2D) = "white" {}
		_maskBlend("Mask blending", Float) = 0.5
		_maskSize("Mask Size", Float) = 1
		_color("Color",Color) = (0.0,0.0,0.0,1.0)
		_Position("Position",Vector) = (0.0,0.0,0.0,0.0)
		_ecart("Ecart", Float) = 0.4
		_height("Hauteur",Float) = 10.0
		_PourcentHeight("Height display",Range(0.0,1.0)) = 1.0
		_Fq("Frequence arrosage",Float) = 1.0
	}
		//Bien selectionner dans Shader RenderQueue Transparent
		SubShader{
			Pass{
			Tags
				{
					"Queue" = "Transparent"//Cutout"
					//"Queue" = "AlphaTest"
					"RenderType" = "Transparent"//Cutout"
					//to render to normal buffer
					//"LightMode" = "ForwardBase"
					"IgnoreProjector" = "True"
				}

			//ordre très important
			//Ztest always
			ZWrite Off
			Cull off
			Blend SrcAlpha OneMinusSrcAlpha
			//AlphaToMask On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geo

			#include "UnityCG.cginc"

			//float total_grey;
			//groupshared float tab[1];
		//ca partagé
		shared float res = 0.0f;

		uniform sampler2D _MainTex;
		uniform sampler2D _MaskTex;
		uniform float type;
		uniform float time;
		uniform float height;
		uniform float width;
		uniform float4 _Position;
		uniform float4 _Pivot;
		uniform float _Fq;

		uniform float _DistanceMax;

		uniform float _ecart;
		uniform float _height;
		uniform float4 _color;
		uniform float _PourcentHeight;
		uniform float height_scale;


		//pour composante Main_Text
		float4 _MainTex_ST;


		struct vertexInput {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			half2 uv : TEXCOORD0;
			uint id : SV_VertexID;
		};

		struct vertexOutput{
			float4 vertex : SV_POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			half2 uv : TEXCOORD0;
			uint id : TEXCOORD1;
			bool up : TEXCOORD2;
			float distance : DISTANCE;
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
			//pas encore besoin des coord dans 
			if (v.vertex.y >= _height) {
				o.up = true;
			}
			else {
				o.up = false;
			}
			//o.vertex = UnityObjectToClipPos(v.vertex);
			//unity_ObjectToWorld
			o.vertex = mul(unity_ObjectToWorld, v.vertex);
			//o.vertex = v.vertex;
			o.distance = 0.0f;
			o.id = v.id;
			o.normal = v.normal;
			o.tangent = v.tangent;
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			
			//v.uv;

			//UNITY_TRANSFER_FOG(o, o.pos);
			return o;
		}

		
		//void geo(triangle vertexOuput IN[3], inout TriangleStream<geometryOutput> triStream) {
		[maxvertexcount(3)]
		void geo(triangle vertexOutput IN[3] : SV_POSITION, inout TriangleStream<vertexOutput> triStream) {
			geometryOutput o;
			vertexOutput vo;

			

			float fq = sin(_Time.y) / 2.0f + 0.5f;

			//float4 Pivot = UnityObjectToClipPos(_Pivot);

			float time = _Fq * _Time.y;

			//float3 Pivot = float3(_Pivot.x, _Pivot.y, _Pivot.z);
			float3 Pivot = float3(0.0f, 0.0f, 0.0f);

			vo = IN[0];//UnityObjectToClipPos(I);
			vo.distance = length(vo.vertex.xyz - _Pivot.xyz) / (_DistanceMax *2.0f);
			float3 tmp = vo.vertex.xyz - Pivot;
			//tmp.x = tmp.x * cos(time) + tmp.z * sin(time);
			//tmp.z = tmp.x * -sin(time) + tmp.z * cos(time);
			vo.vertex = float4(tmp.xyz + Pivot, vo.vertex.w);
			
			vo.vertex = mul(UNITY_MATRIX_VP,vo.vertex);
			/*vo.distance = length(vo.vertex.xyz - _Pivot.xyz)/_DistanceMax;
			float3 tmp = vo.vertex.xyz - _Pivot.xyz;
			tmp.x = tmp.x * cos(_Time.y) + tmp.z * sin(_Time.y);//float3(sin(_Time.y)+tmp.x, tmp.y, sin(_Time.y) + tmp.z);
			tmp.z = tmp.x * -sin(_Time.y) + tmp.z * cos(_Time.y);
			vo.vertex = float4(tmp.xyz, vo.vertex.w);//float4(tmp.xyz + _Pivot.xyz, vo.vertex.w);*/
			//vo.vertex = mul(UNITY_MATRIX_VP, vo.vertex); //UnityObjectToClipPos(vo.vertex);
			triStream.Append(vo);
			
			vo = IN[1];
			vo.distance = length(vo.vertex.xyz - _Pivot.xyz) / (_DistanceMax *2.0f);
			tmp = vo.vertex.xyz - Pivot;
			//tmp.x = tmp.x * cos(time) + tmp.z * sin(time);
			//tmp.z = tmp.x * -sin(time) + tmp.z * cos(time);
			vo.vertex = float4(tmp.xyz + Pivot, vo.vertex.w);
			vo.vertex = mul(UNITY_MATRIX_VP, vo.vertex);
			/*vo.distance = length(vo.vertex.xyz - _Pivot.xyz) / _DistanceMax;
			tmp = vo.vertex.xyz - _Pivot.xyz;
			tmp.x = tmp.x * cos(_Time.y) + tmp.z * sin(_Time.y);//float3(sin(_Time.y)+tmp.x, tmp.y, sin(_Time.y) + tmp.z);
			tmp.z = tmp.x * -sin(_Time.y) + tmp.z * cos(_Time.y);
			vo.vertex = float4(tmp.xyz, vo.vertex.w);//float4(tmp.xyz + _Pivot.xyz, vo.vertex.w);*/
			//vo.vertex = mul(UNITY_MATRIX_VP, vo.vertex);//UnityObjectToClipPos(vo.vertex);
			triStream.Append(vo);

			vo = IN[2];
			vo.distance = length(vo.vertex.xyz - _Pivot.xyz) / (_DistanceMax *2.0f);
			tmp = vo.vertex.xyz - Pivot;
			//tmp.x = tmp.x * cos(time) + tmp.z * sin(time);
			//tmp.z = tmp.x * -sin(time) + tmp.z * cos(time);
			vo.vertex = float4(tmp.xyz + Pivot, vo.vertex.w);
			vo.vertex = mul(UNITY_MATRIX_VP, vo.vertex);
			/*vo.distance = length(vo.vertex.xyz - _Pivot.xyz) / _DistanceMax;
			tmp = vo.vertex.xyz - _Pivot.xyz;
			tmp.x = tmp.x * cos(_Time.y) + tmp.z * sin(_Time.y);//float3(sin(_Time.y)+tmp.x, tmp.y, sin(_Time.y) + tmp.z);
			tmp.z = tmp.x * -sin(_Time.y) + tmp.z * cos(_Time.y);
			vo.vertex = float4(tmp.xyz, vo.vertex.w);//float4(tmp.xyz + _Pivot.xyz, vo.vertex.w);*/
			//vo.vertex = mul(UNITY_MATRIX_VP, vo.vertex);//UnityObjectToClipPos(vo.vertex);
			triStream.Append(vo);
		}

		//loi normal pour rendu lumière
		//avec transparence
		//sur un seul vertex

		fixed4 frag(vertexOutput im) : COLOR{
			/*fixed4 mask = tex2D(_MaskTex, im.uv * _maskSize);
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
			col = float4(col.r * base.r, col.g * base.g, col.b * base.b, 1.0f);*/
			
			float4 col = _color;//float4(0.0f,0.0f,1.0f,0.5f);
			
			float taille = (height_scale * 2.0f);

			float uv = taille - im.uv.y;
			

			
			int time = (int)_Time.w;

			float fq = sin(_Time.y) / 2.0f + 0.5f;
			


			if (rand(float3(im.uv.xy, _Time.y)) < 0.5f) {
				discard;
			}
			
			//col.a = uv;

			//col = lerp(float4(1.0f, 1.0f, 1.0f, _color.a), _color, sin(_Time.y) * im.distance);

			/*if (im.uv.x < area || im.uv.x >(1.0f - area) || im.uv.y < area || im.uv.y >(1.0f - area)) {
				discard;
			}*/
			/*float area = ((1.0f - fq) / 2.0f);
			
			if (im.uv.x < area || im.uv.y < area) {
				if(im.uv.x < area)
					col.rgb = lerp(float3(1.0f,1.0f,1.0f),col.rgb,area-im.uv.x);
				if(im.uv.y < area)
					col.rgb = lerp(float3(1.0f, 1.0f, 1.0f), col.rgb, area - im.uv.y);
			}
			else if (im.uv.x >(1.0f - area) || im.uv.y >(1.0f - area)) {
				if(im.uv.x > (1.0f - area))
					col.rgb = lerp(float3(1.0f, 1.0f, 1.0f), col.rgb, im.uv.x - (1-area));
				if(im.uv.y > (1.0f - area))
					col.rgb = lerp(float3(1.0f, 1.0f, 1.0f), col.rgb, im.uv.y - (1 - area));
			}*/



			//float tmp = (tex2D(_MainTex, uv).x + tex2D(_MainTex, uv).y + tex2D(_MainTex, uv).z)/3.0f;
			
			/*float seuil = taille * _PourcentHeight;
			float alpha = im.uv.y / (taille - (taille - seuil));
			alpha = (alpha >= 0 && alpha <= 1.0f ? alpha : 0.0f);*/
			
			/*if (im.uv.y >= seuil) {
				discard;
			}*/
			//col.a = alpha;

			return col;
		}
		ENDCG
	}
	}
}
