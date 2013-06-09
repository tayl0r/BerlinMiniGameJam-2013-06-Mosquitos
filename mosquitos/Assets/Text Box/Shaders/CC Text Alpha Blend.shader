// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0
// Simple shader that alpha blends.
Shader "Text Box/Alpha Blend"{
	Properties{
		_MainTex("Font Atlas (Alpha)", 2D) = "clear" {}
	}
	Category{
		Tags{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
//		Cull Off // use this to make it visible from behind
		Lighting Off
		ZWrite Off
		SubShader{
			Pass{
				CGPROGRAM
				#pragma vertex cc_text_vert
				#pragma fragment frag
				
				#include "CC Text CG.cginc"
				
				sampler2D _MainTex;
				
				fixed4 frag(cc_text_v2f f) : COLOR {
					f.color.a *= tex2D(_MainTex, f.uv).a;
					return f.color;
				}

//				version that includes texture RGB				
//				fixed4 frag(cc_text_v2f f) : COLOR {
//					return f.color * tex2D(_MainTex, f.uv);
//				}
				ENDCG
			}
		}
		SubShader{
			// fixed function fallback
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
