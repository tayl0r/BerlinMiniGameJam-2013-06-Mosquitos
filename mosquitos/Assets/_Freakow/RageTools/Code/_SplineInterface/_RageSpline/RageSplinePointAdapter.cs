using UnityEngine;

public class RageSplinePointAdapter : ScriptableObject, ISplinePoint {

    public RageSpline RageSpline;

    public int Index { get; set; }

    public Vector3 Position{
        get { return RageSpline.GetPositionWorldSpace(Index); }
        set { RageSpline.SetPointWorldSpace(Index, value); }
    }

    public Vector3 InTangent {
        get { return RageSpline.GetInControlPositionWorldSpace(Index); }
        set { RageSpline.SetInControlPositionWorldSpace(Index, value); }
    }


    public Vector3 PositionLocal {
        get { return RageSpline.GetPosition(Index); }
        set { RageSpline.SetPoint(Index, value); }
    }

    public Vector3 InTangentLocal {
        get { return RageSpline.GetInControlPositionPointSpace(Index); }
        set { RageSpline.SetInControlPositionPointSpace(Index, value); }
    }

    public Vector3 OutTangent {
        get { return RageSpline.GetOutControlPositionWorldSpace(Index); }
        set { RageSpline.SetOutControlPositionWorldSpace(Index, value); }
    }

    public Vector3 OutTangentLocal {
        get { return RageSpline.GetOutControlPositionPointSpace(Index); }
        set { RageSpline.SetOutControlPositionPointSpace(Index, value); }
    }

    public bool Smooth {
        get { return RageSpline.GetNatural(Index); }
        set { RageSpline.SetNatural(Index, value); }
    }
}