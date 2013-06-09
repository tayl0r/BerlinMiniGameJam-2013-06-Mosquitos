using UnityEngine;

public class RageSvgPathElement : ScriptableObject {
	public ISpline Spline;
	public GameObject gO;
	public bool IsClosed;
	public bool IsLinear;

	private RageSvgPathElement() {
		Spline = null;
		gO = null;
		IsClosed = false;
		IsLinear = false;
	}

	public static RageSvgPathElement NewInstance() {
		return CreateInstance(typeof(RageSvgPathElement)) as RageSvgPathElement;
	}
}
