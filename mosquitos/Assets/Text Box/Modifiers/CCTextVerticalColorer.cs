// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0

using UnityEngine;

/// <summary>
/// CCText modifier that colors characters using a simple vertical gradient.
/// </summary>
public sealed class CCTextVerticalColorer : CCTextModifier {
	
	/// <summary>
	/// Top color.
	/// </summary>
	public Color topColor = Color.white;
	
	/// <summary>
	/// Bottom color.
	/// </summary>
	public Color bottomColor = Color.red;
	
	public override void Modify (CCText text) {
		float
			offset = text.minBounds.y,
			scale = 1f / (text.maxBounds.y - offset);
		Vector3[] vertices = text.vertices;
		Color[] colors = text.colors;
		for(int i = 0, v = 0, l = text.Length; i < l; i++){
			char c = text[i];
			if(c <= ' '){
				continue;
			}
			Color
				top = Color.Lerp(bottomColor, topColor, (vertices[v].y - offset) * scale),
				bottom = Color.Lerp(bottomColor, topColor, (vertices[v + 2].y - offset) * scale);
			colors[v] = top;
			colors[v + 1] = top;
			colors[v + 2] = bottom;
			colors[v + 3] = bottom;
			v += 4;
		}
		text.mesh.colors = colors;
	}
}
