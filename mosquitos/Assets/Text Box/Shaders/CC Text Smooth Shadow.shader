// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0
// Variant of the "Text Box/Smooth" shader that adds a shadow effect.
Shader "Text Box/Smooth Shadow"{
	Properties{
		_MainTex("Distance Map (Alpha)", 2D) = "white" {}
		_EdgeMin("Edge Minimum (Outside)", Float) = 0.45
		_EdgeMax("Edge Maximum (Inside)", Float) = 0.55
		_ShadowColor("Shadow Color", Color) = (0, 0, 0, 1)
		_ShadowMin("Shadow Minimum (Outside)", Float) = 0.3
		_ShadowMax("Shadow Maximum (Inside)", Float) = 0.5
		_ShadowOffsetU("Shadow Offset U", Float) = -0.005
		_ShadowOffsetV("Shadow Offset V", Float) = 0.005
	}
	Subshader{
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
			fixed _EdgeMin, _EdgeMax, _ShadowMin, _ShadowMax;
			half _ShadowOffsetU, _ShadowOffsetV;
			fixed4 _ShadowColor;
			
			fixed4 frag(cc_text_v2f f) : COLOR {
				f.color.a *= smoothstep(_EdgeMin, _EdgeMax, tex2D(_MainTex, f.uv).a);
				
				fixed4 shadow = _ShadowColor;
				shadow.a *= smoothstep(_ShadowMin, _ShadowMax, tex2D(_MainTex, f.uv + half2(_ShadowOffsetU, _ShadowOffsetV)).a);
				
				f.color.rgb = lerp(shadow, f.color, f.color.a);
				f.color.a = max(f.color.a, shadow.a);
				return f.color;
			}
			
//			version that includes texture RGB
//			fixed4 frag(cc_text_v2f f) : COLOR {
//				fixed4 t = tex2D(_MainTex, f.uv);
//				f.color.rgb *= t;
//				f.color.a *= smoothstep(_EdgeMin, _EdgeMax, t.a);
//				
//				fixed4 shadow = _ShadowColor;
//				shadow.a *= smoothstep(_ShadowMin, _ShadowMax, tex2D(_MainTex, f.uv + half2(_ShadowOffsetU, _ShadowOffsetV)).a);
//				
//				f.color.rgb = lerp(shadow, f.color, f.color.a);
//				f.color.a = max(i.color.a, shadow.a);
//				return f.color;
//			}
			ENDCG
		}
	}
	Fallback "Text Box/Smooth"
}
