using UnityEngine;
using System.Collections.Generic;

public class RageMagnetPoint : ScriptableObject {

	[SerializeField] private ScriptableObject _spline;
	public ISpline Spline{
		get { return (ISpline) _spline; }
		set { _spline = (ScriptableObject) value; }
	}

	public bool Changed;
	public int Index;

	public RageMagnetPointData[] RelatedData = new RageMagnetPointData[0];

	public static void RemoveMagnetsFromPoint(RageMagnet magnet, RageMagnetPoint[] points) {
		if(points == null) return;
		foreach(RageMagnetPoint point in points){
			if (point == null) continue;
			point.RemovePointData(magnet); 
		}
	}

	private void RemovePointData(RageMagnet magnet){
		List<RageMagnetPointData> points = new List<RageMagnetPointData>(RelatedData.Length);
		foreach(RageMagnetPointData pointData in RelatedData) 
			if(pointData.Magnet != magnet)
				points.Add(pointData);
		RelatedData = points.ToArray();
	}

	public static RageMagnetPoint Instantiate(RageMagnet magnet, ISpline rageSpline, int index,
												float pointOffset, float inCtrlOffset, float outCtrlOffset,
												Vector3 pointPos, Vector3 inCtrlPos, Vector3 outCtrlPos,
												Vector3 currentPosition) {

		List<RageMagnetPoint> points = GetAllMagnetPoints();
		RageMagnetPoint result = null;
		foreach (RageMagnetPoint point in points) {
			if(rageSpline.Oid != point.Spline.Oid) continue;
			if(index != point.Index) continue;
			result = point;
			break;
		}

		if(result ==null) result = CreateInstance(typeof(RageMagnetPoint)) as RageMagnetPoint;

		result.Spline = rageSpline;
		result.Index = index;

		result.RemovePointData(magnet);
		var magnetPointData = RageMagnetPointData.Instantiate(magnet, pointPos, inCtrlPos, outCtrlPos, currentPosition);
		result.AddRelatedData(magnetPointData);

		magnetPointData.PointOffset = pointOffset;
		magnetPointData.InCtrlOffset = inCtrlOffset;
		magnetPointData.OutCtrlOffset = outCtrlOffset;

		return result;
	}

	private void AddRelatedData(RageMagnetPointData newPointData){
		List<RageMagnetPointData> points = new List<RageMagnetPointData>(RelatedData.Length);
		foreach(RageMagnetPointData pointData in RelatedData)
			if(pointData.Magnet != newPointData.Magnet) 
				points.Add(pointData);

		points.Add(newPointData);
		RelatedData = points.ToArray();
	}

	public static List<RageMagnetPoint> GetAllMagnetPoints(RageGroup group=null){
		List<RageMagnetPoint> result = new List<RageMagnetPoint>();
		Object[] magnets = FindSceneObjectsOfType(typeof(RageMagnet));
		foreach(RageMagnet magnet in magnets) {
			if(group!=null && magnet.Group != group) continue;
			if(magnet.Points == null) continue;
			foreach(RageMagnetPoint point in magnet.Points) {
				if(result.Contains(point)) continue;
				if(point == null) continue;
				result.Add(point);
			}            
		}
		
		return result;
	}

	private void ApplyRotation(RageMagnet magnet) {
		var relativeData = FindRelatedData(magnet);
		ApplyRotation(magnet, ref relativeData.PointPos, relativeData.PointOffset);
		ApplyRotation(magnet, ref relativeData.InCtrlPos, relativeData.InCtrlOffset);
		ApplyRotation(magnet, ref relativeData.OutCtrlPos, relativeData.OutCtrlOffset);
	}

	public RageMagnetPointData FindRelatedData(Object magnet){
		foreach(RageMagnetPointData pointData in RelatedData)
			if(pointData.Magnet == magnet) return pointData;
		return null;
	}

	private void ApplyScale(RageMagnet magnet, Vector3 scaleOffset) {
		RageMagnetPointData pointData = FindRelatedData(magnet);
		ApplyScale(ref pointData.PointPos, Vector3.one + pointData.PointOffset * scaleOffset);
		ApplyScale(ref pointData.InCtrlPos, Vector3.one + pointData.InCtrlOffset * scaleOffset);
		ApplyScale(ref pointData.OutCtrlPos, Vector3.one + pointData.OutCtrlOffset * scaleOffset);
	}

	private void ApplyPosition(RageMagnet magnet, Vector3 baseOffset) {
		RageMagnetPointData pointData = FindRelatedData(magnet);
		pointData.PointPos += baseOffset * pointData.PointOffset;
		pointData.InCtrlPos += baseOffset * pointData.InCtrlOffset;
		pointData.OutCtrlPos += baseOffset * pointData.OutCtrlOffset;
	}

	private static void ApplyRotation(RageMagnet magnet, ref Vector3 position, float offset) {
		Quaternion proportionalRotationOffset = magnet.GetTransformRotation(offset);
		position = (proportionalRotationOffset * position);
	}

	private static void ApplyScale(ref Vector3 position, Vector3 proportionalScaleOffset) {
		position = new Vector3(proportionalScaleOffset.x * position.x,
								proportionalScaleOffset.y * position.y,
								proportionalScaleOffset.z * position.z);
	}

	public void ApplyDeformations(RageMagnet magnet, Vector3 posOffset, Vector3 scaleOffset) {
		Changed = true;
		ApplyRotation(magnet);
		ApplyPosition(magnet, posOffset);
		ApplyScale(magnet, scaleOffset);
	}

	public static void ReloadAllMagnetsPoints(){
		List<RageMagnetPoint> points = GetAllMagnetPoints();
		foreach(RageMagnetPoint point in points) 
			point.RelatedData = new RageMagnetPointData[0];

		Object[] magnets = FindSceneObjectsOfType(typeof (RageMagnet));
		foreach(RageMagnet magnet in magnets)
			magnet.UpdateRestPosition();
	}

	public override string ToString(){ return "Point( " + Spline.Oid + " : " + Index + " )"; }
}
