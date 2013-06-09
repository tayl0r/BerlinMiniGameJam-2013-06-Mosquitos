// Copyright 2012, Catlike Coding
// http://catlikecoding.com
// Version 1.0

using UnityEngine;

/// <summary>
/// CCTextModifier that highlights marked text in another color.
/// </summary>
public sealed class CCTextMarkedColorer : CCTextModifier {
	
	/// <summary>
	/// Symbol used to begin coloring.
	/// Only the first character of the string is used.
	/// Do not set to the empty string!
	/// </summary>
	public string beginSymbol = "[";
	
	/// <summary>
	/// Symbol used to end coloring.
	/// Only the first character of the string is used.
	/// Do not set to the empty string!
	/// </summary>
	public string endSymbol = "]";
	
	/// <summary>
	/// Alternative color to use.
	/// </summary>
	public Color color;
	
	public override void Modify (CCText text) {
		char
			b = beginSymbol[0],
			e = endSymbol[0];
		Color currentColor = text.Color;
		Color[] colors = text.colors;
		for(int i = 0, v = 0, l = text.Length; i < l; i++){
			char c = text[i];
			if(c <= ' '){
				continue;
			}
			if(c == b){
				colors[v] = currentColor;
				colors[v + 1] = currentColor;
				colors[v + 2] = currentColor;
				colors[v + 3] = currentColor;
				currentColor = color;
			}
			else if(c == e){
				currentColor = text.Color;
				colors[v] = currentColor;
				colors[v + 1] = currentColor;
				colors[v + 2] = currentColor;
				colors[v + 3] = currentColor;
			}
			else{
				colors[v] = currentColor;
				colors[v + 1] = currentColor;
				colors[v + 2] = currentColor;
				colors[v + 3] = currentColor;
			}
			v += 4;
		}
		text.mesh.colors = colors;
	}
}
