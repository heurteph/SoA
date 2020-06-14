Shader "Shaders/Esthesia_Shader"
{
	Properties
	{
		_Color("Color", Color) = (0.5, 0.65, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}

		_ShadowPercentColor("Shadow Pourcent Color",Range(0.0,0.5)) = 0.01
		_ShadowStrenght("Shadow Strenght",Range(0.0,1.0)) = 0.5
		_ColorShadow("Color of Shadow",Color) = (1.0,1.0,1.0)

		_AmbientLevel("Ambient Level",Range(0.0,1.0)) = 0.4

		//ca c'est pour coef de Blinn Phong
		//coef quadratic
		_Glossiness("Glossiness",Float) = 32
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
			float3 _ColorShadow;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				//world space dir (not normalized) from given object vertex pos toward camera)
				//pour Blinn Phong
				o.viewDir = WorldSpaceViewDir(v.vertex);
				//transfrom the input vertex's space to the shadow map's space, and stores it in the SHADOW_COORD we declared
				TRANSFER_SHADOW(o);
				return o;
			}


			float4 frag(v2f i) : SV_Target
			{
				float4 sample = tex2D(_MainTex, i.uv);
				float4 flat = sample;
				float3 viewDir = normalize(i.viewDir);

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
					light.rgb *= float3(0.62f, 0.81f, 1.0f);//_ColorShadow.rgb;
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
