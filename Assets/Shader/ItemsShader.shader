Shader "AKStudio/ItemsSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (RGB)", 2D) = "white" {}
        _NormalTex ("Normal (RGB)", 2D) = "bump" {}

        _Sharpness ("Sharpness", Range (0, 2)) = 0
		_Blink  ("Blink Time", Range(0.001, 5)) = 1
		_Intensity  ("Intensity", Range(0.1, 20)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MaskTex;
        sampler2D _NormalTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_MaskTex;
            float2 uv_NormalTex;
        };

        UNITY_INSTANCING_BUFFER_START(Props)
		
        fixed4 _Color;
        half _Sharpness;
		half _Blink;
		half _Intensity;

        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 col = tex2D (_MainTex, IN.uv_MainTex) * 2;

            fixed4 c1 = tex2D (_MainTex, IN.uv_MainTex.xy + 0.0003);
            fixed4 c2 = tex2D (_MainTex, IN.uv_MainTex.xy - 0.0003);
            
            col += c2 * 7.0 * _Sharpness;
            col -= c1 * 7.0 * _Sharpness;
			
			float t = 0;
			if(_Blink > 0.01) {
				//t = 0.5f + abs(sin( _Time.w * _Blink));				
				//if(t < 0.7f) t = 0.7f;
				t = abs(sin( _Time.w * _Blink)) * 2;
				if(t < 0.5f) t = 0.5f;
			}
			t *= t;
			
            o.Albedo = col.rgb;

            fixed4 m = tex2D (_MaskTex, IN.uv_MaskTex);

            o.Metallic   = m.r;
            o.Smoothness = m.a;
            o.Occlusion = m.g;
            o.Normal = UnpackNormal (tex2D (_NormalTex, IN.uv_NormalTex));			
			
            o.Alpha = col.a;
			
			o.Emission = col * _Color * _Intensity * t;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
