using UnityEngine;
using Com.Freakow.RageTools.Extensions;

public class RageSplineAdapter : ScriptableObject, ISpline{

	public bool Triangulate {
		get { return _rageSpline.spline.Triangulate; }
		set { _rageSpline.spline.Triangulate = value; }
	}

	public bool FixUvs {
		get { return _rageSpline.FixUvs; }
		set { _rageSpline.FixUvs = value; }
	}

	public float ProgressiveNormalsPrecalc {
		get { return _rageSpline.spline.PercentPrecalcNormals ; }
		set {
			if(value > 1){
				_rageSpline.spline.PercentPrecalcNormals = 1;
				return;
			}
			if(value < 0) {
				_rageSpline.spline.PercentPrecalcNormals = 0;
				return;
			}
			_rageSpline.spline.PercentPrecalcNormals = value;
		}
	}

	public Color OutlineColor {
		get { return _rageSpline.outlineColor1; }
		set { _rageSpline.SetOutlineColor1(value); }
	}

	[SerializeField] 
	private  RageSpline _rageSpline;
	private int _currentStartNormals;

	public void SetSpline(MonoBehaviour rageSpline){ 
		_rageSpline = (RageSpline) rageSpline;

		FillGradient = CreateInstance<RageSplineGradientAdapter>();
		Outline = CreateInstance<RageSplineOutlineAdapter>();

		((RageSplineGradientAdapter)FillGradient).RageSpline = _rageSpline;
		((RageSplineOutlineAdapter)Outline).RageSpline = _rageSpline;
	}

	[SerializeField] private ScriptableObject _fillGradient;
	public ISplineGradient FillGradient {
		get { return (ISplineGradient) _fillGradient; }
		set { _fillGradient = (ScriptableObject) value; }
	}

	public bool IsDirty {
		get { return _rageSpline.isDirty; }
		set { if (_rageSpline) _rageSpline.isDirty = value; }
	}

	[SerializeField] private ScriptableObject _outline;
	public ISplineOutline Outline {
		get { return (ISplineOutline)_outline; }
		set { _outline = (ScriptableObject)value; }
	}

	[SerializeField]private ScriptableObject _outlineGradient;
	public ISplineGradient OutlineGradient {
		get { return (ISplineGradient) _outlineGradient; }
		set { _outlineGradient = (ScriptableObject)value; }
	}

	public Spline.OutlineType OutlineType {
		get { return Adapt(_rageSpline.GetOutline()); }
		set { _rageSpline.SetOutline(Adapt(value)); }
	}

	public GameObject GameObject { get { return _rageSpline.gameObject; } }
	public MonoBehaviour MonoBehaviour {get { return _rageSpline; }}
	public long Oid { get { return _rageSpline.GetHashCode(); } }

	public bool Changed { get; set; }

	public bool OutlineBehindFill{
		get { return _rageSpline.inverseTriangleDrawOrder; }
		set { _rageSpline.inverseTriangleDrawOrder = value; }
	}

	public float OutlineWidth {
		get { return _rageSpline.OutlineWidth; }
		set { _rageSpline.SetOutlineWidth(value); }
	}

	public Color FillColor{
		get { return _rageSpline.GetFillColor1(); }
		set { _rageSpline.SetFillColor1(value); }
	}

	public Spline.FillType FillType {
		get { return Adapt(_rageSpline.GetFill()); }
		set { _rageSpline.SetFill(Adapt(value)); }
	}

	public Spline.CornerType CornerType{
		get { return Adapt(_rageSpline.GetCorners()); }
		set { _rageSpline.SetCorners(Adapt(value)); }
	}

	private int VerticesCount {
		get { return _rageSpline.GetVertexCount(); }
		set { _rageSpline.SetVertexCount(value); }
	}

	public Rect Bounds { get { return _rageSpline.GetBounds(); }}

	public RageSpline Rs {	get { return _rageSpline; }
							set { _rageSpline = value; }
	}

	private static Spline.FillType Adapt(RageSpline.Fill fill) {
		switch(fill) {
			case RageSpline.Fill.Gradient: return Spline.FillType.Gradient;
			case RageSpline.Fill.Landscape: return Spline.FillType.Landscape;
			case RageSpline.Fill.None: return Spline.FillType.None;
			case RageSpline.Fill.Solid:  return Spline.FillType.Solid;
			default: return Spline.FillType.None;
		}
	}

	private static RageSpline.Fill Adapt(Spline.FillType fillType) {
		switch(fillType) {
			case Spline.FillType.Gradient: return RageSpline.Fill.Gradient;
			case Spline.FillType.Landscape: return RageSpline.Fill.Landscape;
			case Spline.FillType.None: return RageSpline.Fill.None;
			case Spline.FillType.Solid: return RageSpline.Fill.Solid;
			default: return RageSpline.Fill.None;
		}
	}

	private static Spline.OutlineType Adapt(RageSpline.Outline outline) {
		switch (outline) {
			case RageSpline.Outline.None: return Spline.OutlineType.None;
			case RageSpline.Outline.Free: return Spline.OutlineType.Free;
			case RageSpline.Outline.Loop: return Spline.OutlineType.Loop;
			default: return Spline.OutlineType.None;
		}
	}

	private static RageSpline.Outline Adapt(Spline.OutlineType outlineType) {
		switch(outlineType) {
			case Spline.OutlineType.None: return RageSpline.Outline.None;
			case Spline.OutlineType.Free: return RageSpline.Outline.Free;
			case Spline.OutlineType.Loop: return RageSpline.Outline.Loop;
			default: return RageSpline.Outline.None;
		}
	}

	private static Spline.CornerType Adapt(RageSpline.Corner corner) {
		switch(corner) {
			case RageSpline.Corner.Beak:return Spline.CornerType.Beak;
			case RageSpline.Corner.Default:return Spline.CornerType.Default;
			default:return Spline.CornerType.Default;
		}
	}

	private static RageSpline.Corner Adapt(Spline.CornerType cornerType) {
		switch(cornerType) {
			case Spline.CornerType.Beak: return RageSpline.Corner.Beak;
			case Spline.CornerType.Default:return RageSpline.Corner.Default;
			default: return RageSpline.Corner.Default;
		}
	}


	public bool PointsAreInClockWiseOrder() {
		return _rageSpline.pointsAreInClockWiseOrder();
	}

	public void FlipPointOrder() {
		_rageSpline.flipPointOrder();
	}

	public void Redraw(bool draft = false) {
		Redraw(true, true, true, draft);
	}

	public void Redraw(bool calculateNormals, bool calculatePhysics) {
		//Redraw(Triangulate, calculateNormals, calculatePhysics);
		Redraw(true, calculateNormals, calculatePhysics, false);
	}

	public void Redraw(bool triangulate, bool calculateNormals, bool calculatePhysics, bool draft) {
		if (_rageSpline==null) return;
		_rageSpline.FlattenZ = false;
		_rageSpline.FixOverlaps = !draft;
		if (!draft) {
			_rageSpline.RefreshMeshInEditor (true, true, true);
			//_rageSpline.Redraw (true, true, true, 0, 1f);
			return;
		}
		_currentStartNormals = _rageSpline.Redraw (_rageSpline.spline.Triangulate, calculateNormals, calculatePhysics, _currentStartNormals, 0.5f); // ProgressiveNormalsPrecalc);
	}

	public int VertexDensity {
		get { return VerticesCount/PointsCount; }
		set{
			if(value < 1) {
				VerticesCount = PointsCount;
				return;
			}
			VerticesCount = PointsCount * value;               
		}
	}

	public void RemoveAllPoints() { _rageSpline.ClearPoints(); }
	public void RemovePointAt(int index) { _rageSpline.RemovePoint(index); }
	public void RemovePoint(ISplinePoint point) { _rageSpline.RemovePoint(point.Index); }

	public ISplinePoint GetPointAt(int index) { return InstantiateISplinePoint(index); }

	public Mesh SharedMesh{
		get { return _rageSpline.GetComponent<MeshFilter>().sharedMesh; }
	}

	public bool Optimize {
		get { return _rageSpline.GetOptimize(); }
		set { _rageSpline.SetOptimize(value); }
	}

	public float OptimizeAngle {
		get { return _rageSpline.GetOptimizeAngle(); }
		set { _rageSpline.SetOptimizeAngle(value); }
	}

	public bool IsVisible() {
		//TODO: implement this
		return true;
	}

	public int RemoveOverlap(int index, float snapRadius, bool debug){
		var p0 = GetPointAt(index - 1);
		var p1 = GetPointAt(index);

		if(index <= 0) {
			if(Vector3.Distance(p0.Position, p1.Position) < snapRadius) {
				if(debug) Debug.Log("overlap path pos = " + p0.Position);

				p1.Smooth = false;
				p1.InTangent = p0.Position;

				RemovePoint(p0);
				return index;
			}
		}

		return (index + 1);
	}

	public int PointsCount { get { return _rageSpline.GetPointCount(); } }

	public ISplinePoint AddPointLocal(int index, Vector3 position, Vector3 inTangent) {
		_rageSpline.AddPoint(index, position, inTangent);
		return InstantiateISplinePoint(index);
	}

	public ISplinePoint AddPointLocal(int index, Vector3 position, Vector3 inTangent, Vector3 outTangent, float width, bool smooth) {
		_rageSpline.AddPoint(index, position, inTangent, outTangent, width, smooth);
		return InstantiateISplinePoint(index);
	}

	public ISplinePoint AddPoint(int index, Vector3 position, Vector3 outTangent) {
		_rageSpline.AddPointWorldSpace(index, position, outTangent);
		return InstantiateISplinePoint(index);
	}

	private ISplinePoint InstantiateISplinePoint(int index) {
		var point = CreateInstance<RageSplinePointAdapter>();
		point.Index = index;
		point.RageSpline = _rageSpline;
		return point;
	}
}
