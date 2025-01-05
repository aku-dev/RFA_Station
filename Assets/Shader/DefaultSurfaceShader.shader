Shader "AKStudio/HDRPMaskSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (RGB)", 2D) = "white" {}
        _NormalTex ("Normal (RGB)", 2D) = "bump" {}

        _Metallic ("Metallic", Range(0,5)) = 1.0
        _MetallicMin ("Metallic Min", Range(0,1)) = 0.0
        _MetallicMax ("Metallic Max", Range(0.1,1)) = 1

        _Smoothness ("Smoothness", Range(0,5)) = 1.0
        _SmoothnessMin ("Smoothness Min", Range(0,1)) = 0.0
        _SmoothnessMax ("Smoothness Max", Range(0.1,1)) = 1

        _Sharpness ("Sharpness", Range (0, 2)) = 0
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

        half _Smoothness;
        half _SmoothnessMin;
        half _SmoothnessMax;

        half _Metallic;
        half _MetallicMin;
        half _MetallicMax;

        half _Sharpness;

        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 col = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            fixed4 c1 = tex2D (_MainTex, IN.uv_MainTex.xy + 0.0003);
            fixed4 c2 = tex2D (_MainTex, IN.uv_MainTex.xy - 0.0003);
            
            col += c2 * 7.0 * _Sharpness;
            col -= c1 * 7.0 * _Sharpness;
            o.Albedo = col.rgb;

            fixed4 m = tex2D (_MaskTex, IN.uv_MaskTex);

            o.Metallic   = clamp(m.r, _MetallicMin, _MetallicMax) * _Metallic;
            o.Smoothness = clamp(m.a, _SmoothnessMin, _SmoothnessMax) * _Smoothness;

            o.Occlusion = m.g;
            o.Normal = UnpackNormal (tex2D (_NormalTex, IN.uv_NormalTex));

            o.Alpha = col.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
