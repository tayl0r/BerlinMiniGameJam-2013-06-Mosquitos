// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0
// Shader that uses a distance map to apply a gradient.
Shader "Text Box/Gradient"{
	Properties{
		_MainTex("Distance Map (Alpha)", 2D) = "white" {}
		_Gradient("Gradient Map (RGBA)", 2D) = "white" {}
	}
	Category{
		Tags{ "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
//		Cull Off // use this to make it visible from behind
		ZWrite Off
		SubShader{
			Pass{
				CGPROGRAM
				#pragma vertex cc_text_vert
				#pragma fragment frag
				
				#include "CC Text CG.cginc"
				
				sampler2D _MainTex, _Gradient;
				
				fixed4 frag(cc_text_v2f f) : COLOR {
					return f.color * tex2D(_Gradient, half2(tex2D(_MainTex, f.uv).a, 0));
				}
				
//				version that includes texture RGB
//				fixed4 frag(cc_text_v2f f) : COLOR {
//					fixed4 t = tex2D(_MainTex, f.uv);
//					f.color.rgb *= t;
//					return f.color * tex2D(_Gradient, half2(t.a, 0));
//				}
				ENDCG
			}
		}
		SubShader{
			// fixed function fallback to alpha blend
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
