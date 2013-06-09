#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class MacrosRageConstraint : MonoBehaviour {
	/// <summary> Quick creation and setup of a new external controller, tied to the current selection
	/// </summary>
	[MenuItem("Component/RageTools/Macros/RageConstraint - Create Controller")]
	public static void RageConstraintQuickCreate() {
		#region Error Detection
		if (Selection.activeTransform == null) {
			Debug.Log("Macro Error: First select one or more game objects.");
			return;
		}
		#endregion Error Detection

		var controllersRoot = GameObject.Find ("_Controllers");
		if (controllersRoot == null || controllersRoot.transform.parent != null)
			controllersRoot = new GameObject("_Controllers");

		foreach (GameObject selectionItem in Selection.gameObjects) {
			var controller = new GameObject("Controller" + selectionItem.name);
			controller.transform.parent = selectionItem.transform;
			controller.transform.localPosition = Vector3.zero;
			controller.transform.localRotation = Quaternion.identity;
			controller.transform.localScale = selectionItem.transform.lossyScale;
			controller.transform.parent = controllersRoot.transform;
			var rageConstraint = controller.AddComponent<RageConstraint>();
			rageConstraint.Follower = selectionItem;
			rageConstraint.FollowPosition = true;
			Selection.activeGameObject = controller;
			var rageHandle = controller.AddComponent<RageHandle>();
			rageHandle.GizmoFile = "pole";
			rageHandle.Live = true;
		}
	}
}
#endif
