// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0
// Variant of the "Text Box/Smooth" shader that uses an alternative edge based on the vertex alpha channel.
Shader "Text Box/Smooth Bold"{
	Properties{
		_MainTex("Distance Map (Alpha)", 2D) = "white" {}
		_EdgeMin("Edge Minimum (Outside)", Float) = 0.45
		_EdgeMax("Edge Maximum (Inside)", Float) = 0.55
		_BoldMin("Bold Minimum (Outside)", Float) = 0.4
		_BoldMax("Bold Maximum (Inside)", Float) = 0.5
	}
	SubShader{
		Tags{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
//		Cull Off // use this to make it visible from behind
		Lighting Off
		ZWrite Off
		Pass{
			CGPROGRAM
			#pragma vertex cc_text_vert
			#pragma fragment frag
			
			#include "CC Text CG.cginc"
			
			sampler2D _MainTex;
			fixed _EdgeMin, _EdgeMax, _BoldMin, _BoldMax;
			
			fixed4 frag(cc_text_v2f f) : COLOR {
				fixed a = tex2D(_MainTex, f.uv).a;
				f.color.a = smoothstep(lerp(_EdgeMin, _BoldMin, f.color.a), lerp(_EdgeMax, _BoldMax, f.color.a), tex2D(_MainTex, f.uv).a);
				return f.color;
			}
			
//			version that includes texture RGB
//			fixed4 frag(cc_text_v2f f) : COLOR {
//				fixed4 t = tex2D(_MainTex, f.uv);
//				f.color.rgb *= t;
//				f.color.a = smoothstep(lerp(_EdgeMin, _BoldMin, f.color.a), lerp(_EdgeMax, _BoldMax, f.color.a), t.a);
//				return f.color;
//			}
			ENDCG
		}
	}
	Fallback "Text Box/Smooth"
}
