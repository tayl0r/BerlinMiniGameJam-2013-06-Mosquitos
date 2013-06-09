using System;
using System.Collections.Generic;
using UnityEngine;

public class RageSpritePreviewData
{
	public List<RageSplinePreviewData> FramePreviewData;
	public Rect Boundary;

	public Vector2 Center {
		get { return Boundary.center; }
		set {
			if (Boundary.center.Equals(value)) return;
			Vector2 positionOffset = Boundary.center - value;
			Boundary.center = value;
			foreach (RageSplinePreviewData splinePreview in FramePreviewData)
				splinePreview.Center -= positionOffset;
		}
	}

	public void Scale(float amount) {
		if (Mathf.Approximately (amount, 1f)) return;
		Vector2 boundaryCenter = Boundary.center;
		Boundary.height *= amount;
		Boundary.width *= amount;
		Boundary.center = boundaryCenter;
		foreach (RageSplinePreviewData splinePreview in FramePreviewData) {
			splinePreview.Center = ((splinePreview.Center - Boundary.center) * amount) + Boundary.center;
			splinePreview.Scale(amount);
		}
	}

	public RageSpritePreviewData(GameObject frameGameObject) {
		FramePreviewData = new List<RageSplinePreviewData>();
		if (frameGameObject == null) return;
		RageSpline[] splines = frameGameObject.GetComponentsInChildren<RageSpline>();
		bool firstSprite = true;
		foreach (RageSpline spline in splines) {
			var splinePreview = new RageSplinePreviewData(spline);
			FramePreviewData.Add(splinePreview);
			Rect currentBoundary = splinePreview.Boundary;
			if (firstSprite) {
				Boundary = new Rect(currentBoundary);
				firstSprite = false;
				continue;
			}
			if (currentBoundary.xMin < Boundary.xMin) Boundary.xMin = currentBoundary.xMin;
			if (currentBoundary.yMin < Boundary.yMin) Boundary.yMin = currentBoundary.yMin;
			if (currentBoundary.xMax > Boundary.xMax) Boundary.xMax = currentBoundary.xMax;
			if (currentBoundary.yMax > Boundary.yMax) Boundary.yMax = currentBoundary.yMax;
		}
	}

	public void Draw(Rect drawArea, bool fastPreview){
		//if (_framePreviewData.Count == 0) return;
		FixPreviewScale(drawArea);
		Center = new Vector2(drawArea.x + Boundary.width/2, drawArea.center.y);
		foreach (RageSplinePreviewData splinePreview in FramePreviewData)
			splinePreview.Draw(Boundary, fastPreview);
	}

	private void FixPreviewScale(Rect drawArea) {
		float bound;
		float draw;

		if (Boundary.height > Boundary.width){
			bound = Boundary.height;
			draw = drawArea.height;
		} else{
			bound = Boundary.width;
			draw = drawArea.width;
		}
	   
		if (Mathf.Approximately(bound, draw)) return;
		float scaleOffset = draw / bound;
		Scale(scaleOffset);
	}
}

