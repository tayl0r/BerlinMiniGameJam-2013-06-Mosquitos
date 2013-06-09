// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0
// Simple shader that alpha tests.
Shader "Text Box/Alpha Test"{
	Properties{
		_MainTex("Distance Map (Alpha)", 2D) = "white" {}
		_AlphaBoundary("Alpha Boundary", Float) = 0.5
	}
	Category{
		Tags{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Blend Off
//		Cull Off // use this to make it visible from behind
		Lighting Off
		ZWrite On
		SubShader{
			Pass{
				CGPROGRAM
				#pragma vertex cc_text_vert
				#pragma fragment frag
				
				#include "CC Text CG.cginc"
				
				sampler2D _MainTex;
				fixed _AlphaBoundary;
				
				fixed4 frag(cc_text_v2f f) : COLOR {
					clip(f.color.a * tex2D(_MainTex, f.uv).a - _AlphaBoundary);
					return f.color;
				}

//				version that includes texture RGB		
//				fixed4 frag(cc_text_v2f f) : COLOR {
//					fixed4 t = tex2D(_MainTex, f.uv);
//					clip(t.a - _AlphaBoundary);
//					return f.color * t;
//				}
				ENDCG
			}
		}
		SubShader{
			// fixed function fallback
			AlphaTest GEqual [_AlphaBoundary]
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
