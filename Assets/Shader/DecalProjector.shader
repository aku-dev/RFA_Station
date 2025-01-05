Shader "AKStudio/DecalProjector" {
	Properties {
		_DecalTex ("Decal Map", 2D) = "gray" {}
		[NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
		_Color ("Color", Color) = (0.5,0.5,0.5,1)
		[NoScaleOffset] _Cube ("Cubemap", CUBE) = "" {}
		_Smoothness ("Smoothness", Range(0,2)) = 1.0
		_SmoothnessThreshold ("Smoothness Threshold", Range(0,0.4)) = 0.00001
	}
	Subshader {
		Tags {"Queue"="Transparent"}
	Pass {
			ZWrite Off
			ColorMask RGB
			Blend DstColor Zero
			Offset -1, -1

			CGPROGRAM

			#pragma target 3.0
			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityGlobalIllumination.cginc"
			#pragma multi_compile_fog

			UNITY_DECLARE_TEXCUBE(_RealtimeCubemap);

			
			
			struct v2f {
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;				
				float4 pos : SV_POSITION;

				float3 worldPos : TEXCOORD3;
                half3 tspace0 : TEXCOORD4; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD5; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD6; // tangent.z, bitangent.z, normal.z

				UNITY_FOG_COORDS(2)
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			sampler2D _DecalTex;
			sampler2D _BumpMap;
			samplerCUBE _Cube;
			float4 _DecalTex_ST;
			half _Smoothness;
			half _SmoothnessThreshold;
			
			uniform float4 _Color;

			v2f vert (float4 vertex : POSITION, float3 normal : NORMAL, float4 tangent : TANGENT)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_SETUP_INSTANCE_ID(o);

				//fixed4 tex = tex2D(_DecalTex, vertex.xy);
								
				o.uvShadow = mul (unity_Projector, vertex);
				o.uvShadow.xy = TRANSFORM_TEX(o.uvShadow.xy, _DecalTex);
				
				//o.uvShadow.xy = o.uvShadow.xy * _DecalTex_ST.xy + _DecalTex_ST.zw;
				//o.uvShadow.xy = tex.xy * _DecalTex_ST.xy + _DecalTex_ST.zw;

				o.uvFalloff = mul (unity_ProjectorClip, vertex);

				o.pos = UnityObjectToClipPos(vertex);

				// NORMAL
				o.worldPos = mul(unity_ObjectToWorld, vertex).xyz;
                half3 wNormal = UnityObjectToWorldNormal(normal);
                half3 wTangent = UnityObjectToWorldDir(tangent.xyz);
                
                half tangentSign = tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                
                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);				



				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 texS = tex2D (_DecalTex, i.uvShadow);

				texS.rgb = texS.rgb * _Color.rgb;
				
				fixed4 res = lerp(fixed4(1,1,1,0), texS, texS.a);
				
				
				
				if(i.uvShadow.x > 1.0 || i.uvShadow.x < 0.0 || i.uvShadow.y > 1.0 || i.uvShadow.y < 0.0 || i.uvShadow.z > 1.0 || i.uvShadow.z < 0.0) return 1;
				//if(i.pos.x > 700) return 1;


				// sample the normal map, and decode from the Unity encoding
                half3 tnormal = UnpackNormal(tex2D(_BumpMap, i.uvShadow));
                // transform normal from tangent to world space
                half3 worldNormal;
                worldNormal.x = dot(i.tspace0, tnormal);
                worldNormal.y = dot(i.tspace1, tnormal);
                worldNormal.z = dot(i.tspace2, tnormal);

                // rest the same as in previous shader
                half3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                half3 worldRefl = reflect(-worldViewDir, worldNormal);
                //half4 skyData = UNITY_SAMPLE_TEXCUBE(_RealtimeCubemap, worldRefl);
				//half3 skyColor = DecodeHDR (skyData, _RealtimeCubemap_HDR);
				fixed3 ref = texCUBE (_Cube, worldRefl).rgb * _Smoothness;
				// Black Color
				if (res.a > 0.5 && any(res.rgb > fixed3(_SmoothnessThreshold,_SmoothnessThreshold,_SmoothnessThreshold))) {
					res.rgb += ref;
				}
				
				UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(1,1,1,1));
				


				return res;
			}
			ENDCG
		}
	  
	}
}
