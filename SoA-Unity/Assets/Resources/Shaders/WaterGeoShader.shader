Shader "Shaders/WaterGeoShader"
{
	//Tesselation c'est un redécoupage en triangle => comme avec poly découpé en triangle(use algo)
	//sinon exemple avec Shader Tesselation

	//pour pouvoir modif dans material
	Properties
	{
		[Header(Shading)]
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5
		_CoefTaille("Taille",Range(1,10)) = 1.0
		_Scale("Scale",float) = 0.1
		_Transparency("Transparency",Range(0,1)) = 1.0
		_Amplitude("Amplitude",Range(0,10)) = 0.25
		_FrequenceEcume("Fq ecume temps",Range(0,80)) = 4.0

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

		//autour de l'axe x sur elle melle
		//var présente dans fichier tess shader
		//attention au nom et pas oublier de lettres......
		_TessellationUniform("Tessellation", Range(1,64)) = 1
		_WindDistoMap("Wind Disto Map", 2D) = "white"{}
		_WindFq("Wind Frequence",Vector) = (0.05,0.05,0,0)
		_WindStrength("Wind Strength",Float) = 1
		_DynamicRender("Dynamic mode",int) = 1
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
		sampler2D _WindDistoMap;
		float4 _WindDistoMap_ST;
		float2 _WindFq;
		float _WindStrength;
		float _Amplitude;
		float _FrequenceEcume;
		
		float _TessellationUniform;
		float distance;
		int _DynamicRender;
		int _Transparency;

		//Toon var
		sampler2D _MainTex;
		float4 _MainTex_ST;
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
			/*bool ecume : TEXCOORD3;
			float pos_ecume : TEXCOORD4;
			float segment : TEXCOORD5;*/
		};

		struct TessellationFactors
		{
			float edge[3] : SV_TessFactor;
			float inside : SV_InsideTessFactor;
		};

		struct geometryOutput {
			//mot a droite pour sémantic
			float4 pos : SV_POSITION;
			//niveau de texture utilisé comme dans OGL
			float2 uv : TEXCOORD0;
			//unityShadowCoord4 _ShadowCoord : TEXCOORD1;
			float3 normal : NORMAL;
		};

		geometryOutput VertexOutput(float3 pos, float2 uv,float3 normal) {
			geometryOutput o;
			o.pos = UnityObjectToClipPos(pos);
			o.uv = uv;
			o.normal = normal;
			return o;
		}

		vertexOutput tessVert(vertexInput v)
		{
			vertexOutput o;
			// Note that the vertex is NOT transformed to clip
			// space here; this is done in the grass geometry shader.
			o.vertex = v.vertex;
			o.worldNormal = v.normal;
			o.tangent = v.tangent;
			o.id = v.id;
			return o;
		}

		//ecume => idée avec loi normale centrée réduite 
		//autour d'un point en y qui évolue de 0 a 100 de maniere périodique 
		//ajustement fréquence
		//génération de point aléatoire a cette hauteur => en rand avec répartition de loi normale
		//donc récupération de l'id => avec bande de segment
		//0 10 -> 11 20 -> 21 30 -> 31 40

		vertexOutput vert(vertexInput v)
		{
			vertexOutput o;

			
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.worldNormal = UnityObjectToWorldNormal(v.normal);
			//world space dir (not normalized) from given object vertex pos toward camera)
			//pour Blinn Phong
			o.viewDir = WorldSpaceViewDir(v.vertex);
			

			//ECUME géré ici pour économie de calcul
			//pour repérer si dans le bloc a dessiner
			//float time = (_Time - ((_Time / _FrequenceEcume) * _FrequenceEcume))/ _FrequenceEcume * 100.0;
			/*o.ecume = false;
			//_Time => float4 avec t/20, t, 
			float time = _Time.y - (((int)(_Time.y / _FrequenceEcume)) * _FrequenceEcume);
			//comme ca on l'a sur 100
			time = (time / _FrequenceEcume) * 100.0;

			//comme quadrillage de 10*10
			//int segment = (int)(v.id / 10.0);
			//on va tester avec espace model pour decaler le point de pivot, plus au centre
			//comme redimenionner :o => avec le scale
			//par rapport a la dimension de la taille surement :o
			float x = v.vertex.x + (4.9 / _Scale);// + (10.0 / 2.0);
			int value = (int)(x / 10.0);
			o.segment = value;
			value *= 10.0;


			*/
			//o.pos_ecume = (int)(time - value);
			//o.pos_ecume = 0;
			//o.pos_ecume = (int)(time - value);
			
			/*if (time >= value && time < (value + 10)) {
				o.ecume = true;
				//pos de 0 a 1
				//au dixieme
				o.pos_ecume = (int)(time - value);// / 10.0;
				//au centieme
				//o.pos_ecume = (int)((time - value)*10);

				//if (o.pos_ecume < 10.0 && o.pos_ecume >= 0) {
				//	o.pos_ecume = 0;
				//}
				//o.pos_ecume = 0;
				//o.pos_ecume = 0;
				//on tronque au dixieme
				//o.pos_ecume = ((int)(o.pos_ecume * 10.0)) / 10.0f;
			}*/

			o.tangent = v.tangent;
			o.id = v.id;
			return o;
		}

		TessellationFactors patchConstantFunction(InputPatch<vertexInput, 3> patch)
		{
			TessellationFactors f;
			f.edge[0] = _TessellationUniform;
			f.edge[1] = _TessellationUniform;
			f.edge[2] = _TessellationUniform;
			f.inside = _TessellationUniform;
			return f;
		}

		[UNITY_domain("tri")]
		[UNITY_outputcontrolpoints(3)]
		[UNITY_outputtopology("triangle_cw")]
		[UNITY_partitioning("integer")]
		[UNITY_patchconstantfunc("patchConstantFunction")]
		vertexInput hull(InputPatch<vertexInput, 3> patch, uint id : SV_OutputControlPointID)
		{
			return patch[id];
		}

		[UNITY_domain("tri")]
		vertexOutput domain(TessellationFactors factors, OutputPatch<vertexInput, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
		{
			vertexInput v;

#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) v.fieldName = \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z;

			MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
				MY_DOMAIN_PROGRAM_INTERPOLATE(normal)
				MY_DOMAIN_PROGRAM_INTERPOLATE(tangent)

				return tessVert(v);
		}

		//rand Trauma
		float rand(float3 myVector) {
			return frac(sin(dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
		}


		//calcul d'une matrice pour angle
		//autour d'un axe pour nous axe z 0 0 1
		//avec angle aléa entre 0 et 2PI
		float3x3 AngleAxis3x3(float angle, float3 axis)
		{
			float c, s;
			//angle => return sin et cos de cet anagle
			sincos(angle, s, c);

			//reste du cos sur PI
			float t = 1 - c;
			float x = axis.x;
			float y = axis.y;
			float z = axis.z;

			return float3x3(
				t * x * x + c, t * x * y - s * z, t * x * z + s * y,
				t * x * y + s * z, t * y * y + c, t * y * z - s * x,
				t * x * z - s * y, t * y * z + s * x, t * z * z + c
				);
		}

		
		[maxvertexcount(3)]
		void geo(triangle vertexOutput IN[3], inout TriangleStream<vertexOutput> triStream) {
			
			geometryOutput o;
			float4 a = IN[0].vertex - IN[1].vertex;
			float4 b = IN[2].vertex - IN[1].vertex;
			float3 normal = -normalize(cross(a, b)).rgb;
			float4 pos;

			float4 tmp;
			pos = IN[0].vertex;
			float3 vNormal = IN[0].worldNormal;
			//ICI ON VA UTILISER POUR BITANGENT SPACE POUR TRANSFORMATION LOCAL DANS LE WORLD
			float4 vTangent = IN[0].tangent;
			//cross product tangent and normal multilply by the direction here stocked by unity in w homogeneus data to lightened mem usage
			float3 vBinormal = cross(vNormal, vTangent) * vTangent.w;



			float3x3 tangentToLocal = float3x3(
				vTangent.x, vBinormal.x, vNormal.x,
				vTangent.y, vBinormal.y, vNormal.y,
				vTangent.z, vBinormal.z, vNormal.z
			);

			for (int i = 0; i < 3; i++) {
				pos = IN[i].vertex;
				//ici avec 0.5 => pour mettre sur 0 1
				pos.y += cos(_WindStrength*(_Time.y+IN[i].id))*_Amplitude + _Amplitude;
				IN[i].vertex = pos;
				triStream.Append(IN[i]);
				//triStream.Append(VertexOutput(pos, float2(0,0),normal));
			}
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
					/*"Queue" = "Transparent"
					"RenderType" = "Transparent"*/
				}
				//avec ZWrite On => abération chromatique avec décalage buffer
				//Blend SrcAlpha OneMinusSrcAlpha
				//ZWrite off
				
				CGPROGRAM
				#pragma vertex vert
				#pragma geometry geo
				#pragma fragment frag
				//gérer dans TESSELATION SHADER
				/*#pragma hull hull
				#pragma domain domain
				//directiv to compile all necessary shader variants
				#pragma multi_compile_fwdbase*/
				#pragma target 4.6

				#include "Lighting.cginc"
				//#include "AutoLight.cginc"


				float4 frag(vertexOutput i) : SV_Target{

					/*float4 col;
					
					col = _Color;
					col.a = 1;
					return col;*/

					float4 sample = tex2D(_MainTex, i.uv);
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

					//rim => defined as surfaces that are facing away from the camera
					//calculate the rim with the dot product of the normal and the view dir
					//and opposé it 1/res

					float4 rimDot = 1 - dot(viewDir, normal);

					//float rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimDot);
					//float rimIntensity = rimDot * NdotL;
					//ici on va scale le rim
					float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
					rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);

					float4 rim = rimIntensity * _RimColor;

					//lightColor0 => comme texture => main directional light
					//present in Lighting.cginc
					float4 light = lightIntensity * _LightColor0;
					

					//return _Color * (_AmbientColor + light + specular + rim);
					return _Color * sample * (_AmbientColor + light + specular + rim);
				}

				ENDCG
			}
			//second Pass to render Shadow to Shadow map => only main Directional Light
			/*Pass
			{
				Tags
				{
					//to render to ShadowMaps buffer
					"LightMode" = "ShadowCaster"
				}
				CGPROGRAM
				//preprocessor directivs as openmp
				#pragma vertex vert
				#pragma fragment frag
				#pragma hull hull
				#pragma domain domain
				#pragma target 4.6
				//compile all necessary variants required for shadow casting
				#pragma multi_compile_shadowcaster

				float4 frag(geometryOutput i) : SV_Target
				{
					SHADOW_CASTER_FRAGMENT(i)
				}
				ENDCG
			}*/
		}
}
