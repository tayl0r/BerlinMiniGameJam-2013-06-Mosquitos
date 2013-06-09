using UnityEngine;

[System.Serializable]
public class RageGroupStyleData : ScriptableObject {

	public Spline.FillType FillType = Spline.FillType.Solid;	// { None = 0, Solid, Gradient };
	public Color FillColor = Color.gray;
	#region public FillGradient
	[SerializeField]
	private RageGroupGradientData _fillGradient;
	public RageGroupGradientData FillGradient {
		get {
			if (_fillGradient != null)
				return _fillGradient;
			_fillGradient = CreateInstance<RageGroupGradientData>();
			return _fillGradient;
		}
		set {
			if (value == _fillGradient)
				return;
			_fillGradient = value;
		}
	}
	#endregion

	public Color OutlineColor = Color.black;
	public Spline.OutlineType OutlineType = Spline.OutlineType.None;
	public float OutlineWidth = 1.0f;
	#region public OutlineGradient
	[SerializeField]
	private RageGroupGradientData _outlineGradient;
	public RageGroupGradientData OutlineGradient {
		get {
			if (_outlineGradient != null)
				return _outlineGradient;
			_outlineGradient = CreateInstance<RageGroupGradientData>();
			return _outlineGradient;
		}
		set {
			if (value == _outlineGradient)
				return;
			_outlineGradient = value;
		}
	}
	#endregion
	public float OutlineNormalOffset = 1f;

	public bool OutlineBehindFill;
	public bool Optimize;
	public float OptimizeAngle = 5f;

	public float OutlineTexturingScale = 0.1f;
	public RageSpline.UVMapping UvMapping1 = RageSpline.UVMapping.Fill; // { None = 0, Fill, Outline, OutlineFree };
	public RageSpline.UVMapping UvMapping2 = RageSpline.UVMapping.None;
	public float LandscapeBottomDepth;
	public float LandscapeOutlineAlign;

// TODO:
// 	public Vector2 textureOffset = new Vector2(0f, 0f);
// 	public float textureAngle;
// 	public float textureScale = 10f;
// 	public Vector2 textureOffset2 = new Vector2(0f, 0f);
// 	public float textureAngle2;
// 	public float textureScale2 = 10f;
// 
// 	public RageSpline.Emboss emboss = RageSpline.Emboss.None; 	// { None = 0, Sharp, Blurry };
// 	public Color embossColor1 = Color.white;
// 	public Color embossColor2 = Color.black;
// 	public float embossAngle = 180f;
// 	public float embossOffset = 0.5f;
// 	public float embossSize = 10f;
// 	public float embossCurveSmoothness = 3f;


	public void ApplyToSpline(ISpline spline) {
		spline.FillColor = FillColor;
		spline.OutlineColor = OutlineColor;

		spline.FillType = FillType;
		FillGradient.AssignGradient(spline.FillGradient);
		OutlineGradient.AssignGradient(spline.OutlineGradient);

		//spline.Outline = OutlineType;
		spline.OutlineType = OutlineType;
		spline.OutlineWidth = OutlineWidth;
		/*	spline.OutlineGradient = OutlineGradient;*/
		spline.OutlineBehindFill = OutlineBehindFill;
		spline.Optimize = Optimize;
		spline.OptimizeAngle = OptimizeAngle;
		//spline.Redraw();
	}

	
	
}
