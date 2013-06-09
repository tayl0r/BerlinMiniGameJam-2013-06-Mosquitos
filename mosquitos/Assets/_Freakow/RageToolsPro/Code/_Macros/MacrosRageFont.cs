#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary> This Macro prepares an imported SVG file, in the RageFont format, for use with RageText.
/// It must be fired with the root of the RageFont selected. 
/// It expect RageChars as children and RageChar elements as grandchildren of the RageFont root.
/// </summary>

public class MacrosRageFont : EditorWindow {
	bool processCharacters = true;
	bool processKerning = true;
	float Kerning = 10f;
	float ColliderZDepth = 10f;

	[MenuItem("Component/RageTools/Macros/RageFont - Setup")]
	public static void Init() {
		var window = GetWindow(typeof(MacrosRageFont),true,"RageFont Setup");
		window.maxSize = new Vector2 (250f, 137f);
		window.minSize = window.maxSize;
	}

	public void OnGUI() {
		GUILayout.Label("* Keep the RageFont root selected", EditorStyles.whiteMiniLabel);
		//GUILayout.Box("aaAA", EditorStyles.whiteLabel);

		processCharacters = EditorGUILayout.BeginToggleGroup("Format Chars", processCharacters);
		EditorGUILayout.EndToggleGroup();
		processKerning = EditorGUILayout.BeginToggleGroup("Auto-Kerning", processKerning);
			Kerning = EditorGUILayout.Slider("Kerning", Kerning, -10, 100);
			ColliderZDepth = EditorGUILayout.FloatField("Collider Depth", ColliderZDepth);
		EditorGUILayout.EndToggleGroup();
		if (GUILayout.Button("Process")) {
			if (processCharacters) RageCharactersSetup();
			if (processKerning) RageFontAutoKerning(Kerning, ColliderZDepth);
			GetWindow(typeof(MacrosRageFont)).Close();
		}
	}

	public static void RageCharactersSetup() {
		#region Error Detection
		if (Selection.activeTransform == null) {
			Debug.Log("Macro Error: First select the Root Game gO of an imported Font.");
			return;
		}

		if (Selection.activeTransform.childCount == 0) {
			Debug.Log("Macro Error: Game gO has no children. Please select the RageFont root.");
			return;
		}
		#endregion Error Detection

		var keycodes = new Dictionary<string, string>();
		keycodes = InitializeKeycodes(keycodes);

		foreach (Transform rageChar in Selection.activeTransform) {
			// Activates the RageChar and contained elements temporarily
			#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
			rageChar.gameObject.SetActiveRecursively(true);
			#else
			rageChar.gameObject.SetActive(true);
			#endif

			var thisRageChar = rageChar.gameObject;
			if (thisRageChar.name.EndsWith("_1_"))
				thisRageChar.name = thisRageChar.name.Replace("_1_", "");

			var rageCharKeyName = thisRageChar.name;
			// Disable the fill of the space character
			rageCharKeyName = rageCharKeyName.Substring(0,(int)(Mathf.Min(thisRageChar.name.Length, 5f)));
			// When a keycode is found, replace it with the related string;
			if (rageCharKeyName=="space") {
				var spline = thisRageChar.GetComponentInChildren<RageSpline>();
				if (spline == null)
					Debug.Log("'space' char RageSpline not found. Game gO disabled?");
				else
					spline.gameObject.GetComponent<MeshRenderer>().enabled = false;
			}
			if (keycodes.ContainsKey(rageCharKeyName))
				thisRageChar.name = keycodes[rageCharKeyName];

			// Cycle through RageChar elements and number them sequentially
			var counter = 1;
			foreach (Transform rageCharElement in rageChar) {
				var thisRageCharElement = rageCharElement.gameObject;
				thisRageCharElement.name = counter.ToString();
				counter++;
			}
		}
		// Finally, de-activates the RageFont game gO, chars and elements
		#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
		Selection.activeGameObject.SetActiveRecursively(false);
		#else
		Selection.activeGameObject.SetActive(false);
		#endif
	}

	public static void RageFontAutoKerning(float kerning, float colliderZDepth) {
		#region Error Detection
		if (Selection.activeTransform == null) {
			Debug.Log("Macro Error: First select the Root Game gO of an imported Font.");
			return;
		}

		if (Selection.activeTransform.childCount == 0) {
			Debug.Log("Macro Error: Game gO has no children. Please select the RageFont root.");
			return;
		}
		#endregion Error Detection

		foreach (Transform rageChar in Selection.activeTransform) {
			var thisRageChar = rageChar.gameObject;
			// If it already has a collider attached, destroy it
			var thisCollider = thisRageChar.GetComponent<BoxCollider>();
			if (thisCollider != null) DestroyImmediate(thisCollider);

			var totalBoundaryMin = Vector3.zero;
			var totalBoundaryMax = Vector3.zero;
			float zPosition = 0f;
			var firstElement = true;
			foreach (Transform rageCharElement in rageChar) {
				var thisMeshFilter = rageCharElement.GetComponent<MeshFilter>();
				if (!thisMeshFilter) {
					Debug.Log("Macro Error: RageSpline or Mesh Filter not found for character "+rageChar.name);
					continue;
				}
				var thisBoundary = thisMeshFilter.sharedMesh.bounds;
				// Flag Used to get a valid starting reference
				if (firstElement) {
					totalBoundaryMin = new Vector3(thisBoundary.min.x, thisBoundary.min.y, 0);
					var spline = rageCharElement.GetComponent<RageSpline>();
					// Boundaries won't help with the z position due to the way RageSpline build the meshes. 
					// Thus we get the z coordinate of the first spline point, since all are colinear anyways.
					if (spline) zPosition = spline.GetPositionWorldSpace(0).z;
					totalBoundaryMax = new Vector3(thisBoundary.max.x, thisBoundary.max.y, 0);
					firstElement = false;
					continue;
				}
				if (thisBoundary.min.x < totalBoundaryMin.x)
					totalBoundaryMin = SetVector3X(totalBoundaryMin, thisBoundary.min.x);
				if (thisBoundary.min.y < totalBoundaryMin.y)
					totalBoundaryMin = SetVector3Y(totalBoundaryMin, thisBoundary.min.y);
				if (thisBoundary.max.x > totalBoundaryMax.x)
					totalBoundaryMax = SetVector3X(totalBoundaryMax, thisBoundary.max.x);
				if (thisBoundary.max.y > totalBoundaryMax.y)
					totalBoundaryMax = SetVector3Y(totalBoundaryMax, thisBoundary.max.y);
			}
			var newCollider = thisRageChar.AddComponent<BoxCollider>();
			newCollider.center = new Vector3(	(totalBoundaryMin.x + totalBoundaryMax.x) / 2, 
												(totalBoundaryMin.y + totalBoundaryMax.y) / 2,
												zPosition );
			newCollider.size = new Vector3(	(totalBoundaryMax.x - totalBoundaryMin.x) + kerning,
											(totalBoundaryMax.y - totalBoundaryMin.y),
											colliderZDepth );
		}
	}

	private static Vector3 SetVector3X(Vector3 vector, float xValue) {
		return new Vector3(xValue,vector.y,vector.z);
	}

	private static Vector3 SetVector3Y(Vector3 vector, float yValue) {
		return new Vector3(vector.x, yValue, vector.z);
	}

	/// <summary> Sets the proper key codes values, used to replace SVG-export symbol group names to their proper symbols
	/// </summary>
	private static Dictionary<string, string> InitializeKeycodes(Dictionary<string, string> keycodes) {
		keycodes = new Dictionary<string, string> {  {"_x27_", "`"}
													,{"_x2A_", "*"}
													,{"_x2B_", "+"}
													,{"_x2C_", ","}
													,{"_x2D_", "-"}
													,{"_x2E_", "/."}
													,{"_x2F_", "//"}
													,{"_x3B_", ";"}
													,{"_x3C_", "<"}
													,{"_x3D_", "="}
													,{"_x3E_", ">"}
													,{"_x3F_", "?"}
													,{"_x5B_", "["}
													,{"_x5C_", "\\"}
													,{"_x5D_", "]"}
													,{"_x5F_", "_"}
													,{"_x21_", "!"}
													,{"_x22_", "\""}
													,{"_x23_", "#"}
													,{"_x24_", "$"}
													,{"_x25_", "%"}
													,{"_x26_", "&"}
													,{"_x28_", "("}
													,{"_x29_", ")"}
													,{"_x30_", "0"}			
													,{"_x31_", "1"}
													,{"_x32_", "2"}
													,{"_x33_", "3"}
													,{"_x34_", "4"}
													,{"_x35_", "5"}
													,{"_x36_", "6"}			
													,{"_x37_", "7"}
													,{"_x38_", "8"}
													,{"_x39_", "9"}
													,{"_x40_", "@"}
													,{"space", " "}
		};
		return keycodes;
	}
}
#endif
