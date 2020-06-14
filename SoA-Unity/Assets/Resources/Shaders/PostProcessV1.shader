Shader "Shaders/PostProcessV1"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MaskTex("Mask texture", 2D) = "white" {}
		_maskBlend("Mask blending", Float) = 0.5
		_maskSize("Mask Size", Float) = 1
		//_CoefBlur("Coef Blur",Float) = 16.0
	}
		SubShader{
			Pass{
				CGPROGRAM
				// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
				//#pragma exclude_renderers d3d11 gles
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				shared float res = 0.0f;	
				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform float4 _MainTex_TexelSize;

				uniform sampler2D _MaskTex;
				uniform float _CoefBlur;
				uniform float type;
				uniform float time;
				uniform float height;
				uniform float width;
				uniform float life;
				uniform float _Radius;
				uniform float4 _OffsetColor;

				uniform bool _StateBlur;
				uniform bool _StateChromatique;
				uniform bool _VignettePleine;

				uniform float _LerpEffect;

				fixed _maskBlend;
				fixed _maskSize;

				float rand(float3 myVector) {
					return frac(sin(dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
				}

				float3 greyScale(float3 vec) {
					float value = (vec.r + vec.g + vec.b) / 3.0f;
					return float3(value, value, value);
				}

				//const ne fonctionne pas !!!!!!!

				//static partout !!!!!!
				//static float offset = 1.0f / 300.0f;
				static float2 offset = float2(1.0f / width, 1.0f / height);

				//repartition sur axe vertical
				static const float gaussian[] = { 0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216 };

				//static partout !!!!!!
				/*static half2 offsets[9] = { half2(-offset, offset), half2(0.0f, offset), half2(offset, offset),
				half2(-offset, 0.0f), half2(0.0f, 0.0f), half2(offset, 0.0f),
				half2(-offset, -offset), half2(0.0f, -offset), half2(offset, -offset) };*/

				static half2 offsets[9] = { half2(-offset.x , offset.y), half2(0.0f, offset.y), half2(offset.x, offset.y),
				half2(-offset.x, 0.0f), half2(0.0f, 0.0f), half2(offset.x , 0.0f),
				half2(-offset.x, -offset.y), half2(0.0f, -offset.y), half2(offset.x, -offset.y) };


				//static partout !!!!!!
				static float kernel[] = { 1.0f,1.0f,1.0f,1.0f,-8.0f,1.0f,1.0f,1.0f,1.0f };

				static float kernel2[] = { -1.0f,-1.0f,-1.0f,-1.0f,8.0f,-1.0f,-1.0f,-1.0f,-1.0f };

				static float kernelCell[] = { -1.0f,-2.0f,-1.0f,0.0f,0.0f,0.0f,1.0f,2.0f,1.0f };

				static float kernelBlur[] = { 1.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f, 2.0f / 16.0f, 4.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f, 2.0f / 16.0f, 1.0f / 16.0f };

				static float kernelBlurDynamic[] = { 1.0f, 2.0f, 1.0f, 2.0f, 4.0f, 2.0f, 1.0f, 2.0f, 1.0f };

				v2f_img vert(float4 pos : POSITION,	float2 uv : TEXCOORD0)
				{
					v2f_img o;
					o.pos = UnityObjectToClipPos(pos);
					o.uv = uv;//TRANSFORM_TEX(uv, _MainTex);
					//UNITY_TRANSFER_FOG(o, o.pos);
					return o;
				}


				//aberration chromatique => offset color r g b

				fixed4 frag(v2f_img im) : COLOR{
					//courbe de distortion avec cos | sin sur la hauteur => avec amplitude variable
					fixed4 mask = tex2D(_MaskTex, im.uv * _maskSize);
					fixed4 base = tex2D(_MainTex, im.uv);
					int i;
					int j;
					float4 col = float4(0.0f, 0.0f, 0.0f, 1.0f);
					float4 sampleTex[9];

					//aberation chromatique simple
					/*col.r = tex2D(_MainTex, float2(im.uv.x + (1.0f / width), im.uv.y)).r;
					col.g = tex2D(_MainTex, float2(im.uv.x, im.uv.y)).g;
					col.b = tex2D(_MainTex, float2(im.uv.x - (1.0f / width), im.uv.y)).b;*/

					if (_StateBlur){
						for (i = 0; i < 9; i++) {
							float2 uv = im.uv;
							uv.x += offsets[i].x;
							uv.y += offsets[i].y;
							//sampleTex[i] = tex2D(_MainTex, uv);
							if (_StateChromatique) {
								sampleTex[i].r = tex2D(_MainTex, float2(uv.x + (1.0f / width), uv.y)).r;
								sampleTex[i].g = tex2D(_MainTex, uv).g;
								sampleTex[i].b = tex2D(_MainTex, float2(uv.x - (1.0f / width), uv.y)).b;
								sampleTex[i].a = 1.0f;
							}
							else {
								sampleTex[i] = tex2D(_MainTex, uv);
							}
						}

						for (i = 0; i < 9; i++) {
							col += (sampleTex[i] * kernelBlurDynamic[i] / _CoefBlur);
						}
					}
					else {
						if (_StateChromatique) {
							col.r = tex2D(_MainTex, float2(im.uv.x + (1.0f / width), im.uv.y)).r;
							col.g = tex2D(_MainTex, float2(im.uv.x, im.uv.y)).g;
							col.b = tex2D(_MainTex, float2(im.uv.x - (1.0f / width), im.uv.y)).b;
						}
						else
							col = base;
					}
					
					//gaussian
					/*float4 col1;// = float4(0.0f, 0.0f, 0.0f, 1.0f);
					float4 col2;// = float4(0.0f, 0.0f, 0.0f, 1.0f);
					col1.rgb = tex2D(_MainTex, im.uv).rgb * gaussian[0];
					float2 tex_offset = float2(1.0f,1.0f) / _MainTex_TexelSize.xy;

					//if(im.uv.x >= 4.0f && im.uv.x <= 1.0f)
					for (i = 1; i < 5; i++) {
						col1.rgb += (tex2D(_MainTex, float2(im.uv.x+i*tex_offset.x, im.uv.y)).rgb * gaussian[i]);
						col1.rgb += (tex2D(_MainTex, float2(im.uv.x-i*tex_offset.x, im.uv.y)).rgb * gaussian[i]);
					}
					col2.rgb = tex2D(_MainTex, im.uv).rgb * gaussian[0];
					for (i = 1; i < 5; i++) {
						col2.rgb += (tex2D(_MainTex, float2(im.uv.x, im.uv.y + i * tex_offset.y)).rgb * gaussian[i]);
						col2.rgb += (tex2D(_MainTex, float2(im.uv.x, im.uv.y - i * tex_offset.y)).rgb * gaussian[i]);
					}

					col = lerp(col1, col2, 0.5f);*/



					/*for (j = 0; j < 3; j++) {
						for (i = 0; i < 3; i++) {
							col += (sampleTex[j*3+i] * gaussian[j]);
							col += (sampleTex[j*3 + i] * gaussian[j]);
						}
					}*/
					
					//base = saturate(base);
					//col = float4(1.0f - col.r, 1.0f - col.g, 1.0f - col.b, 1.0f);
					//col = float4(col.r * base.r, col.g * base.g, col.b * base.b, 1.0f);
					//float4 _color = tex2D(_MainTex, im.uv) * float4(life/100.0f, life / 100.0f, life / 100.0f, 1.0f);	
					/*float2 uv = im.uv;
					float rapport = 1.0f - (life / 100.0f);
					uv.x += sin(_Time.y + uv.y) * rapport;
					uv.x -= (int)uv.x;
					_color = tex2D(_MainTex, uv);
					_color = lerp(saturate(_color), _color, rapport);*/
					//return _color;//float4(greyScale(_color.rgb),_color.a);
					
					//0.5f 0.5f => centre
					//

					float2 centre = float2(0.5f, 0.5f);

					float dist = length(centre - im.uv);
					if (dist >= _Radius) {
						if(_VignettePleine)
							col = float4(0.0f, 0.0f, 0.0f, 1.0f);
						else
							col = lerp(col, float4(0.0f, 0.0f, 0.0f, 1.0f), dist);
					}

					return lerp(base, col, _LerpEffect);//col;
				}
			ENDCG
		}
	}
}
