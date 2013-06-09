#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MacrosRageGroup : EditorWindow {
	private Material _material;

	[MenuItem("Component/RageTools/Macros/RageGroup - Hierarchy Group Update")]
	public static void RageGroupHierarchyUpdate() {
		if (!ValidSelection(false)) return;
		var groupGlobalCollection = Selection.activeTransform.root.GetComponentsInChildren<RageGroup>();

		foreach (var group in groupGlobalCollection) 
			group.UpdatePathList();
	}

	[MenuItem("Component/RageTools/Macros/RageGroup - Apply Material")]
	public static void Init() {
		var window = GetWindow(typeof(MacrosRageGroup), true, "RageGroup Apply Material");
		window.maxSize = new Vector2(245f, 55f);
		window.minSize = window.maxSize;
	}

	public void OnGUI() {
		//InitStyles();
		EditorGUIUtility.LookLikeControls(60f);
		_material = (Material) EditorGUILayout.ObjectField("Material", _material, typeof(Material), true);
		if (GUILayout.Button("Process"))
			if (ValidSelection(true)) {
				var group = Selection.activeTransform.GetComponent<RageGroup>();
				RageGroupApplyMaterial (group, _material);
			}
	}

	public static void RageGroupApplyMaterial(RageGroup group, Material material) {
		foreach (var item in group.List)
			item.Spline.GameObject.GetComponent<MeshRenderer>().material = material;
	}

	private static bool ValidSelection(bool groupCheck) {
		if (Selection.activeTransform == null) {
			Debug.Log("Macro Error: First select a Game gO in the desired hierarchy.");
			return false;
		}
		if (groupCheck && Selection.activeTransform.GetComponent<RageGroup>() == null) {
			Debug.Log("Macro Error: No RageGroup in the selected Game gO.");
			return false;
		}
		return true;
	}
}
#endif
