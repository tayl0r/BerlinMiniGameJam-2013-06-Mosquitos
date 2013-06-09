using UnityEngine;

public interface ISpline {

	Rect Bounds { get; }
	Color FillColor { get; set; }
	bool OutlineBehindFill { get; set; }
	float ProgressiveNormalsPrecalc { get; set; }
	bool Triangulate { get; set; }
	bool FixUvs { get; set; }

	Color OutlineColor { get; set; }
	Spline.FillType FillType { get; set; }    
	ISplineGradient FillGradient { get; set; }

	ISplineOutline Outline { get; set; }
	ISplineGradient OutlineGradient { get; set; }
	float OutlineWidth { get; set; }
	Spline.OutlineType OutlineType { get; set; }

	GameObject GameObject { get; }
	MonoBehaviour MonoBehaviour { get; }

	long Oid { get; }
	bool Changed { get; set; }

	void Redraw (bool draft = false);
	void Redraw (bool calculateNormals, bool calculatePhysics);
	void Redraw (bool triangulate, bool calculateNormals, bool calculatePhysics, bool draft);

	int VertexDensity { get; set; }
	int RemoveOverlap(int index, float snapRadius, bool debug);

	bool PointsAreInClockWiseOrder();
	void FlipPointOrder();
	bool IsDirty { get; set; }

	int PointsCount { get; }
	void RemoveAllPoints();
	void RemovePointAt(int index);
	void RemovePoint(ISplinePoint point);

	ISplinePoint AddPointLocal(int index, Vector3 position, Vector3 inTangent);
	ISplinePoint AddPointLocal(int index, Vector3 position, Vector3 inTangent, Vector3 outTangent, float width, bool smooth);
	ISplinePoint AddPoint(int index, Vector3 position, Vector3 outTangent);

	ISplinePoint GetPointAt(int index);
	RageSpline Rs { get; set; }

	Mesh SharedMesh { get; }
	bool Optimize { get; set; }
	float OptimizeAngle { get; set; }
	bool IsVisible();
}

public interface ISplineGradient {

	Vector2 Offset { set; get; }
	float Scale { set; get; }
	float Angle { set; get; }

	Color StartColor { get; set; }
	Color EndColor { set; get; }

	bool StyleLocalPositioning { set; get; } //TODO
}

public interface ISplinePoint {

	int Index { get; }

	Vector3 Position { get; set; }
	Vector3 InTangent { get; set; }
	Vector3 OutTangent { get; set; }

	Vector3 PositionLocal { get; set; }
	Vector3 InTangentLocal { get; set; }
	Vector3 OutTangentLocal { get; set; }

	bool Smooth { get; set; }
}

public interface ISplineOutline {

	Spline.OutlineType Type { set; get; }
	Color StartColor { set; get; }
	Color EndColor { set; get; }
	float Width { set; get; }
	float GetWidth(int index);
	float AntialiasingWidth { get; set; }
	Spline.CornerType CornerType { set; get; }
}