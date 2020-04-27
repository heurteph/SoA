Shader "Shaders/GrassGeoShader"
{
	//Tesselation c'est un redécoupage en triangle => comme avec poly découpé en triangle(use algo)
	//sinon exemple avec Shader Tesselation

	//pour pouvoir modif dans material
	Properties
	{
		[Header(Shading)]
		_TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5
		_CoefTaille("Taille",Range(1,10)) = 1.0
		//autour de l'axe x sur elle melle
		_BendRotRand("Bend Rotation",Range(0,1)) = 0.2
		_BladeWidth("Width",float) = 0.05
		_BladeWidthRand("Width Rand", float) = 0.02
		_BladeHeight("Height", float) = 0.5
		_BladeHeightRand("Height Rand", float) = 0.3
		//var présente dans fichier tess shader
		//attention au nom et pas oublier de lettres......
		_TessellationUniform("Tessellation", Range(1,64)) = 1
		_WindDistoMap("Wind Disto Map", 2D) = "white"{}
		_WindFq("Wind Frequence",Vector) = (0.05,0.05,0,0)
		_WindStrength("Wind Strength",Float) = 1
		_BladeForward("Blade Forward Amount",Float) = 0.38
		_BladeCurve("Blade Curve Amount",Range(1,4)) = 2
	}

		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Autolight.cginc"
		#include "./CustomTessellation.cginc"
		
		//base
		/*float rand(float3 co)
		{
			return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
		}*/

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

		//DANS TESSELATION SHADER
		/*struct vertexInput {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			half2 uv : TEXCOORD0;
		};

		//DANS TESSELATION SHADER
		//hull and domain qui s'en occupe
		struct vertexOutput {
			float4 vertex : SV_POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
			half2 uv : TEXCOORD0;
		};*/

		struct geometryOutput {
			//mot a droite pour sémantic
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		geometryOutput VertexOutput(float3 pos, float2 uv) {
			geometryOutput o;
			o.pos = UnityObjectToClipPos(pos);
			o.uv = uv;

			return o;
		}

		//forward pour curve de l'herbe
		geometryOutput GenerateGrassVertex(float3 vertexPosition, float width, float height, float forward, float2 uv, float3x3 transformMatrix) {
			float3 tangentPoint = float3(width, forward, height);

			float3 localPosition = vertexPosition + mul(transformMatrix, tangentPoint);
			return VertexOutput(localPosition, uv);
		}

		//pour la sortie
		/*float4 vert(float4 vertex : POSITION) : SV_POSITION{
			return vertex;//UnityObjectToClipPos(vertex);
		}*/

		//DANS TESSELATION SHADER avec juste return vertex => et passage dans un Tess Shader
		/*vertexOutput vert(vertexInput v) {
			vertexOutput o;
			o.vertex = v.vertex;
			o.normal = v.normal;
			o.tangent = v.tangent;
			return o;
		}*/


		ENDCG

			SubShader{
			//ordre face impacté par geoshader ?
			//rendu avec ordre point 1 -> 2 -> 3 etc...
			//Cull Off
			Cull Off

				Pass
			{
				Tags
				{
					"RenderType" = "Opaque"
					"LightMode" = "ForwardBase"
				}

				CGPROGRAM
				#pragma vertex vert
				#pragma geometry geo
				#pragma fragment frag
			//gérer dans TESSELATION SHADER
			#pragma hull hull
			#pragma domain domain
			#pragma target 4.6

			#include "Lighting.cginc"
			#define BLADE_SEGMENTS 3

				float distance;

				//var dans matériaux globales
				float4 _TopColor;
				float4 _BottomColor;
				float _TranslucentGain;
				float _CoefTaille;
				float _BendRotRand;
				float _BladeWidth;
				float _BladeWidthRand;
				float _BladeHeight;
				float _BladeHeightRand;
				sampler2D _WindDistoMap;
				float4 _WindDistoMap_ST;
				float2 _WindFq;
				float _WindStrength;
				float _BladeForward;
				float _BladeCurve;
				//penser a mettre position joueur
				//et point central pour changer Tessellation et mouvement

				//DRY => Don't Repeat Yourself

				//ATTENTION GROUND PLANE => Une seul
				//GROUND PLANE 10*10 => plusieurs vertices
				
				//SV_POSITION n'est plus de rigueur car on l'a déjà mis dans la struct d'entrée
				//void geo(triangle float4 IN[3] : SV_POSITION, inout TriangleStream<geometryOutput> triStream) {
				//[maxvertexcount(3)]
				//ajout découpage en plusieurs segments de l'herbe
				[maxvertexcount(BLADE_SEGMENTS * 2 + 1)]
				void geo(triangle vertexOutput IN[3], inout TriangleStream<geometryOutput> triStream) {
					geometryOutput o;

					/*float4 a = IN[0].vertex - IN[1].vertex;
					float4 b = IN[2].vertex - IN[1].vertex;
					float3 normal = -normalize(cross(a, b)).rgb;*/
					float3 pos;

					float3 tmp;
					pos = IN[0].vertex;
					float3 vNormal = IN[0].normal;
					//ICI ON VA UTILISER POUR BITANGENT SPACE POUR TRANSFORMATION LOCAL DANS LE WORLD
					float4 vTangent = IN[0].tangent;
					//cross product tangent and normal multilply by the direction here stocked by unity in w homogeneus data to lightened mem usage
					float3 vBinormal = cross(vNormal,vTangent) * vTangent.w;

					float3x3 tangentToLocal = float3x3(
						vTangent.x,vBinormal.x,vNormal.x,
						vTangent.y,vBinormal.y,vNormal.y,
						vTangent.z,vBinormal.z,vNormal.z
						);

					float3x3 facingRotMat = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));
					//0.5 pour 1/PI => soit 180 degré /2.0f => 90 pour pas de plillure au dela (en sous map)
					//float3x3 bendRotMat = AngleAxis3x3(rand(pos.zzx) * _BendRotRand * UNITY_PI * 0.5f * (sin(_Time.x*100.0f) + 1 / 2.0f), float3(-1, 0, 0));
					float3x3 bendRotMat = AngleAxis3x3(rand(pos.zzx) * _BendRotRand * UNITY_PI * 0.5f, float3(-1, 0, 0));
					//mat mul not commutativ => rappel!
					//droite vers gauche en info
					
					//pour texture wind => avec fq en fonction du temps
					float2 uv = pos.xz * _WindDistoMap_ST.xy + _WindDistoMap_ST.zw + _WindFq * _Time.y;
					//passage de 0 a 1 => -1 1 => utilisation de coord texture entre -1 et 1 pour vecteur et opposé * coef force
					//ici recup donné sur normal map en gros
					float2 windSample = (tex2Dlod(_WindDistoMap, float4(uv, 0, 0)).xy * 2 - 1) * _WindStrength;

					//normalisation de ce vecteur recup en prenant uniquement axe x et y
					float3 wind = normalize(float3(windSample.x, windSample.y, 0));

					//pour rot angle sur 180 degré
					float3x3 windRot = AngleAxis3x3(UNITY_PI * windSample, wind);

					float3x3 transformationMat = mul(mul(mul(tangentToLocal,windRot), facingRotMat), bendRotMat);

					float3x3 transformationMatFacing = mul(tangentToLocal, facingRotMat);

					//entre 0 et 1 normalement
					float height = ((rand(pos.zyx) * 2 - 1) * _BladeHeightRand + _BladeHeight) * _CoefTaille;
					//ordre x y z => pour x et inverse pour z
					float width = ((rand(pos.xyz) * 2 - 1) * _BladeWidthRand + _BladeWidth) * _CoefTaille;

					float forward = rand(pos.yyz) * _BladeForward;

					//v1
					/*triStream.Append(VertexOutput(pos + mul(tangentToLocal,float3(0.5f*_CoefTaille, 0.0f, 0.0f)),float2(0,0)));
					triStream.Append(VertexOutput(pos + mul(tangentToLocal,float3(-0.5f*_CoefTaille, 0.0f, 0.0f)), float2(1, 0)));
					//change the Y dir by conv of Tangent with Z dir
					triStream.Append(VertexOutput(pos + mul(tangentToLocal,float3(0.0f, 0.0f, 1.0f*_CoefTaille)), float2(0.5, 1)));*/
					
					//v2 avec rot rand
					/*triStream.Append(VertexOutput(pos + mul(transformationMat, float3(width, 0.0f, 0.0f)), float2(0, 0)));
					triStream.Append(VertexOutput(pos + mul(transformationMat, float3(-width, 0.0f, 0.0f)), float2(1, 0)));
					//change the Y dir by conv of Tangent with Z dir
					triStream.Append(VertexOutput(pos + mul(transformationMat, float3(0.0f, 0.0f, height)), float2(0.5, 1)));*/

					//v3 avec ajustement rot de la base
					/*triStream.Append(VertexOutput(pos + mul(transformationMatFacing, float3(width, 0.0f, 0.0f)), float2(0, 0)));
					triStream.Append(VertexOutput(pos + mul(transformationMatFacing, float3(-width, 0.0f, 0.0f)), float2(1, 0)));
					//change the Y dir by conv of Tangent with Z dir
					triStream.Append(VertexOutput(pos + mul(transformationMat, float3(0.0f, 0.0f, height)), float2(0.5, 1)));*/
					
					//v4 avec fonction pour générer deux vertices avec la transformation
					/*triStream.Append(GenerateGrassVertex(pos, width, 0, float2(0, 0), transformationMat));
					triStream.Append(GenerateGrassVertex(pos, -width, 0, float2(1, 0), transformationMat));
					triStream.Append(GenerateGrassVertex(pos, 0, height, float2(0.5, 1), transformationMat));*/
					
					//3 premiers niveaux de l'herbe => avec deux vertices
					for (int i = 0; i < BLADE_SEGMENTS; i++) {
						//en fonction de i avec une division de la hauteur totale
						float t = i / (float)BLADE_SEGMENTS;
						float segmentHeight = height * t;
						float segmentWidth = width * (1 - t);

						//la hauteur a la puissance
						float segmentForward = pow(t, _BladeCurve) * forward;

						//pour base
						float3x3 transformationMatrix = (i == 0 ? transformationMatFacing : transformationMat);
						triStream.Append(GenerateGrassVertex(pos, segmentWidth, segmentHeight, segmentForward, float2(0, t), transformationMatrix));
						triStream.Append(GenerateGrassVertex(pos, -segmentWidth, segmentHeight, segmentForward, float2(1, t), transformationMatrix));
					}
					triStream.Append(GenerateGrassVertex(pos, 0, height, forward, float2(0.5, 1), transformationMat));
				}

				//quand une struct avec un param
				//float4 frag(float4 vertex : SV_POSITION) : SV_TARGET{
				//float4 frag(geometryOutput i,fixed facing : VFACE) : SV_Target{
				float4 frag(geometryOutput i) : SV_TARGET{
					//return float4(1,1,1,1);

					//interpolation lerpt
					return lerp(_BottomColor,_TopColor,i.uv.y);
				}

				ENDCG
			}
		}
}
