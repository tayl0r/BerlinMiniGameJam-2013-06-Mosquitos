using UnityEngine;

[AddComponentMenu("RageTools/Rage Handle")]
[ExecuteInEditMode]
public class RageHandle : MonoBehaviour {

	public string GizmoFile = "";
	public bool Live = true;

	public void OnDrawGizmos() {
		if(!Live) return;
		if (string.IsNullOrEmpty(GizmoFile)) return;
	   
		Gizmos.DrawIcon(transform.position, GizmoFile);
	}
}
