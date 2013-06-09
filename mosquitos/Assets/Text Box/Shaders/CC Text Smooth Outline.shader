// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0
// Variant of the "Text Box/Smooth" shader that adds an outline.
Shader "Text Box/Smooth Outline"{
	Properties{
		_MainTex("Distance Map (Alpha)", 2D) = "white" {}
		_EdgeMin("Edge Minimum (Outside)", Float) = 0.45
		_EdgeMax("Edge Maximum (Inside)", Float) = 0.55
		_OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
		_OutlineMin("Outline Minimum (Ouside)", Float) = 0.25
		_OutlineMax("Outline Maximum (Inside)", Float) = 0.35
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
			fixed4 _OutlineColor;
			fixed _EdgeMin, _EdgeMax, _OutlineMin, _OutlineMax;
			
			fixed4 frag(cc_text_v2f f) : COLOR {
				fixed a = tex2D(_MainTex, f.uv).a;
				f.color = lerp(_OutlineColor, f.color, smoothstep(_EdgeMin, _EdgeMax, a));
				f.color.a *= smoothstep(_OutlineMin, _OutlineMax, a);
				return f.color;
			}

//			version that includes texture RGB
//			fixed4 frag(cc_text_v2f f) : COLOR {
//				fixed4 t = tex2D(_MainTex, i.uv);
//				f.color = lerp(_OutlineColor, f.color, smoothstep(_EdgeMin, _EdgeMax, t.a));
//				f.color.rgb *= t;
//				f.color.a *= smoothstep(_OutlineMin, _OutlineMax, t.a);
//				return f.color;
//			}
			ENDCG
		}
	}
	Fallback "Text Box/Smooth"
}
