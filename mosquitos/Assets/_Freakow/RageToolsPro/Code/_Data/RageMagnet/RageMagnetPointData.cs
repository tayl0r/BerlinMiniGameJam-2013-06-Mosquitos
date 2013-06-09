using UnityEngine;

public class RageMagnetPointData : ScriptableObject{

    public RageMagnet Magnet;

    public Vector3 PointPos;
    public Vector3 InCtrlPos;
    public Vector3 OutCtrlPos;

    public Vector3 InitialPos;
    public Vector3 InitialInCtrlPos;
    public Vector3 InitialOutCtrlPos;

    public Vector3 AbsolutePointPos;
    public Vector3 AbsoluteInCtrlPos;
    public Vector3 AbsoluteOutCtrlPos;

    public float InCtrlOffset;
    public float OutCtrlOffset;
    public float PointOffset;

    public static RageMagnetPointData Instantiate(RageMagnet magnet, Vector3 pointPos, Vector3 inCtrlPos, Vector3 outCtrlPos, Vector3 currentPosition) {
        var point = (RageMagnetPointData)CreateInstance(typeof(RageMagnetPointData));

        point.Magnet = magnet;
        point.AbsolutePointPos = pointPos;
        point.AbsoluteInCtrlPos = inCtrlPos;
        point.AbsoluteOutCtrlPos = outCtrlPos;

        point.PointPos = point.InitialPos = pointPos - currentPosition;
        point.InCtrlPos = point.InitialInCtrlPos = inCtrlPos - currentPosition;
        point.OutCtrlPos = point.InitialOutCtrlPos = outCtrlPos - currentPosition;

        return point;
    }
} 
