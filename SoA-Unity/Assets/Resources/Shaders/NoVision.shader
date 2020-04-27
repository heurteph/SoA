Shader "Shaders/NoVision"
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

		fixed4 frag(v2f_img i) : COLOR{
			fixed4 mask = tex2D(_MaskTex, i.uv * _maskSize);
			fixed4 base = tex2D(_MainTex, i.uv);

			
			/*for (int i = 0; i < height; i++) {
				for (int j = 0; j < width; j++) {

				}
			}*/

			

			//float r = rand(float3(i.pos.x+time*10.0f, i.pos.y + time * 10.0f,0.0f));

			//return lerp(base, mask, _maskBlend);
			//float4 col = float4(time, time, time, 1.0f);
			float grey = (base.r + base.g + base.b) / 3.0f;
			//res += grey;
			grey = 0.0f;
			//res += 0.01f;
			//tab[0] += 0.01f;
			//float tmp = tab[0];

			float4 col;
			if (type == 1) {
				col = float4(1.0f, grey, grey, 1.0f);
			}
			else {
				col = float4(grey, grey, grey, 1.0f);
			}


			col = float4(base.r, base.g, base.b, 1.0f);
		
			time = -100.0f;

			/*if (i.pos.x == width - 1 && i.pos.y == height - 1) {
				total = res;
			}*/

			/*if(i.pos.y >= 0.0f && i.pos.y <= 200.0f) {
				col.g = 1.0f;
			}*/
			return col;		}
		ENDCG
	}
	}
}
