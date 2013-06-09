using System;
using UnityEngine;

public static class RageMagnetMath {

	public static void AverageMagnetPoint(this RageMagnetPoint ragePoint) {
		var pointsData = ragePoint.RelatedData;

		Vector3 pointPosSum = Vector3.zero;
		Vector3 inCtrlPosSum = Vector3.zero;
		Vector3 outCtrlPosSum = Vector3.zero;

		foreach(var relativeData in pointsData) {
			pointPosSum += relativeData.AbsolutePointPos;
			inCtrlPosSum += relativeData.AbsoluteInCtrlPos;
			outCtrlPosSum += relativeData.AbsoluteOutCtrlPos;
		}

		ISplinePoint point = ragePoint.Spline.GetPointAt(ragePoint.Index);
		point.Position = pointPosSum / pointsData.Length;
		point.InTangent = inCtrlPosSum/pointsData.Length;
		point.OutTangent =  outCtrlPosSum/pointsData.Length;
	}

	public static Quaternion GetTransformRotation(this RageMagnet magnet, float weight) {
		return Quaternion.Slerp(magnet.RestRotation, magnet.transform.rotation, weight) * Quaternion.Inverse(magnet.RestRotation);
	}

	public static Vector3 CalculateDeformations(this RageMagnet magnet, Vector3 pointPos, Vector3 posOffset){
		return pointPos + posOffset;
	}

	public static void CalculateDeformations(this RageMagnet magnet, RageMagnetPoint ragePoint, Vector3 posOffset, Vector3 scaleOffset) {
		RageMagnetPointData pointData = ragePoint.FindRelatedData(magnet);
		
		pointData.PointPos = pointData.InitialPos;
		pointData.InCtrlPos = pointData.InitialInCtrlPos;
		pointData.OutCtrlPos = pointData.InitialOutCtrlPos;

		ragePoint.ApplyDeformations(magnet, posOffset, scaleOffset);

		pointData.AbsolutePointPos = pointData.PointPos + magnet.RestPosition;
		pointData.AbsoluteInCtrlPos = pointData.InCtrlPos + magnet.RestPosition;
		pointData.AbsoluteOutCtrlPos = pointData.OutCtrlPos + magnet.RestPosition;
	}

	public static float SphericalProjectionDistance(this RageMagnet magnet, Vector3 centerPosition, Vector3 pointPos, Quaternion nullifyRotation) {
		return Vector3.Distance(SphericalProjection(magnet, pointPos, nullifyRotation), centerPosition);
	}

	public static Vector3 SphericalProjection(this RageMagnet magnet, Vector3 pointPosition, Quaternion nullifyRotation) {
		Collider cl = magnet.ActiveInnerCollider;
		if(cl == null) throw new Exception("Null collider");

		float clRadius;
		float clHeight;

		if(cl is CapsuleCollider) {
			var capsuleCollider = (CapsuleCollider)cl;
			clHeight = capsuleCollider.height;
			clRadius = capsuleCollider.radius;
		} else {
			clRadius = ((SphereCollider)cl).radius;
			clHeight = 0;
		}

		if((2 * clRadius) >= clHeight) return pointPosition;

		Vector3 relativePos = pointPosition - cl.bounds.center;
		pointPosition = (nullifyRotation * relativePos) + cl.bounds.center;

		float halfHeightOffset = (clHeight / 2) - clRadius;
		float yCenterPosition = cl.bounds.center.y;

		if((new Vector3(0f, pointPosition.y - yCenterPosition, 0f).magnitude <= halfHeightOffset))
			return new Vector3(pointPosition.x, yCenterPosition, pointPosition.z);

		return new Vector3(pointPosition.x, pointPosition.y > yCenterPosition ?
																					pointPosition.y - halfHeightOffset :
																					pointPosition.y + halfHeightOffset, pointPosition.z);
	}

	public static void AdjustOffsetCheck (Collider activeCollider, Vector3 pointPos, Vector3 inCtrlPos,
												Vector3 outCtrlPos,  ref float pointOffset, 
												ref float inCtrlOffset, ref float outCtrlOffset,
												float strengthMin, float strengthMax, Quaternion nullifyRotation) {
		if (!(activeCollider is CapsuleCollider)) return;

		var capsuleCollider = (CapsuleCollider)activeCollider;
		if (capsuleCollider.center.y == 0) return;

		Vector3 relativePos = pointPos - capsuleCollider.bounds.center;
		pointPos = (nullifyRotation * relativePos) + capsuleCollider.bounds.center;

		relativePos = inCtrlPos - capsuleCollider.bounds.center;
		inCtrlPos = (nullifyRotation * relativePos) + capsuleCollider.bounds.center;

		relativePos = outCtrlPos - capsuleCollider.bounds.center;
		outCtrlPos = (nullifyRotation * relativePos) + capsuleCollider.bounds.center;

		float pointPosY = pointPos.y;
		float inCtrlPosY = inCtrlPos.y;
		float outCtrlPosY = outCtrlPos.y;

		float halfHeightOffset = (capsuleCollider.height / 2) - capsuleCollider.radius;
		float top = capsuleCollider.bounds.center.y;
		float bottom = top;
		top += (capsuleCollider.center.y) < 0 ? halfHeightOffset : -halfHeightOffset;
		bottom += (capsuleCollider.center.y) > 0 ? halfHeightOffset : -halfHeightOffset;

		pointOffset = Mathf.Lerp(strengthMin, strengthMax, Mathf.InverseLerp(bottom, top, pointPosY));
		inCtrlOffset = Mathf.Lerp(strengthMin, strengthMax, Mathf.InverseLerp(bottom, top, inCtrlPosY));
		outCtrlOffset = Mathf.Lerp(strengthMin, strengthMax, Mathf.InverseLerp(bottom, top, outCtrlPosY));
	}
}
