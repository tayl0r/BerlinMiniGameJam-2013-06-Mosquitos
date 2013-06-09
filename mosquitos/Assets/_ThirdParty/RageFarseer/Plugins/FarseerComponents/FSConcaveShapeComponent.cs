using System.Globalization;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Category = FarseerPhysics.Dynamics.Category;

[AddComponentMenu("FarseerUnity/Collision/Concave Shape Component")]
public class FSConcaveShapeComponent : MonoBehaviour
{
	public float Density = 1f;
	public float Restitution = 0.5f;
	public float Friction = 0.75f;
	
	[HideInInspector]
	public CollisionGroupDef CollisionFilter = CollisionGroupDef.None;
	
	public FSCollisionGroup CollisionGroup;
	
	public Category BelongsTo = Category.Cat1;
	public bool BelongsToFold = false;
	public Category CollidesWith = Category.All;
	public bool CollidesWithFold = false;
	
	[HideInInspector]
	public Vector3[,] ConvertedVertices;
	
	[HideInInspector]
	public FSShapePointInput PointInput = FSShapePointInput.Transform;
	
	[HideInInspector]
	public Transform[] PointsTransforms;
	
	[HideInInspector]
	public Vector2[] PointsCoordinates;

	public static float GizmoScale;
	public float LocalGizmoScale {
		get { return GizmoScale; }
		set { if (GizmoScale == value) return; GizmoScale = value;
			ConvertToConvex();
		}
	}

	public virtual void OnDrawGizmos() {
		if (Application.isPlaying) return;
		if(PointInput == FSShapePointInput.Transform) {
			if (PointsTransforms == null) return;
			var pointList = new List<Transform>(PointsTransforms);
			foreach (Transform child in transform ) {
				bool childIsInList = pointList.Contains(child);
				Gizmos.color = childIsInList ? Color.magenta : Color.white;
				Gizmos.DrawWireSphere(child.position, childIsInList ? 0.13f : 0.1f);
			}
			DrawConnectionsFromTransforms();
		} else {
			Gizmos.color = Color.magenta;
			foreach (var item in PointsCoordinates)
				Gizmos.DrawWireSphere(transform.TransformPoint(item.x, item.y, transform.position.z), 0.13f);
			DrawConnectionsFromCoordinates();
		}
	}

	private void DrawConnectionsFromTransforms() {
		if (PointsTransforms == null) return;
		if (PointsTransforms.Length <= 2) return;
		Transform last = PointsTransforms[PointsTransforms.Length - 1];
		for (int i = 0; i < PointsTransforms.Length; i++) {
			if (last != null && PointsTransforms[i] != null) {
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine(last.position, PointsTransforms[i].position);
				// draw id
				Vector3 midPosition = last.position;
				midPosition += (PointsTransforms[i].position - last.position)/2f;
				Gizmos.color = Color.white;
				GizmosHelper.DrawString(midPosition, new Vector3(GizmoScale,GizmoScale), i.ToString());
			}
			last = PointsTransforms[i];
		}
	}

	private void DrawConnectionsFromCoordinates() {
		if (PointsCoordinates == null) return;
		if (PointsCoordinates.Length > 2) {
			Vector2 last = PointsCoordinates[PointsCoordinates.Length - 1];
			for (int i = 0; i < PointsCoordinates.Length; i++) {
				Gizmos.color = Color.magenta;
				var lastPosition = new Vector3 (last.x, last.y, transform.position.z);
				var currentPosition = new Vector3 (PointsCoordinates[i].x, PointsCoordinates[i].y, transform.position.z);
				Gizmos.DrawLine (transform.TransformPoint(lastPosition), transform.TransformPoint(currentPosition));
				// draw id
				Vector3 midPosition = lastPosition + (currentPosition - lastPosition) / 2f;
				Gizmos.color = Color.white;
				GizmosHelper.DrawString(transform.TransformPoint(midPosition), new Vector3(GizmoScale, GizmoScale), i.ToString());
				last = currentPosition;
			}
		}
	}

	public void ConvertToConvex() {
		FSShapeComponent[] childFsShapes = GetComponentsInChildren<FSShapeComponent>();

		foreach (FSShapeComponent shapeComponent in childFsShapes) {
			if (shapeComponent.gameObject == null) continue;
			DestroyImmediate(shapeComponent.gameObject);
		}
		// convert vertices
		var concaveVertices = new FarseerPhysics.Common.Vertices();

		if (PointInput == FSShapePointInput.Transform)
			for (int i = 0; i < PointsTransforms.Length; i++)
				concaveVertices.Add(FSHelper.Vector3ToFVector2(PointsTransforms[i].localPosition));

		if (PointInput == FSShapePointInput.Vector2List)
			foreach (var coordinate in PointsCoordinates)
				concaveVertices.Add(FSHelper.Vector2ToFVector2(transform.TransformPoint(coordinate)));

		List<FarseerPhysics.Common.Vertices> convexShapeVs =
			FarseerPhysics.Common.Decomposition.BayazitDecomposer.ConvexPartition(concaveVertices);

		for (int i = 0; i < convexShapeVs.Count; i++) {
			var newConvShape = new GameObject("convexShape" + i.ToString());
			newConvShape.transform.parent = transform;
			newConvShape.transform.localPosition = Vector3.zero;
			newConvShape.transform.localRotation = Quaternion.Euler(Vector3.zero);
			newConvShape.transform.localScale = Vector3.one;

			var shapeComponent = newConvShape.AddComponent<FSShapeComponent>();
			shapeComponent.CollidesWith = CollidesWith;
			shapeComponent.CollisionFilter = CollisionFilter;
			shapeComponent.BelongsTo = BelongsTo;
			shapeComponent.CollisionGroup = CollisionGroup;
			shapeComponent.Friction = Friction;
			shapeComponent.Restitution = Restitution;
			shapeComponent.Density = Density;
			shapeComponent.UseUnityCollider = false;
			shapeComponent.UseTransforms = (PointInput == FSShapePointInput.Transform);

			if (PointInput == FSShapePointInput.Transform) {
				shapeComponent.PolygonTransforms = new Transform[convexShapeVs[i].Count];
				for (int j = 0; j < convexShapeVs[i].Count; j++) {
					var pnew = new GameObject("p" + j.ToString(CultureInfo.InvariantCulture));
					pnew.transform.parent = shapeComponent.transform;
					pnew.transform.localPosition = FSHelper.FVector2ToVector3(convexShapeVs[i][j]);
					shapeComponent.PolygonTransforms[j] = pnew.transform;
				}
			} else {
				shapeComponent.PolygonCoordinates = new Vector2[convexShapeVs[i].Count];
				for (int j = 0; j < convexShapeVs[i].Count; j++)
					shapeComponent.PolygonCoordinates[j] = newConvShape.transform.InverseTransformPoint(FSHelper.FVector2ToVector3(convexShapeVs[i][j]));
			}
		}
	}

}
