// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0
// Shader that alpha blends and looks like an anti-aliased alpha test.
Shader "Text Box/Smooth"{
	Properties{
		_MainTex("Distance Map (Alpha)", 2D) = "white" {}
		_EdgeMin("Edge Minimum (Outside)", Float) = 0.45
		_EdgeMax("Edge Maximum (Inside)", Float) = 0.55
	}
	Category{
		Tags{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
//		Cull Off // use this to make it visible from behind
		Lighting Off
		SubShader{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Pass{
				CGPROGRAM
				#pragma vertex cc_text_vert
				#pragma fragment frag
				
				#include "CC Text CG.cginc"
				
				sampler2D _MainTex;
				fixed _EdgeMin, _EdgeMax;
				
				fixed4 frag(cc_text_v2f f) : COLOR {
					f.color.a *= smoothstep(_EdgeMin, _EdgeMax, tex2D(_MainTex, f.uv).a);
					return f.color;
				}
				
//				version that includes texture RGB
//				fixed4 frag(cc_text_v2f f) : COLOR {
//					fixed4 t = tex2D(_MainTex, f.uv);
//					f.color.rgb *= t;
//					f.color.a *= smoothstep(_EdgeMin, _EdgeMax, t.a);
//					return f.color;
//				}
				ENDCG
			}
		}
		SubShader{
			// fixed function fallback to alpha test
			AlphaTest GEqual [_EdgeMin]
			Blend Off
			ZWrite On
			BindChannels{
				Bind "Vertex", vertex
				Bind "Color", color
				Bind "TexCoord", texcoord0
			}
			Pass{
				SetTexture[_MainTex]{
					Combine primary, primary * texture
//					Combine primary * texture // use this instead to include texture RGB
				}
			}
		}
	}
}
