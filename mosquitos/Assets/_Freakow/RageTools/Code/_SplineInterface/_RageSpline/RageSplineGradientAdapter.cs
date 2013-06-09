using UnityEngine;

public class RageSplineGradientAdapter: ScriptableObject, ISplineGradient {

    public RageSpline RageSpline;

    public Vector2 Offset {
        get { return RageSpline.GetGradientOffset(); }
        set { RageSpline.SetGradientOffset(value); }
    }

    public float Scale {
        get { return RageSpline.gradientScale; }
        set { RageSpline.gradientScale = value; }
    }

    public float Angle {
        get { return RageSpline.GetGradientAngleDeg(); }
        set { RageSpline.SetGradientAngleDeg(value); }
    }

    public bool StyleLocalPositioning {
        get { return RageSpline.styleLocalTexturePositioning; }
		set { RageSpline.styleLocalTexturePositioning = value; }
    }

    public Color StartColor {
        get { return RageSpline.GetFillColor1(); }
        set { RageSpline.SetFillColor1(value); }
    }

    public Color EndColor {
        get { return RageSpline.GetFillColor2(); }
        set { RageSpline.SetFillColor2(value); }
    }
}