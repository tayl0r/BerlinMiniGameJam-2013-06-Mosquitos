// unlit, vertex colour, alpha blended
// cull off

Shader "tk2d/AdditiveVertexColorScrollingMask" 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_MaskTex ("Mask (RGB) Trans (A)", 2D) = "white" {}
        _ScrollU ("Scroll U", float) = 0.6
        _ScrollV ("Scroll V", float) = 0
	}
	
//    SubShader {
//		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
//		ZWrite Off Lighting Off Cull Off Fog { Mode Off } Blend SrcAlpha One
//		LOD 110
//
//        Pass {
//            SetTexture [_MaskTex] {
//                combine texture alpha
//            }
//            SetTexture [_MainTex] {
//                combine texture * previous + texture
//            }
//        }
//    }
    
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off Lighting Off Cull Off Fog { Mode Off } Blend SrcAlpha One
		LOD 110
		
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert_vct
			#pragma fragment frag_mult 
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _MaskTex;
			//float4 _MainTex_ST;
			//float4 _MaskTex_ST;
		    float _ScrollU;
		    float _ScrollV;
         
			struct vin_vct 
			{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoordMask : TEXCOORD1;
			};

			struct v2f_vct
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoordMask : TEXCOORD1;
			};

			v2f_vct vert_vct(vin_vct v)
			{
				v2f_vct o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;
				o.texcoordMask = v.texcoordMask;
				//o.texcoord.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				//o.texcoordMask.xy = TRANSFORM_TEX(v.texcoordMask, _MaskTex);
//				o.texcoordMask.x += _ScrollU;
//				o.texcoordMask.y += _ScrollV;
//				o.texcoordMask.x += frac(_Time * _ScrollU);
//				o.texcoordMask.y += frac(_Time * _ScrollV);
				o.texcoordMask.x += frac(_ScrollU);
				o.texcoordMask.y += frac(_ScrollV);
				return o;
			}

			fixed4 frag_mult(v2f_vct i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
				fixed4 mask = tex2D(_MaskTex, i.texcoordMask);
				col.a *= mask.a;
				return col;
			}
			
			ENDCG
		} 
	}
 
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off Blend SrcAlpha One Cull Off Fog { Mode Off }
		LOD 100

		BindChannels 
		{
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
			Bind "Color", color
		}

		Pass 
		{
			Lighting Off
			SetTexture [_MainTex] { combine texture * primary } 
		}
	}
}
