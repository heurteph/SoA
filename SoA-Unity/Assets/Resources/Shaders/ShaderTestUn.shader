Shader "Shaders/ShaderTestUn"
{
	Properties{
		_MainTex("Base (RGB)",2D) = "white"{}
		_bwBlend("Black & White blend", Range(0,1)) = 0
	}
		SubShader{
			Pass{
				CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag

				#include "UnityCG.cginc"
				uniform sampler2D _MainTex;
				uniform float _bwBlend;

				float4 frag(v2f_img i) : COLOR{
					float4 c = tex2D(_MainTex,i.uv);
					//intensite des comp rgb
					float lum = c.r*.3 + c.g *.59 + c.b*.11;
					float3 bw = float3(lum, lum, lum);

					float4 res = c;
					res.rgb = lerp(c.rgb, bw, _bwBlend);
					//res.a = 0.0f;
					return res;
				}
				ENDCG
		}
	}
    /*Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"*/
}
