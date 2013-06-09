using UnityEngine;

public class RageSvgStyle : ScriptableObject {

	public Spline.OutlineType OutlineType;
	public Color OutlineColor1, OutlineColor2;
	public float OutlineAlpha, OutlineWidth;
	public Spline.CornerType CornersType;
	public Spline.FillType FillType;
	public RageSvgGradient RageSvgGradient;
	public Color FillColor1, FillColor2;
	public float FillColor1Alpha, FillColor2Alpha;
	public bool HasFill, HasOutline, HasGradient;

	public RageSvgStyle() {
		OutlineType = Spline.OutlineType.None;
		OutlineColor1 = Color.black;
		OutlineColor2 = Color.black;
		OutlineAlpha = 1f;
		OutlineWidth = 1f;
		CornersType = Spline.CornerType.Beak; //Was: Default
		FillType = Spline.FillType.Solid;
		RageSvgGradient = null;
		FillColor1 = Color.black;
		FillColor2 = Color.black;
		FillColor1Alpha = 1f;
		FillColor2Alpha = 1f;
		HasFill = true;
		HasOutline = false;
		HasGradient = false;
	}

	public static RageSvgStyle NewInstance(){
		return CreateInstance(typeof(RageSvgStyle)) as RageSvgStyle;
	}

	public void CopyDataFrom(RageSvgStyle style) {
		OutlineType = style.OutlineType;
		OutlineColor1 = style.OutlineColor1;
		OutlineColor2 = style.OutlineColor2;
		OutlineAlpha = style.OutlineAlpha;
		OutlineWidth = style.OutlineWidth;
		CornersType = style.CornersType;
		FillType = style.FillType;
		RageSvgGradient = style.RageSvgGradient;
		FillColor1 = style.FillColor1;
		FillColor2 = style.FillColor2;
		FillColor1Alpha = style.FillColor1Alpha;
		FillColor2Alpha = style.FillColor2Alpha;
		HasFill = style.HasFill;
		HasOutline = style.HasOutline;
		HasGradient = style.HasGradient;
	}

	public string Debug() {
		return ("Style Fill: " + FillType + " :: Outline: " + OutlineType);
	}
}
