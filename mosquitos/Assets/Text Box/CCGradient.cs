/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.2
	
	1.0.2: Asset reset now results in a valid gradient.
	1.0.0: Initial version.
*/

using System;
using UnityEngine;

/// <summary>
/// Color gradient containing a sequence of color stops.
/// </summary>
public sealed class CCGradient : ScriptableObject {
	
	#region Types
	
	/// <summary>
	/// How to interpolation between color stops.
	/// </summary>
	public enum InterpolationMode {
		/// <summary>
		/// Use linear interpolation.
		/// </summary>
		Linear,
		/// <summary>
		/// Use a smooth interpolation that transitions slowly at the edges of the stops, but rapidly in between.
		/// </summary>
		Smooth
	}
	
	/// <summary>
	/// How to wrap positions outside the [0,1] range.
	/// </summary>
	public enum WrapMode {
		/// <summary>
		/// Clamp positions to the [0,1] range.
		/// </summary>
		Clamp,
		/// <summary>
		/// Repeat the gradient outside the [0,1] range.
		/// </summary>
		Repeat,
		/// <summary>
		/// Repeat the gradient outside the [0,1] range, but flip it each interval.
		/// </summary>
		Flip
	}
	
	#endregion
	
	#region Public Properties and Variables

	/// <summary>
	/// Get a color at some position.
	/// </summary>
	/// <param name="p">
	/// A position.
	/// </param>
	public Color this[float p]{
		get{
			switch(wrap){
			case WrapMode.Clamp:
				if(p < 0f) p = 0f;
				if(p > 1f) p = 1f;
				break;
			case WrapMode.Repeat:
				while(p < 0f) p += 1f;
				while(p > 1f) p -= 1f;
				break;
			case WrapMode.Flip:
				bool flip = false;
				while(p < 0f){
					p += 1f;
					flip = !flip;
				}
				while(p > 1f){
					p -= 1f;
					flip = !flip;
				}
				if(flip){
					p = 1f - p;
				}
				break;
			}
			
			if(p <= positions[0]){
				return colors[0];
			}
			else if(p >= positions[positions.Length - 1]){
				return colors[positions.Length - 1];
			}
			int i = 0;
			while(positions[i] < p){
				i += 1;
			}
			i -= 1;
			float t = (p - positions[i]) / ((positions[i + 1] - positions[i]));
			if(interpolation == InterpolationMode.Smooth){
				t = Mathf.SmoothStep(0f, 1f, t);
			}
			return Color.Lerp(colors[i], colors[i + 1], t);
		}
	}
	
	/// <summary>
	/// How to interpolate between stops.
	/// </summary>
	public InterpolationMode interpolation;
	
	/// <summary>
	/// How to wrap positions outside the [0,1] range.
	/// </summary>
	public WrapMode wrap;
	
	#endregion
	
	#region Private Variables
	
	[SerializeField]
	private Color[] colors;
	[SerializeField]
	private float[] positions;
	
	#endregion
	
	#region Public Methods
	
	/// <summary>
	/// Write the gradient to the first pixel row of a texture.
	/// This method does not call Apply on the texture, you have to do that yourself.
	/// </summary>
	/// <param name="minimum">
	/// Minimum gradient position to use.
	/// </param>
	/// <param name="maximum">
	/// Maximum gradient position to use.
	/// </param>
	/// <param name="texture">
	/// Destination texture. Must be writable.
	/// </param>
	public void WriteToTexture (float minimum, float maximum, Texture2D texture) {
		try{
			texture.GetPixel(0, 0);
		}
		catch{
			Debug.LogError("Destination texture must be readable.");
			return;
		}
		for(int i = 0, w = texture.width; i < w; i++){
			texture.SetPixel(i, 0, this[Mathf.Lerp(minimum, maximum, (i + 0.5f) / w)]);
		}
	}
	
	#endregion
	
	#region Unity Events

	void OnEnable () {
		if(positions == null){
			// make sure a new instance is valid
			colors = new Color[]{Color.black, Color.white};
			positions = new float[]{0f, 1f};
		}
	}
	
#if UNITY_EDITOR
	
	void Reset () {
		colors = new Color[]{Color.black, Color.white};
		positions = new float[]{0f, 1f};
	}
	
#endif
	
	#endregion
	
	#region Private Stuff
	
	// prevent direct creation via constructor
	private CCGradient () {}
	
	#endregion
}
