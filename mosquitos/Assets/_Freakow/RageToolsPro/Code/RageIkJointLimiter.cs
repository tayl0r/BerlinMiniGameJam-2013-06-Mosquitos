using UnityEngine;

public class RageIkJointLimiter : MonoBehaviour {

	public bool Live = true;
	public float MinLimiterAngle = 45f;
	public float MaxLimiterAngle = 315f;
	public static int GizmoRadius = 40;
	public static bool DrawGizmos = true;

	public float RestAngle;

	public Vector3 MaxAngle {
		get { return GetVectorLimiter(MinLimiterAngle); }
		set { MinLimiterAngle = RageIk.FullAngle(GetVectorLimiter(0), value); }
	}

	public Vector3 MinAngle {
		get { return GetVectorLimiter(MaxLimiterAngle); }
		set { MaxLimiterAngle = RageIk.FullAngle(GetVectorLimiter(0), value); }
	}

	private Vector3 GetVectorLimiter(float limiterAngle) {
		Quaternion parentRotation = transform.parent == null ? Quaternion.identity : transform.parent.rotation;
		return Quaternion.AngleAxis(limiterAngle, Vector3.forward) * parentRotation * Vector3.right;
	}

	public Vector3 RestDirection { 
		get { 
			return transform.rotation * Quaternion.AngleAxis(RestAngle, Vector3.forward) * Vector3.right; }
	}

	public float GetRestDirectionAngle(Vector3 restVector) {
		return FullAngle (Vector3.right, Quaternion.Inverse(transform.rotation) * restVector);
	}

	public bool ValidVector(Vector3 vector) {
		float angle = FullAngle(MaxAngle, vector);
		if (angle > AngleLimits()) return false;
		return true;
	}

	public float AngleLimits(){
		float angle = FullAngle(MaxAngle, MinAngle);
		if (Mathf.Approximately(angle , 0f)) 
			angle = 360;
		return angle;
	}

	private float FullAngle(Vector3 origin, Vector3 destiny){
		float angle = Vector3.Angle(origin, destiny);
		if (origin.y * destiny.x > origin.x * destiny.y)
			angle = 360 - angle;
		return angle;
	}
}
