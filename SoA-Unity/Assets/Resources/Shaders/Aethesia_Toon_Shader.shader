Shader "Shaders/Aethesia_Toon_Shader"
{
	Properties
	{
		_Color("Color", Color) = (0.5, 0.65, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}
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
				SHADOW_COORDS(2)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _AmbientColor;
			float _Glossiness;
			float4 _SpecularColor;
			float4 _RimColor;
			float _RimAmount;
			float _RimThreshold;

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
				float3 viewDir = normalize(i.viewDir);

				//Blinn Phong avec half vector entre viewDir and Light
				float3 halfVector = normalize(_WorldSpaceLightPos0 + viewDir);

				float3 normal = normalize(i.worldNormal);
				float NdotL = dot(_WorldSpaceLightPos0, normal);
				float NdotH = dot(normal, halfVector);

				//on segment en deux l'intensité lumineuse en fonction du resultat du dot
				//float lightIntensity = NdotL > 0 ? 1 : 0;

				//return value between 0 and 1, no shadow 0 and 1 shadowed
				float shadow = SHADOW_ATTENUATION(i);
				//float shadow = 1.0f;
				//pour smooth edge
				//float lightIntensity = smoothstep(0, 0.01, NdotL);
				
				//float lightIntensity = smoothstep(0, 0.01f, NdotL * shadow);
				float lightIntensity = smoothstep(0, 0.5f, NdotL * shadow);


				float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
				//float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
				float4 specular = specularIntensitySmooth * _SpecularColor;

				//rim => defined as surfaces that are facing away from the camera
				//calculate the rim with the dot product of the normal and the view dir
				//and opposé it 1/res

				float4 rimDot = 1 - dot(viewDir, normal);

				//float rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimDot);
				//float rimIntensity = rimDot * NdotL;
				//ici on va scale le rim
				float rimIntensity = rimDot * pow(NdotL,_RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);

				float4 rim = rimIntensity * _RimColor;

				//lightColor0 => comme texture => main directional light
				//present in Lighting.cginc
				float4 light = lightIntensity * _LightColor0;
				
				//smoothstep => smooth transition between 0 and 1
				//pivot
				//clamping interpolation => tronqué/arrondi pour le clamp (avec segment)
				//avec interpolation linéaire avec cette valeur
				//en gros on le garde d'un intervalle choisi en le remontant ou le baissant puis on interpole
				//exemple => entre 0 et 1 => 0
				//clamp si inf 0.25 = 0.25, is sup 0.75 = 0.75 return value
				//interpolation entre 0 et 1 => borne_max - value / (borne_max - borne_min)
				//après évaluation polynomial du resultat
				//donc fonction non linéaire => car clampe dans un intervalle inf a celui du depart
				
				//dont en 0 90 a droite ou a gauche en +
				//ou en bas 90 < degree < 270
				//more segment => on deplace l'ensemble d'intersection entre 0 et 1
				//complément du dot entre 0 et 1 au milieu
				//avec une texture degradée et on cherche avec uv coord
				//float2 uv = float2(1 - (NdotL * 0.5 + 0.5), 0.5);
				
				//rim = 0.0f;

				//add ambient color
				//return _Color * sample * (_AmbientColor + lightIntensity);
				//light de la scène
				//return _Color * sample * (_AmbientColor + light + specular + rimDot);
				return sample * (_AmbientColor + light + specular + rim);
			}
			ENDCG
		}
		//after a Pass UsePass => grabs a pass from different shader and inserts into our shader
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}
