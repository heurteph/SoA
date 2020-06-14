Shader "Shaders/Duck"
{
	Properties
	{
		_Color("Color", Color) = (0.5, 0.65, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}
		[HDR]
		_AmbientColor("Ambient",Color) = (0.4,0.4,0.4,1)
		[HDR]
		_SpecularColor("Spec Color",Color) = (0.9,0.9,0.9,1)

		_ShadowPercentColor("Shadow Pourcent Color",Range(0.0,0.5)) = 0.01
		_ShadowStrenght("Shadow Strenght",Range(0.0,1.0)) = 0.5
		
		_AmbientLevel("Ambient Level",Range(0.0,1.0)) = 0.4

		_Amplitude("Amplitude",Range(0,10)) = 0.25
		_OffSetFactor("Factor OffSet",Range(0.0,0.5)) = 0.01
		_WindStrength("Wind Strength",Float) = 1
		
		_Glossiness("Glossiness",Float) = 32
		[HDR]
		_RimColor("Rim Color",Color) = (1,1,1,1)
		_RimSmooth("Rim Smooth",Range(0.0,1.0)) = 0.0
		_RimAmount("Rim Amount",Range(0.700,1.0)) = 0.716
		_RimThreshold("Rim Threshold", Range(0,1)) = 0.1
	}
		SubShader
	{
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geo
			//directiv => to compile
			#pragma multi_compile_fwdbase

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			//contains severals macros => sample shadows
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				//object space
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				//comme d'hab différent niveau de texture
				float2 uv : TEXCOORD0;
				float3 viewDir : TEXTCOORD1;
				//world space
				float3 worldNormal : NORMAL;
				//generates a 4dim value with varying procision (depending on the target platform)
				//and assigns it ti the TEXCOORD semantic (c2 in our case
				//car TEXTCOORDS 2 autres non dispo
				SHADOW_COORDS(2)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _AmbientColor;
			float _AmbientLevel;
			float _Glossiness;
			float4 _SpecularColor;
			float _ShadowPercentColor;
			float _ShadowStrenght;
			float4 _RimColor;
			float _RimAmount;
			float _RimSmooth;
			float _RimThreshold;
			float _OffSetFactor;
			float _WindStrength;
			float _Amplitude;
			float _Id;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = mul(unity_ObjectToWorld, v.vertex);//o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				//world space dir (not normalized) from given object vertex pos toward camera)
				//pour Blinn Phong
				o.viewDir = WorldSpaceViewDir(v.vertex);
				//transfrom the input vertex's space to the shadow map's space, and stores it in the SHADOW_COORD we declared
				TRANSFER_SHADOW(o);
				return o;
			}

			[maxvertexcount(3)]
			void geo(triangle v2f IN[3], inout TriangleStream<v2f> triStream) {
				float4 pos;

				float2 offSet = float2(cos(_Id + _Time.y)*_OffSetFactor, sin(_Id + _Time.y)*_OffSetFactor);

				for (int i = 0; i < 3; i++) {
					pos = IN[i].pos;
					pos.x += offSet.x;
					pos.z += offSet.y;
					pos.y += cos(_WindStrength*(_Time.y + _Id))*_Amplitude + _Amplitude;
					IN[i].worldNormal = UnityObjectToWorldNormal(IN[i].worldNormal);
					//IN[i].vertex = UnityObjectToClipPos(pos);
					IN[i].pos = mul(UNITY_MATRIX_VP, pos);
					TRANSFER_SHADOW(IN[i]);
					triStream.Append(IN[i]);
				}
			}


			float4 frag(v2f i) : SV_Target
			{
				float4 sample = tex2D(_MainTex, i.uv);
				float4 flat = sample;
				float3 viewDir = normalize(i.viewDir);

				//Blinn Phong avec half vector entre viewDir and Light
				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);

				float3 normal = normalize(i.worldNormal);
				float NdotL = dot(_WorldSpaceLightPos0, normal);
				float NdotH = dot(normal, halfVector);
				float shadow = SHADOW_ATTENUATION(i);


				float lightIntensity = smoothstep(0, 0.01, NdotL * shadow);

				//_Glossiness => a 32 : de base 0.5f

				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				float4 specular = specularIntensity * _SpecularColor;

				if (specularIntensity < 0.5) {
					specular = float4(0.01f, 0.01f, 0.01f, 1.0f) * _SpecularColor;
				}
				else {
					specular = float4(1.0f, 1.0f, 1.0f, 1.0f) * _SpecularColor;
				}


				float4 rimDot = 1 - dot(viewDir, normal);


				float rimIntensity = rimDot * pow(NdotL,_RimThreshold);
				rimIntensity = smoothstep(_RimAmount - _RimSmooth, _RimAmount + _RimSmooth, rimIntensity);

				float4 rim = rimIntensity * _RimColor;

				float4 light = lightIntensity * _LightColor0;


				if (lightIntensity < 0.5) {
					light = float4(_ShadowPercentColor, _ShadowPercentColor, _ShadowPercentColor, 1.0f) * _LightColor0;
					float tmp = max((_AmbientLevel - _ShadowStrenght), 0.0f);
					_AmbientColor = float4(tmp, tmp, tmp, 1.0f);
				}
				else {
					_AmbientColor = float4(_AmbientLevel, _AmbientLevel, _AmbientLevel, _AmbientLevel);
				}


				_AmbientColor *= UNITY_LIGHTMODEL_AMBIENT;

				sample *= (_AmbientColor + light);

				return sample;
			}
			ENDCG
		}
		//after a Pass UsePass => grabs a pass from different shader and inserts into our shader
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}
