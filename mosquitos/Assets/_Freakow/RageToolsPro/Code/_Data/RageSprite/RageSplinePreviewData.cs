using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RageSplinePreviewData
{
	private Rect _boundary;
	public Rect Boundary { get { return _boundary; } }
	private readonly List<RageSplinePreviewPointData> _points;
	#if UNITY_EDITOR
	private static readonly Color LightSkinPreviewColor = new Color(0.33f, 0.33f, 0.33f);
	private static readonly Color DarkSkinPreviewColor = new Color(0.82f, 0.82f, 0.82f);
	#endif

	public Vector2 Center{
		get{ return _boundary.center; }
		set{
			if (_boundary.center.Equals(value)) return;
			Vector2 positionOffset = _boundary.center - value;
			_boundary.center = value;
			foreach (RageSplinePreviewPointData pointData in _points) {
				pointData.Position -= positionOffset;
				pointData.StartTangent -= positionOffset;
				pointData.EndTangent -= positionOffset;
			}
		}
	}

	public void Scale(float amount){
		if (Mathf.Approximately(amount, 1f)) return;
		Vector2 boundaryCenter = _boundary.center;
		_boundary.height *= amount;
		_boundary.width *= amount;
		_boundary.center = boundaryCenter;
		foreach (RageSplinePreviewPointData pointData in _points) {
			pointData.Position = ((pointData.Position - boundaryCenter) * amount) + boundaryCenter;
			pointData.StartTangent = ((pointData.StartTangent - boundaryCenter) * amount) + boundaryCenter;
			pointData.EndTangent = ((pointData.EndTangent - boundaryCenter) * amount) + boundaryCenter;
		}
	}

	public RageSplinePreviewData(RageSpline spline) {
		_points = new List<RageSplinePreviewPointData>();
		for (int i = 0; i <= spline.GetPointCount() - 1; i++){
			var pointPosition = spline.GetPosition(i);
			if (i == 0) {
				_boundary = new Rect(pointPosition.x, pointPosition.y, 0, 0);
			} else {
				if (_boundary.xMin > pointPosition.x) _boundary.xMin = pointPosition.x;
				if (_boundary.xMax < pointPosition.x) _boundary.xMax = pointPosition.x;
				if (_boundary.yMin > pointPosition.y) _boundary.yMin = pointPosition.y;
				if (_boundary.yMax < pointPosition.y) _boundary.yMax = pointPosition.y;
			}
			_points.Add(new RageSplinePreviewPointData {	Position = pointPosition,
															StartTangent = spline.GetInControlPosition(i),
															EndTangent = spline.GetOutControlPosition(i)
													   });
		}
	}

	public void Draw(Rect drawArea, bool fastPreview) {
		#if UNITY_EDITOR
			if (_points.Count < 2) return;
			Color previewColor = EditorGUIUtility.isProSkin ? DarkSkinPreviewColor : LightSkinPreviewColor;
			if (fastPreview) {
				var fastPreviewPoints = new Vector3[_points.Count + 1];
				for (int i = 0; i <= _points.Count - 1; i++) {
					fastPreviewPoints[i] = FlipY (_points[i].Position, drawArea);
				}
				fastPreviewPoints[_points.Count] = FlipY(_points[0].Position, drawArea);
				Handles.DrawPolyLine (fastPreviewPoints);
				return;
			}
			for (int i = 0; i < _points.Count - 1; i++) {
				var startPoint = FlipY(_points[i].Position, drawArea)	;
				var endPoint = FlipY(_points[i + 1].Position, drawArea);
				Handles.DrawBezier(startPoint, endPoint,
					FlipY(_points[i].EndTangent, drawArea), FlipY(_points[i + 1].StartTangent, drawArea),
					previewColor, null, 1.5f);
			}
			var finalStartPoint = FlipY(_points[_points.Count - 1].Position, drawArea);
			var finalEndPoint = FlipY(_points[0].Position, drawArea);
			Handles.DrawBezier(finalStartPoint, finalEndPoint,
				FlipY(_points[_points.Count - 1].EndTangent, drawArea), FlipY(_points[0].StartTangent, drawArea), 
				previewColor, null, 1.5f);
		#endif
	}

	private Vector2 FlipY(Vector2 pointPositon, Rect drawArea) {
		float yPosition = pointPositon.y - drawArea.yMin;
		yPosition = drawArea.yMax - yPosition;
		pointPositon.y = yPosition;
		return pointPositon;
	}
}
