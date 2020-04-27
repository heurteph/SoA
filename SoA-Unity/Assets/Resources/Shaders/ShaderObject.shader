Shader "Shaders/ShaderObject"
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

	float kernel[] = { 1.0f,1.0f,1.0f,1.0f,-8.0f,1.0f,1.0f,1.0f,1.0f };

	float rand(float3 myVector) {
		return frac(sin(dot(myVector, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
	}

	fixed4 frag(v2f_img im) : COLOR{
		float4 color = tex2D(_MainTex, im.uv);
		color = float4(1.0f - color);
		
		
		return color;
	}
		ENDCG
	}
	}
}
