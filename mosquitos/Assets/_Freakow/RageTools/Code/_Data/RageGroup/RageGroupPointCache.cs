using UnityEngine;

[System.Serializable]
public class RageGroupPointCache {

	public Vector3 PointPos;
	public Vector3 InCtrlPos;
	public Vector3 OutCtrlPos;

	public RageGroupPointCache() { PointPos = InCtrlPos = OutCtrlPos = Vector3.zero; }
}