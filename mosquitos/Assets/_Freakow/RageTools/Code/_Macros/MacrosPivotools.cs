#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MacrosPivotools : EditorWindow {

	GameObject TargetObject, SourceObject;

	private GUIStyle _headerStyle, _toggleStyle;
	public bool AddGroups = true, TempGroups = true;
	public bool NameCleanup = true, PivotoolsBranch = true;

	/// <summary> Offsets the UVs of the Target RageGroup according to the reference's counterparts </summary>
	[MenuItem("Component/RageTools/Macros/RagePivotools - Clone UVs")]
	public static void Init() {
		var window = GetWindow(typeof(MacrosPivotools), true, "Pivotools Clone UVs");
		window.maxSize = new Vector2(245f, 115f);
		window.minSize = window.maxSize;
	}

	public void OnGUI() {
		// Instead of 'awake' we use Initialization checks
		InitStyles();
		EditorGUIUtility.LookLikeControls(60f);

		SourceObject = (GameObject) EditorGUILayout.ObjectField("Source", SourceObject, typeof(GameObject), true);
		TargetObject = (GameObject) EditorGUILayout.ObjectField("Target", TargetObject, typeof(GameObject), true);
		EditorGUILayout.BeginHorizontal();
			AddGroups = GUILayout.Toggle(AddGroups, "Add Groups", GUILayout.MaxWidth(120f));
			if (AddGroups) TempGroups = GUILayout.Toggle(TempGroups, "Temporary");
		EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal();
			NameCleanup = GUILayout.Toggle(NameCleanup, "Name Cleanup", GUILayout.MaxWidth(120f));
			PivotoolsBranch = GUILayout.Toggle(PivotoolsBranch, "Auto Pivots");
		EditorGUILayout.EndHorizontal();
		if (GUILayout.Button("Process")) Process();
	}

	/// <summary> Finds (or add if needed) the source and Target groups, fits UVs to both the copies from source to Target UVs
	/// </summary>
	public void Process( ) {
		if (SourceObject == null || TargetObject == null) {
			Debug.Log ("Macro error: Both Source and Target gO fields must be filled in. Aborting.");
			return;
		}
		RageGroup sourceGroup = SourceObject.GetComponent<RageGroup>();
		RageGroup targetGroup = TargetObject.GetComponent<RageGroup>();

		if (sourceGroup == null) sourceGroup = AddGroups ? SourceObject.AddComponent<RageGroup>() : null;
		if (targetGroup == null) targetGroup = AddGroups ? TargetObject.AddComponent<RageGroup>() : null;
		if (sourceGroup == null || targetGroup == null) {
			Debug.Log ("Macro error: One or both groups missing, aborting.");
			return;
		}
		sourceGroup.UvFit(); targetGroup.UvFit();
		if (NameCleanup) {
			sourceGroup.CleanupExtensions(); targetGroup.CleanupExtensions();
		}
		if (PivotoolsBranch) {
			ApplyPivotoolsBranch(sourceGroup); ApplyPivotoolsBranch (targetGroup);
		}
		sourceGroup.UpdatePathList(); targetGroup.UpdatePathList();
		OffsetUVs (sourceGroup, targetGroup);
		if (TempGroups) {
			SmartDestroy(sourceGroup);
			SmartDestroy(targetGroup);
		}
	}

	private void ApplyPivotoolsBranch (RageGroup group) {
		var pivotools = (RagePivotools) AddComponentCheck<RagePivotools> (group.gameObject);
		pivotools.CenteringType = RagePivotools.CenteringMode.PerBranch;
		pivotools.CenterPivot();
		SmartDestroy (pivotools);
	}

	public static Component AddComponentCheck<T> (GameObject GO) {
		Component component = GO.GetComponent (typeof (T));
		if (component != null)
			return component;
		return GO.AddComponent(typeof (T));
	}

	private void OffsetUVs (RageGroup sourceGroup, RageGroup targetGroup) {
		foreach (RageGroupElement item in targetGroup.List) {
			var matchingSpline = SearchCounterpart (item.Spline.Rs.gameObject.name, sourceGroup);
			if (matchingSpline == null) continue;
			var sourceOffset = matchingSpline.GetTextureOffset();

			//Debug.Log ("Target: "+item.Spline.Rs.name + " , Source: " + matchingSpline.name);
			item.Spline.Rs.SetTextureOffset (sourceOffset);
			item.Spline.Rs.RefreshMesh ();
		}
	}

	private RageSpline SearchCounterpart (string targetName, RageGroup sourceGroup) {
		foreach (RageGroupElement item in sourceGroup.List) {
			if (item.Spline.Rs.gameObject.name == targetName)
				return item.Spline.Rs;
		}
		return null;
	}

	private void InitStyles() {
		if (_headerStyle == null)
			_headerStyle = new GUIStyle(EditorStyles.label) { padding = { top = 5 } };
		if (_toggleStyle == null)
			_toggleStyle = new GUIStyle(EditorStyles.toggle) { padding = { top = 1 } };
	}

	public static void SmartDestroy(Object gameObj) {
		if (gameObj == null) return;
		if (Application.isEditor) {
			DestroyImmediate(gameObj);
			return;
		}
		Destroy(gameObj);
	}

}
#endif

