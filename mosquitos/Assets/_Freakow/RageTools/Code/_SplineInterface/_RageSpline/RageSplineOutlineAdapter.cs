using UnityEngine;

public class RageSplineOutlineAdapter : ScriptableObject, ISplineOutline {

	public RageSpline RageSpline;

	public float AntialiasingWidth {
		get { if (RageSpline) return RageSpline.GetAntialiasingWidth(); return 0; }
		set { if (RageSpline) RageSpline.SetAntialiasingWidth(value); }
	}

	public Color StartColor {
		get { return RageSpline.GetOutlineColor1(); }
		set { RageSpline.SetOutlineColor1(value); }
	}

	public Color EndColor {
		get { return RageSpline.GetOutlineColor2(); }
		set { RageSpline.SetOutlineColor2(value); }
	}

	public Spline.OutlineType Type {
		get { return Adapt(RageSpline.GetOutline()); }
		set { RageSpline.SetOutline(Adapt(value)); }
	}

	public Spline.CornerType CornerType {
		get { return Adapt(RageSpline.GetCorners()); }
		set { RageSpline.SetCorners(Adapt(value)); }
	}

	public float GetWidth(int index) {
		return RageSpline.GetOutlineWidth(index);
	}

	public float Width {
		get { return RageSpline.GetOutlineWidth(); }
		set { RageSpline.SetOutlineWidth(value); }
	}

	private static RageSpline.Outline Adapt(Spline.OutlineType outlineType) {
		switch(outlineType) {
			case Spline.OutlineType.Free: return RageSpline.Outline.Free;
			case Spline.OutlineType.Loop: return RageSpline.Outline.Loop;
			case Spline.OutlineType.None: return RageSpline.Outline.None;
			default: return RageSpline.Outline.None;
		}
	}

	private static Spline.OutlineType Adapt(RageSpline.Outline outline) {
		switch(outline) {
			case RageSpline.Outline.Free: return Spline.OutlineType.Free;
			case RageSpline.Outline.Loop: return Spline.OutlineType.Loop;
			case RageSpline.Outline.None: return Spline.OutlineType.None;
			default: return Spline.OutlineType.None;
		}
	}

	private static Spline.CornerType Adapt(RageSpline.Corner corner) {
		switch (corner) {
			case RageSpline.Corner.Default:
				return Spline.CornerType.Default;
			case RageSpline.Corner.Beak:
				return Spline.CornerType.Beak;
			default:
				return Spline.CornerType.Default;
		}
	}

	private RageSpline.Corner Adapt(Spline.CornerType corner) {
		switch (corner) {
			case Spline.CornerType.Default:
				return RageSpline.Corner.Default;
			case Spline.CornerType.Beak:
				return RageSpline.Corner.Beak;
			default:
				return RageSpline.Corner.Default;
		}
	}
}
