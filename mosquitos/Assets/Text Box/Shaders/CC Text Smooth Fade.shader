// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0
// Variant of the "Text Box/Smooth" shader that subtly fades by increasing Edge Maximum.
Shader "Text Box/Smooth Fade"{
	Properties{
		_MainTex("Distance Map (Alpha)", 2D) = "white" {}
		_EdgeMin("Edge Minimum (Outside)", Float) = 0.45
		_EdgeMax("Edge Maximum (Inside)", Float) = 0.55
		_FadeDistance("Fade Distance (Begin)", Float) = 10
		_FadeStrength("Fade Strength (Increase per Unit)", Float) = 1
	}
	SubShader{
		Tags{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
//		Cull Off // use this to make it visible from behind
		Lighting Off
		ZWrite Off
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "CC Text CG.cginc"
			
			sampler2D _MainTex;
			fixed _EdgeMin, _EdgeMax;
			float _FadeDistance, _FadeStrength;
			
			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color: COLOR;
				half2 uv : TEXCOORD0;
				fixed fade;
			};
			
			v2f vert (cc_text_u2v v) {
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.uv = v.texcoord;
				o.fade = lerp(_EdgeMax, 1, saturate(((length(mul(UNITY_MATRIX_MV, v.vertex)) - _FadeDistance)) * _FadeStrength));
				return o;
			}
			
			fixed4 frag(v2f f) : COLOR {
				f.color.a *= smoothstep(_EdgeMin, f.fade, tex2D(_MainTex, f.uv).a);
				return f.color;
			}
			
//			version that includes texture RGB
//			fixed4 frag(v2f f) : COLOR {
//				fixed4 t = tex2D(_MainTex, f.uv);
//				f.color.rgb *= t;
//				f.color.a *= smoothstep(_EdgeMin, f.fade, t.a);
//				return f.color;
//			}
			ENDCG
		}
	}
	Fallback "Text Box/Smooth"
}
