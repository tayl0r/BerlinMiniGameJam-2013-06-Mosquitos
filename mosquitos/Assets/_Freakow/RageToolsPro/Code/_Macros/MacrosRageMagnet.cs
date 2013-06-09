#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary> This Macro takes the "Live" state of the currently selected RageMagnet and sets its opposite to all Magnets in the same hierarchy.
/// </summary>
public class MacrosRageMagnet : MonoBehaviour {
	[MenuItem("Component/RageTools/Macros/RageMagnet - Hierarchy Live Toggle")]
	public static void RageMagnetHierarchyLive() {
		if (!ValidSelection()) return;
		var selectedLiveState = Selection.activeGameObject.GetComponent<RageMagnet>().On;
		var magnetGlobalCollection = Selection.activeTransform.root.GetComponentsInChildren<RageMagnet>();

		foreach (var magnet in magnetGlobalCollection)
			magnet.Live = !selectedLiveState;
	}

	[MenuItem("Component/RageTools/Macros/RageMagnet - Hierarchy Set Rest Position")]
	public static void RageMagnetHierarchySetRest() {
		if (Selection.activeTransform == null) {
			Debug.Log ("Macro Error: First select any game object");
			return;
		}

		var magnetCollection = Selection.activeTransform.root.GetComponentsInChildren<RageMagnet>();

		foreach (RageMagnet magnet in magnetCollection)
			magnet.UpdateRestPosition();     
	}


	private static bool ValidSelection() {
		if (Selection.activeTransform == null || Selection.activeGameObject.GetComponent<RageMagnet>()==null) {
			Debug.Log("Macro Error: First select a Magnet.");
			return false;
		}
		return true;
	}
}
#endif
