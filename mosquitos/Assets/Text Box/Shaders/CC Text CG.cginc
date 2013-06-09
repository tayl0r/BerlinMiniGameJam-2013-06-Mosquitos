// Copyright 2012, Catlike Coding
// http://catlikecoding.com
// Version 1.0
// Default structs and functions for CC Text shaders.
#ifndef CC_TEXT_CG_INCLUDED
#define CC_TEXT_CG_INCLUDED

// unity to vertex struct
struct cc_text_u2v {
	float4 vertex : POSITION;
	fixed4 color: COLOR;
	half2 texcoord : TEXCOORD0;
};

// vertex to fragment struct
struct cc_text_v2f {
	float4 pos : POSITION;
	fixed4 color: COLOR;
	half2 uv : TEXCOORD0;
};

// vertex function
cc_text_v2f cc_text_vert (cc_text_u2v v) {
	cc_text_v2f o;
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	o.color = v.color;
	o.uv = v.texcoord;
	return o;
}

#endif
