/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.3
	
	1.0.3: Initial version.
*/

using UnityEngine;

/// <summary>
/// CCTextModifier that adjust vertices so the text wraps around a torus.
/// </summary>
public sealed class CCTextTorusWrapper : CCTextModifier {
	
	private static Vector3
		notMinimum = Vector3.one * float.MaxValue,
		notMaximum = Vector3.one * float.MinValue;
	
	/// <summary>
	/// The axis to revolve around.
	/// </summary>
	public enum RevolveMode {
		/// <summary>
		/// Revolve around the X axis.
		/// </summary>
		X,
		/// <summary>
		/// Revolve around the Y axis.
		/// </summary>
		Y
	}
	
	/// <summary>
	/// Which axis to revolve around.
	/// </summary>
	public RevolveMode revolveMode;
	
	/// <summary>
	/// Radius of the circle that is revolved.
	/// A positive radius places text on the outside, while a negative radius places it on the inside.
	/// </summary>
	public float minorRadius;
	
	/// <summary>
	/// Radius at which the circle is revolved.
	/// A positive radius places text on the outside, while a negative radius places it on the inside.
	/// </summary>
	public float majorRadius;
	
	
	public override void Modify (CCText text){
		if(revolveMode == RevolveMode.X){
			RevolveX(text);
		}
		else{
			RevolveY(text);
		}
	}
	
	private void RevolveX (CCText text) {
		Vector3[] vertices = text.vertices;
		Vector3
			minBounds = notMinimum,
			maxBounds = notMaximum;
		float
			r = text.Offset.z - minorRadius,
			y2u = majorRadius == 0f ? 0f : -1f / majorRadius,
			x2v = r == 0f ? 0f : 1f / r;
		for(int i = 0, v = 0, l = text.Length; i < l; i++){
			if(text[i] <= ' '){
				continue;
			}
			for(int lv = v + 4; v < lv; v++){
				Vector3 vertex = vertices[v];
				float
					U = vertex.y * y2u,
					V = vertex.x * x2v,
					R = (r * Mathf.Cos(V) - majorRadius);
				vertex.z = R * Mathf.Cos(U);
				vertex.y = R * Mathf.Sin(U);
				vertex.x = r * Mathf.Sin(V);
				vertices[v] = vertex;
				
				if(vertex.x > maxBounds.x){
					maxBounds.x = vertex.x;
				}
				if(vertex.x < minBounds.x){
					minBounds.x = vertex.x;
				}
				if(vertex.y > maxBounds.y){
					maxBounds.y = vertex.y;
				}
				if(vertex.y < minBounds.y){
					minBounds.y = vertex.y;
				}
				if(vertex.z > maxBounds.z){
					maxBounds.z = vertex.z;
				}
				if(vertex.z < minBounds.z){
					minBounds.z = vertex.z;
				}
			}
		}
		text.minBounds = minBounds;
		text.maxBounds = maxBounds;
	}
	
	private void RevolveY (CCText text) {
		Vector3[] vertices = text.vertices;
		Vector3
			minBounds = notMinimum,
			maxBounds = notMaximum;
		float
			r = text.Offset.z - minorRadius,
			x2u = majorRadius == 0f ? 0f : -1f / majorRadius,
			y2v = r == 0f ? 0f : 1f / r;
		for(int i = 0, v = 0, l = text.Length; i < l; i++){
			if(text[i] <= ' '){
				continue;
			}
			for(int lv = v + 4; v < lv; v++){
				Vector3 vertex = vertices[v];
				float
					U = vertex.x * x2u,
					V = vertex.y * y2v,
					R = (r * Mathf.Cos(V) - majorRadius);
				vertex.z = R * Mathf.Cos(U);
				vertex.x = R * Mathf.Sin(U);
				vertex.y = r * Mathf.Sin(V);
				vertices[v] = vertex;
				
				if(vertex.x > maxBounds.x){
					maxBounds.x = vertex.x;
				}
				if(vertex.x < minBounds.x){
					minBounds.x = vertex.x;
				}
				if(vertex.y > maxBounds.y){
					maxBounds.y = vertex.y;
				}
				if(vertex.y < minBounds.y){
					minBounds.y = vertex.y;
				}
				if(vertex.z > maxBounds.z){
					maxBounds.z = vertex.z;
				}
				if(vertex.z < minBounds.z){
					minBounds.z = vertex.z;
				}
			}
		}
		text.minBounds = minBounds;
		text.maxBounds = maxBounds;
	}
}
