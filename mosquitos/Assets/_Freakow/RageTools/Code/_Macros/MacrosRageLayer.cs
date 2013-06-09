#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MacrosRageLayer : EditorWindow {

	private int _offset = 100;

	[MenuItem("Component/RageTools/Macros/RageLayer - Group Offset")]
	public static void Init() {
		var window = GetWindow(typeof(MacrosRageLayer), true, "RageLayer Group Offset");
		window.maxSize = new Vector2(245f, 55f);
		window.minSize = window.maxSize;
	}

	public void OnGUI() {
		EditorGUIUtility.LookLikeControls(60f);
		_offset = EditorGUILayout.IntField("Offset", _offset);
		if (GUILayout.Button("Process"))
			if (ValidSelection(true)) {
				var group = Selection.activeTransform.GetComponent<RageGroup>();
				RageLayerGroupOffset (group, _offset);
			}
	}

	public static void RageLayerGroupOffset (RageGroup group, int offset) {
		foreach (var item in group.List) {
			var layer = item.Spline.GameObject.GetComponent<RageLayer>();
			if (layer != null)
				layer.OffsetMaterialRenderQueue (offset);
		}
		#if UNITY_EDITOR
		if (!EditorApplication.isPlaying)
			Debug.LogWarning("* Multiple Materials Created. Ignore above error messages.");
		#endif
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
