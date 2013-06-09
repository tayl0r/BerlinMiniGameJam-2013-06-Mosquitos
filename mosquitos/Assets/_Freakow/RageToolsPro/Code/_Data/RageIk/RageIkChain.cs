using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RageIkChain
{
	public List<Transform> Joints;
	public Transform Target;
	public bool TwoDeeMode = true;
	private RageConstraint _constraint;
	public bool AlignEnd;
	public float Snap = 1f;
	
	[System.NonSerialized]
	public float[] SegmentLengths;
	
	[System.NonSerialized]
	public float Length;

	[System.NonSerialized]
	private bool _isInitialized = false;

	public RageIkChain() {
		Joints = new List<Transform>();
		Target = null;
		TwoDeeMode = true;
	}

	public RageIkChain(List<Transform> joints, Transform target, bool twoDeeMode) {
		Joints = joints;
		Target = target;
		TwoDeeMode = twoDeeMode;
		Snap = 1f;
	}

	public void Init(bool force = false) {
		if (_isInitialized) return;

		SegmentLengths = new float[Joints.Count];

		for (int i = 0; i < Joints.Count - 1; i++) {
			float dist = (Joints[i].position - Joints[i + 1].position).magnitude;

			SegmentLengths[i] = dist;
			Length += dist;
		}
		_isInitialized = true;
	}
	
	public void DebugDraw(Color c)
	{
		for (int i = 0; i < Joints.Count - 1; i++) {
			Debug.DrawLine(Joints[i].position, Joints[i+1].position, c);
		}
	}
}
