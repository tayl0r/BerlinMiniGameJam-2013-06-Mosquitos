// Copyright 2012, Catlike Coding
// http://catlikecoding.com
// Version 1.0

using UnityEngine;

/// <summary>
/// CCTextModifier that highlights a range of characters in another color.
/// </summary>
public sealed class CCTextRangeColorer : CCTextModifier {
	
	/// <summary>
	/// First symbol in the range to be colored.
	/// Only the first character of the string is used.
	/// Do not set to the empty string!
	/// </summary>
	public string rangeBeginSymbol = "A";
	
	/// <summary>
	/// Last symbol in the range to be colored.
	/// Only the first character of the string is used.
	/// Do not set to the empty string!
	/// </summary>
	public string rangeEndSymbol = "Z";
	
	/// <summary>
	/// Alternative color to use.
	/// </summary>
	public Color color;
	
	public override void Modify (CCText text) {
		char
			b = rangeBeginSymbol[0],
			e = rangeEndSymbol[0];
		Color normalColor = text.Color;
		Color[] colors = text.colors;
		for(int i = 0, v = 0, l = text.Length; i < l; i++){
			char c = text[i];
			if(c <= ' '){
				continue;
			}
			if(b <= c && c <= e){
				colors[v] = color;
				colors[v + 1] = color;
				colors[v + 2] = color;
				colors[v + 3] = color;
			}
			else{
				colors[v] = normalColor;
				colors[v + 1] = normalColor;
				colors[v + 2] = normalColor;
				colors[v + 3] = normalColor;
			}
			v += 4;
		}
		text.mesh.colors = colors;
	}
}
