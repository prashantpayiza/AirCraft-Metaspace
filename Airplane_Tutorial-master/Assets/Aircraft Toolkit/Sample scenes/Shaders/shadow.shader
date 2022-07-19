// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Gargore/FX/Shadow" {
	Properties {
		_MainTex ("Base", 2D) = "white" {}
		_Alfa ("Alpha", Range (0.0, 1.0)) = 1.0
	}
	
	CGINCLUDE

		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float _Alfa;
						
		struct v2f {
			half4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;
		};

		v2f vert(appdata_full v) {
			v2f o;
			
			o.pos = UnityObjectToClipPos (v.vertex);	
			o.uv.xy = v.texcoord.xy;
					
			return o; 
		}
		
		fixed4 frag( v2f i ) : COLOR {
			fixed4 texel = tex2D(_MainTex, i.uv.xy);
			//texel.r = _Alfa;
			texel.a = _Alfa;
			return texel;
		}
	
	ENDCG
	
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		//Cull Off
		Cull Back
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		//Blend Zero SrcColor
		//Blend Zero SrcAlpha
		//Blend DstColor SrcAlpha
		//Blend SrcAlpha DstColor
		Blend Zero SrcAlpha
	Pass {
	
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		
		ENDCG
		 
		}
				
	} 
	FallBack Off
}