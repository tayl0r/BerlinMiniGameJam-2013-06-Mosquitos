/*
* FarseerUnity based on Farseer Physics Engine port:
* Copyright (c) 2012 Gabriel Ochsenhofer https://github.com/gabstv/Farseer-Unity3D
* 
* Original source Box2D:
* Copyright (c) 2011 Ian Qvist http://farseerphysics.codeplex.com/
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/
using UnityEngine;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FSTransform = FarseerPhysics.Common.Transform;
using Transform = UnityEngine.Transform;
using Microsoft.Xna.Framework;

using Category = FarseerPhysics.Dynamics.Category;

[AddComponentMenu("FarseerUnity/Collision/Basic Shape Component")]
public class FSShapeComponent : MonoBehaviour {

	public ShapeType SType = ShapeType.Polygon;
	public bool UseUnityCollider = true;

	public float Density = 1f;
	public float Restitution = 0.5f;
	public float Friction = 0.75f;

	public bool UseTransforms;
	/// <summary> The polygon points. MUST BE COUNTER-CLOCKWISE </summary>
	public Transform[] PolygonTransforms;
	public Vector2[] PolygonCoordinates;

	[HideInInspector]
	public CollisionGroupDef CollisionFilter = CollisionGroupDef.None;
	[HideInInspector]
	public FSCollisionGroup CollisionGroup;

	[HideInInspector]
	public Category BelongsTo = Category.Cat1;
	[HideInInspector]
	public bool BelongsToFold;
	[HideInInspector]
	public Category CollidesWith = Category.All;
	[HideInInspector]
	public bool CollidesWithFold;

	void OnDrawGizmos() {
		// Draw children
		Gizmos.color = Color.red;
		if (transform.childCount > 0) {
			for (int i = 0; i < transform.childCount; i++)
				Gizmos.DrawWireSphere(transform.GetChild(i).position, 0.15f);
		}
		if (SType == ShapeType.Circle) {
			var sphereCollider = GetComponent<SphereCollider>();
			if (sphereCollider != null) {
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(transform.position, sphereCollider.radius * transform.lossyScale.x);
			}
		}
		//if (Application.isPlaying) return;
		if (UseTransforms)
			DrawTransformGizmos();
		else
			DrawCoordinatesGizmos();
	}

	private void DrawTransformGizmos() {
		if (PolygonTransforms == null) return;
		if (PolygonTransforms.Length < 3) return;

		Color color = Color.yellow;
		for (int i = 1; i < PolygonTransforms.Length; i++) {
			if (PolygonTransforms[i - 1] == null || PolygonTransforms[i] == null) continue;
			Gizmos.color = color;
			Gizmos.DrawLine(PolygonTransforms[i - 1].position, PolygonTransforms[i].position);
			color.r += 0.05f;
			color.g += 0.05f;
			color.b += 0.05f;
		}
		if (PolygonTransforms.Length == 0) return;
		if (PolygonTransforms[PolygonTransforms.Length - 1] == null || PolygonTransforms[0] == null) return;
		Gizmos.DrawLine(PolygonTransforms[PolygonTransforms.Length - 1].position, PolygonTransforms[0].position);
	}

	private void DrawCoordinatesGizmos() {
		if (PolygonCoordinates == null) return;
		if (PolygonCoordinates.Length < 3) return;

		Color color = Color.yellow;
		for (int i = 1; i < PolygonCoordinates.Length; i++) {
			Gizmos.color = color;
			Gizmos.DrawLine(transform.TransformPoint(PolygonCoordinates[i - 1]),
							transform.TransformPoint(PolygonCoordinates[i]));
			color.r += 0.05f;
			color.g += 0.05f;
			color.b += 0.05f;
		}
		Gizmos.DrawLine(transform.TransformPoint(PolygonCoordinates[PolygonCoordinates.Length - 1]),
						transform.TransformPoint(PolygonCoordinates[0]));
	}

	public Shape GetShape() {
		if (SType == ShapeType.Polygon) return GetPolyShape();

		if (SType == ShapeType.Circle) {
			var sphereCollider = GetComponent<SphereCollider>();
			if (sphereCollider == null)
				return null;
			return new CircleShape(sphereCollider.radius * transform.lossyScale.x, Density);
		}
		return null;
	}

	private PolygonShape GetPolyShape() {
		// if this GameObject contains the FSBodyComponent there is no need to adjust the colliders
		bool hasFsBody = GetComponent<FSBodyComponent>() != null;

		var shapePoints = new List<FVector2>();
		if (UseUnityCollider) {
			if (!GetColliderMeasurements(hasFsBody, ref shapePoints)) return null;
		} else {
			if (UseTransforms) {
				if (PolygonTransforms.Length < 3) return null;
				foreach (Transform t in PolygonTransforms)
					shapePoints.Add(hasFsBody ? FSHelper.Vector3ToFVector2(t.localPosition)
									: FSHelper.Vector3ToFVector2(FSHelper.LocalTranslatedVec3(
											transform.parent.InverseTransformPoint(t.position), transform.parent)));
			} else {
				if (PolygonCoordinates.Length < 3) return null;
				foreach (Vector2 t in PolygonCoordinates)
					shapePoints.Add(hasFsBody ? FSHelper.Vector3ToFVector2(t) : FSHelper.Vector3ToFVector2(t));
			}
		}
		return new PolygonShape(new Vertices(shapePoints.ToArray()), Density);
	}

	private bool GetColliderMeasurements(bool hasFsBody, ref List<FVector2> shapePoints) {
		// TODO: Add support for capsule colliders
		var box = GetComponent<BoxCollider>();
		if (box == null) return false;

		Vector3 scale = transform.lossyScale;

		// Added support for collider center offset (has to convert to local first)
		var center = transform.InverseTransformPoint(box.bounds.center);
		// Top left to Top Right points, counter-clockwise
		var v00l = new Vector3(-box.size.x * 0.5f, -box.size.y * 0.5f) + center;
		var v01l = new Vector3(box.size.x * 0.5f, -box.size.y * 0.5f) + center;
		var v02l = new Vector3(box.size.x * 0.5f, box.size.y * 0.5f) + center;
		var v03l = new Vector3(-box.size.x * 0.5f, box.size.y * 0.5f) + center;

		v00l.Scale(scale);
		v01l.Scale(scale);
		v02l.Scale(scale);
		v03l.Scale(scale);

		Vector3 v00 = hasFsBody ? v00l : FSHelper.LocalTranslatedVec3(v00l, transform);
		Vector3 v01 = hasFsBody ? v01l : FSHelper.LocalTranslatedVec3(v01l, transform);
		Vector3 v02 = hasFsBody ? v02l : FSHelper.LocalTranslatedVec3(v02l, transform);
		Vector3 v03 = hasFsBody ? v03l : FSHelper.LocalTranslatedVec3(v03l, transform);

		shapePoints.Add(FSHelper.Vector3ToFVector2(v00));
		shapePoints.Add(FSHelper.Vector3ToFVector2(v01));
		shapePoints.Add(FSHelper.Vector3ToFVector2(v02));
		shapePoints.Add(FSHelper.Vector3ToFVector2(v03));
		return true;
	}
}
