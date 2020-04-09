Shader "Shaders/Contour"
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
			#pragma vertex vert_img
			#pragma fragment frag
			#include "UnityCG.cginc"

			//float total_grey;
			//groupshared float tab[1];
		shared float res = 0.0f;	
		uniform sampler2D _MainTex;
		uniform sampler2D _MaskTex;
		uniform float type;
		uniform float time;
		uniform float height;
		uniform float width;

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

		fixed4 frag(v2f_img im) : COLOR{
			fixed4 mask = tex2D(_MaskTex, im.uv * _maskSize);
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
				/*float r, g, b;
				r = g = b = 0.0f;
				r = sampleTex[i].r * kernel[i];
				g = sampleTex[i].g * kernel[i];
				b = sampleTex[i].b * kernel[i];
				col = (col.r + r, col.g + g, col.b + b, 1.0f);*/
				col += (sampleTex[i] * kernel[i]);
			}
			//pour blur simple
			//col /= 9.0f;

			/*float4 col = float4(0.0f, 0.0f, 0.0f, 1.0f);
			float ite = 0.0f;
			for (int j = 1; j >= -1; j--) {
				for (int i = -1; i <= 1; i++) {
					half2 uv = im.uv;
					uv.x += i;//offsets[i].x;
					uv.y += j;//offsets[i].y;
					if (uv.x >= 0 && uv.x < width && uv.y >= 0 && uv.y < height) {
						float4 tmp = tex2D(_MainTex, uv);
						float r, g, b;
						r = tmp.r * kernel[ite];
						g = tmp.g * kernel[ite];
						b = tmp.b * kernel[ite];
						col = float4(col.r + r, col.g + g, col.b + b, 1.0f);
					}
					ite++;
				}
			}*/
			/*if ((col.r + col.g + col.b) < 2.8f) {
				col = base;
			}
			else {
				col = float4(1.0f - col.r, 1.0f - col.g, 1.0f - col.b, 1.0f);
			}*/
				
				
			//pour cell shading
			//col = float4(1.0f - col.r, 1.0f - col.g, 1.0f - col.b,1.0f);
			//col = float4(col.r * base.r, col.g * base.g, col.b * base.b, 1.0f);
			base = saturate(base);
			col = float4(1.0f - col.r, 1.0f - col.g, 1.0f - col.b, 1.0f);
			col = float4(col.r * base.r, col.g * base.g, col.b * base.b, 1.0f);
			

			return col;		
		}
		ENDCG
	}
	}
}
