Shader "AKStudio/SkyboxObjects" {
Properties {
	_Cube ("Cubemap", CUBE) = "" {}
}

SubShader {
      Tags { "RenderType" = "Opaque" }
      
      Pass {   
         ZWrite Off         

         CGPROGRAM
 
         #pragma vertex vert  
         #pragma fragment frag 
 
         #include "UnityCG.cginc"

         uniform samplerCUBE _Cube;   
 
         struct appdata
		 {
            float4 vertex : POSITION;
			float3 normal : NORMAL;
         };
		 
         struct v2f {
            float4 pos : SV_POSITION;
            float3 vertex : TEXCOORD1;
         };
 
         v2f vert(appdata v) 
         {
            v2f o;
			o.vertex = v.vertex;
            o.pos = UnityObjectToClipPos(v.vertex);
            return o;
         }
 
         float4 frag(v2f i) : SV_Target
         {
			fixed4 col = texCUBE (_Cube, normalize(i.vertex.xyz));
			return col;
         }
 
         ENDCG		 
      }
   }   
}