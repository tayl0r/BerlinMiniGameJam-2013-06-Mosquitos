/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.2
	
	1.0.2: Initial version.
*/

using UnityEngine;

/// <summary>
/// CCTextModifier that adjust vertices so the text wraps around a cylinder.
/// </summary>
public sealed class CCTextCylinderWrapper : CCTextModifier {
	
	private static Vector3
		notMinimum = Vector3.one * float.MaxValue,
		notMaximum = Vector3.one * float.MinValue;
	
	/// <summary>
	/// The axis to wrap around.
	/// </summary>
	public enum WrapMode {
		/// <summary>
		/// Wrap around the X axis.
		/// </summary>
		X,
		/// <summary>
		/// Wrap around the Y axis.
		/// </summary>
		Y
	}
	
	/// <summary>
	/// Which axis to wrap around.
	/// </summary>
	public WrapMode wrapMode;
	
	/// <summary>
	/// Radius of the cylinder to wrap around.
	/// A positive radius places text on the outside, while a negative radius places it on the inside.
	/// </summary>
	public float radius;
	
	public override void Modify (CCText text){
		if(wrapMode == WrapMode.X){
			WrapX(text);
		}
		else{
			WrapY(text);
		}
	}
	
	private void WrapX (CCText text) {
		Vector3[] vertices = text.vertices;
		Vector3
			minBounds = notMinimum,
			maxBounds = notMaximum;
		float
			r = text.Offset.z - radius,
			y2r = r == 0f ? 0f : 1f / r;
		for(int i = 0, v = 0, l = text.Length; i < l; i++){
			if(text[i] <= ' '){
				continue;
			}
			// vertex 0
			Vector3 vertex = vertices[v];
			float rad = vertex.y * y2r;
			vertex.y = Mathf.Sin(rad) * r;
			vertex.z = Mathf.Cos(rad) * r;
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
			
			// vertex 1
			vertices[v + 1].y = vertex.y;
			vertices[v + 1].z = vertex.z;
			
			// vertex 2
			vertex = vertices[v + 2];
			rad = vertex.y * y2r;
			vertex.y = Mathf.Sin(rad) * r;
			vertex.z = Mathf.Cos(rad) * r;
			vertices[v + 2] = vertex;
			
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
			
			// vertex 3
			vertices[v + 3].y = vertex.y;
			vertices[v + 3].z = vertex.z;
			v += 4;
		}
		text.minBounds = minBounds;
		text.maxBounds = maxBounds;
	}
	
	private void WrapY (CCText text) {
		Vector3[] vertices = text.vertices;
		Vector3
			minBounds = notMinimum,
			maxBounds = notMaximum;
		float
			r = text.Offset.z - radius,
			x2r = r == 0f ? 0f : 1f / r;
		for(int i = 0, v = 0, l = text.Length; i < l; i++){
			if(text[i] <= ' '){
				continue;
			}
			// vertex 0
			Vector3 vertex = vertices[v];
			float rad = vertex.x * x2r;
			vertex.x = Mathf.Sin(rad) * r;
			vertex.z = Mathf.Cos(rad) * r;
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
			
			// vertex 3
			vertices[v + 3].x = vertex.x;
			vertices[v + 3].z = vertex.z;
			
			// vertex 1
			vertex = vertices[v + 1];
			rad = vertex.x * x2r;
			vertex.x = Mathf.Sin(rad) * r;
			vertex.z = Mathf.Cos(rad) * r;
			vertices[v + 1] = vertex;
			
			// vertex 2
			vertex.y = vertices[v + 2].y;
			vertices[v + 2] = vertex;
			v += 4;
			
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
		text.minBounds = minBounds;
		text.maxBounds = maxBounds;
	}
}
