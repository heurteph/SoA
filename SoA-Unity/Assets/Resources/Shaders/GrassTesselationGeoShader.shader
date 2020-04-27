Shader "Shaders/GrassTesselationGeoShader"
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
		_TessellationUniform("Tessellation", Range(1,64)) = 1
	}

		CGINCLUDE
		#include "UnityCG.cginc"
		#include "Autolight.cginc"

		//base
		/*float rand(float3 co)
		{
			return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
		}*/

		//rand Trauma
		float rand(float3 myVector) {
			return frac(sin(dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
		}

		struct vertexInput
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
		};

		struct vertexOutput
		{
			float4 vertex : SV_POSITION;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
		};

		struct TessellationFactors
		{
			float edge[3] : SV_TessFactor;
			float inside : SV_InsideTessFactor;
		};

		vertexInput vert(vertexInput v)
		{
			return v;
		}

		vertexOutput tessVert(vertexInput v)
		{
			vertexOutput o;
			// Note that the vertex is NOT transformed to clip
			// space here; this is done in the grass geometry shader.
			o.vertex = v.vertex;
			o.normal = v.normal;
			o.tangent = v.tangent;
			return o;
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


			ENDCG

			SubShader {
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
				#pragma hull hull
				#pragma domain domain
				#pragma target 4.6

				#include "Lighting.cginc"

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
				float _TessellationUniform;

				TessellationFactors patchConstantFunction(InputPatch<vertexInput, 3> patch)
				{
					TessellationFactors f;
					f.edge[0] = _TessellationUniform;
					f.edge[1] = _TessellationUniform;
					f.edge[2] = _TessellationUniform;
					f.inside = _TessellationUniform;
					return f;
				}


				//Hull shader
				[UNITY_domain("tri")]
				[UNITY_outputcontrolpoints(3)]
				[UNITY_outputtopology("triangle_cw")]
				[UNITY_partitioning("integer")]
				[UNITY_patchconstantfunc("patchConstantFunction")]
				vertexInput hull(InputPatch<vertexInput, 3> patch, uint id : SV_OutputControlPointID)
				{
					return patch[id];
				}


				//domain shader
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


				//DRY => Don't Repeat Yourself

				//ATTENTION GROUND PLANE => Une seul
				//GROUND PLANE 10*10 => plusieurs vertices
				
				//SV_POSITION n'est plus de rigueur car on l'a déjà mis dans la struct d'entrée
				//void geo(triangle float4 IN[3] : SV_POSITION, inout TriangleStream<geometryOutput> triStream) {
				[maxvertexcount(3)]
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
					float3x3 bendRotMat = AngleAxis3x3(rand(pos.zzx) * _BendRotRand * UNITY_PI * 0.5f * (sin(_Time.x*100.0f) + 1 / 2.0f), float3(-1, 0, 0));
					//mat mul not commutativ => rappel!
					//droite vers gauche en info
					
					float3x3 transformationMat = mul(mul(tangentToLocal, facingRotMat),bendRotMat);

					//entre 0 et 1 normalement
					float height = ((rand(pos.zyx) * 2 - 1) * _BladeHeightRand + _BladeHeight) * _CoefTaille;
					//ordre x y z => pour x et inverse pour z
					float width = ((rand(pos.xyz) * 2 - 1) * _BladeWidthRand + _BladeWidth) * _CoefTaille;

					//v1
					/*triStream.Append(VertexOutput(pos + mul(tangentToLocal,float3(0.5f*_CoefTaille, 0.0f, 0.0f)),float2(0,0)));
					triStream.Append(VertexOutput(pos + mul(tangentToLocal,float3(-0.5f*_CoefTaille, 0.0f, 0.0f)), float2(1, 0)));
					//change the Y dir by conv of Tangent with Z dir
					triStream.Append(VertexOutput(pos + mul(tangentToLocal,float3(0.0f, 0.0f, 1.0f*_CoefTaille)), float2(0.5, 1)));*/
					
					//v2 avec rot rand
					triStream.Append(VertexOutput(pos + mul(transformationMat, float3(width, 0.0f, 0.0f)), float2(0, 0)));
					triStream.Append(VertexOutput(pos + mul(transformationMat, float3(-width, 0.0f, 0.0f)), float2(1, 0)));
					//change the Y dir by conv of Tangent with Z dir
					triStream.Append(VertexOutput(pos + mul(transformationMat, float3(0.0f, 0.0f, height)), float2(0.5, 1)));

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
