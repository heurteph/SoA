Shader "Shaders/Contour_v1"
{
	Properties{
	_MainTex("Base (RGB)", 2D) = "white" {}
	_MaskTex("Mask texture", 2D) = "white" {}
	_maskBlend("Mask blending", Float) = 0.5
	_maskSize("Mask Size", Float) = 1
	}
		SubShader{
			Pass{
			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
			#pragma exclude_renderers d3d11 gles
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

		uniform sampler2D _MainTex;
		uniform sampler2D _MaskTex;
		uniform float time;
		uniform float height;
		uniform float width;
		//uniform float _tmp;

		fixed _maskBlend;
		fixed _maskSize;

		const float offset = 1.0f / 300.0f;

		/*float2 offsets[9] = float2[](float2(-offset, offset), float2(0.0f, offset), float2(offset, offset),
			float2(-offset, 0.0f), float2(0.0f, 0.0f), float2(offset, 0.0f),
			float2(-offset, -offset), float2(-offset, 0.0f), float2(-offset, offset));*/

		/*half2 offsets[9] = half2[](half2(-offset, offset), half2(0.0f, offset), half2(offset, offset),
			half2(-offset, 0.0f), half2(0.0f, 0.0f), half2(offset, 0.0f),
			half2(-offset, -offset), half2(0.0f, -offset), half2(offset, -offset));*/

		float kernel[] = { 1.0f,1.0f,1.0f,1.0f,-8.0f,1.0f,1.0f,1.0f,1.0f };

		float rand(float3 myVector) {
			return frac(sin(dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
		}
		fixed4 frag(v2f_img im) : COLOR{
			//float4 sampleTex[9];
			/*for (int i = 0; i < 9; i++) {
				half2 uv = im.uv;
				uv.x += offsets[i].x;
				uv.y += offsets[i].y;
				sampleTex[i] = tex2D(_MainTex, uv);
			}*/

			//float4 col = float4(0.0f, 0.0f, 0.0f, 1.0f);

			/*for (int i = 0; i < 9; i++) {
				float r, g, b;
				r = g = b = 0;
				r = sampleTex[i].r * kernel[i];
				g = sampleTex[i].g * kernel[i];
				b = sampleTex[i].b * kernel[i];
				col = (col.r + r, col.g + g, col.b + b, 1.0f);
			}*/



			/*float4 col = float3(0.0f, 0.0f, 0.0f,1.0f);
			for (int i = 0; i < 9; i++) {
				col.r += sampleTex[i].r * kernel[i];
				col.g += sampleTex[i].g * kernel[i];
				col.b += sampleTex[i].b * kernel[i];
			}
			return col;
			*/


					//fixed4 mask = tex2D(_MaskTex, im.uv * _maskSize);
					/*fixed4 base = tex2D(_MainTex, im.uv);
					float r, g, b;
					r = g = b = 0.0f;
					float x, y;
					x = im.pos.x;
					y = im.pos.y;
					int ite = 0.0f;*/


					/*for (int j = 1; j >= -1; j--) {
						for (int i = -1; i <= 1; i++) {
							if (x + i >= 0 && (x+i) < width && y>=0 && (y+j)< height) {
								half2 uv;
								uv.x = i+x;//((im.pos.x + (1/300.0f)) / width)*i;
								uv.y = j+y;//((im.pos.y + (1/300.0f)) / height)*j;
								float4 tmp = tex2D(_MainTex, uv);
								r += tmp.r * kernel[ite];
								g += tmp.g * kernel[ite];
								b += tmp.b * kernel[ite];
							}
							ite++;
						}
					}*/
				//r /= ite;
				//g /= ite;
				//b /= ite;

				//float4 color = tex2D(_MainTex, im.uv);
				/*
				float4 color = float4(r/9.0f, g / 9.0f, b / 9.0f, 1.0f);
				return color;
				*/


				//float r = rand(float3(im.pos.x + time, im.pos.y + time, 0.0f));
				
				//niveau de grey
				/*float average = (base.r + base.g + base.b) / 3.0f;
				float4 col = float4(average, average, average, 1.0f);
				return col;
				*/



				//niveau de grey inverse
				//float range = 0.08f;
				//float average = (base.r + base.g + base.b) / 3.0f;
				//float4 col = float4((1.0f - average)- range * time, (1.0f - average) - range*time, (1.0f - average) - range*time, 1.0f);

				float4 col = float4(1.0f, 1.0f, 1.0f, 1.0f);

				return col;


				//return float4(r,g,b,1.0f);

				//lerp interpolation
				//return lerp(base, mask, _maskBlend);
				//float r = rand(float3(i.pos.x + time, i.pos.y + time,0.0f));
				//return lerp(base, mask, _maskBlend);

			}
			ENDCG
			}
	}
}