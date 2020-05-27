Shader "Shaders/Vitre"
{
	//Tesselation c'est un redécoupage en triangle => comme avec poly découpé en triangle(use algo)
	//sinon exemple avec Shader Tesselation

	//pour pouvoir modif dans material
	Properties
	{
		[Header(Shading)]
		_Color("Color", Color) = (1,1,1,1)
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5
		_CoefTaille("Taille",Range(1,10)) = 1.0
		_Scale("Scale",float) = 0.1
		
		//partie Toon
		[HDR]
		_AmbientColor("Ambient",Color) = (0.4,0.4,0.4,1)
		[HDR]
		_SpecularColor("Spec Color",Color) = (0.9,0.9,0.9,1)
		//ca c'est pour coef de Blinn Phong
		//coef quadratic
		_Glossiness("Glossiness",Float) = 32

		[HDR]
		_RimColor("Rim Color",Color) = (1,1,1,1)
		_RimAmount("Rim Amount",Range(0,1)) = 0.716
		_RimThreshold("Rim Threshold", Range(0,1)) = 0.1
	}

		//factorisation des données pour les différentes couches de traitemenet "Pass"
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Autolight.cginc"
		
		#define BLADE_SEGMENTS 3
		//var dans matériaux globales
		float4 _Color;
		float _CoefTaille;
		float _Scale;
		
		int _Transparency;

		//Toon var
		float4 _AmbientColor;
		float _Glossiness;
		float4 _SpecularColor;
		float4 _RimColor;
		float _RimAmount;
		float _RimThreshold;

		//repartition sur axe vertical

		static const float repartition[5] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };


		struct vertexInput
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			float4 uv : TEXCOORD0;
			uint id : SV_VertexID;
		};

		struct vertexOutput
		{
			float4 vertex : SV_POSITION;
			float4 tangent : TANGENT;
			float2 uv : TEXCOORD0;
			float3 viewDir : TEXTCOORD1;
			//world space
			float3 worldNormal : NORMAL;
			uint id : TEXCOORD2;
		};

		vertexOutput vert(vertexInput v)
		{
			vertexOutput o;

			
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.worldNormal = UnityObjectToWorldNormal(v.normal);
			//world space dir (not normalized) from given object vertex pos toward camera)
			//pour Blinn Phong
			o.viewDir = WorldSpaceViewDir(v.vertex);
			

			o.tangent = v.tangent;
			o.id = v.id;
			return o;
		}

		//rand Trauma
		float rand(float3 myVector) {
			return frac(sin(dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
		}

		//LOI de LAMBERT
		//I = N . L


		ENDCG

			SubShader{
			Pass
			{
				Tags
				{
					"LightMode" = "ForwardBase"
					"PassFlags" = "OnlyDirectional"
					"Queue" = "Transparent"
					"RenderType" = "Transparent"
				}
				//avec ZWrite On => abération chromatique avec décalage buffer
				Blend SrcAlpha OneMinusSrcAlpha
				Cull off
				//cull avec sens vertex rendu horaire ou anti horaire
				ZWrite off
				
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 4.6

				#include "Lighting.cginc"
				//#include "AutoLight.cginc"


				float4 frag(vertexOutput i) : SV_Target{

					float3 viewDir = normalize(i.viewDir);

					//Blinn Phong avec half vector entre viewDir and Light
					float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);

					float3 normal = normalize(i.worldNormal);
					float NdotL = dot(_WorldSpaceLightPos0, normal);
					float NdotH = dot(normal, halfVector);
					float lightIntensity = smoothstep(0, 0.01, NdotL);

					float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
					float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
					float4 specular = specularIntensitySmooth * _SpecularColor;

					//lightColor0 => comme texture => main directional light
					//present in Lighting.cginc
					float4 light = lightIntensity * _LightColor0;
					

					float4 res = _Color * (_AmbientColor + light + specular);
					res.a = _Color.a;
					return res;
				}

				ENDCG
			}
		}
}
