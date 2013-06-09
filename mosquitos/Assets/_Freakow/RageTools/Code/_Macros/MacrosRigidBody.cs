#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary> This Macro prepares an imported SVG file, in the RageFont format, for use with RageText.
/// It must be fired with the root of the RageFont selected. 
/// It expect RageChars as children and RageChar elements as grandchildren of the RageFont root.
/// </summary>

public class MacrosRigidBody : EditorWindow {
	bool FilterCollider;
	string FilterName="";
	float Mass = 1f;
	float Drag;
	float AngularDrag = 0.05f;
	bool UseGravity = true;
	bool IsKinematic;
	RigidbodyInterpolation Interpolate = RigidbodyInterpolation.None;
	CollisionDetectionMode CollisionDetectionMode;

	bool Constraints = true;
	bool FreezePosX; bool FreezePosY; bool FreezePosZ;
	bool FreezeRotX; bool FreezeRotY; bool FreezeRotZ;

	private GUIStyle _headerStyle, _toggleStyle;

	[MenuItem("Component/RageTools/Macros/RigidBody - Multi-Setup")]
	public static void Init() {
		var window = GetWindow(typeof(MacrosRigidBody),true,"RigidBody Multi-Setup");
		window.maxSize = new Vector2 (255f, 285f);
		window.minSize = window.maxSize;
	}

	public void OnGUI() {
		// Instead of 'awake' we use Initialization checks
		InitStyles();

		GUILayout.Label("* Select the Hierarchy Root", EditorStyles.whiteMiniLabel);

		FilterFields();
		ProcessButton();
		GUILayout.Space(5);

		EditorGUIUtility.LookLikeControls(0f, 0f);
		Mass = EditorGUILayout.FloatField("Mass", Mass);
		Drag = EditorGUILayout.FloatField("Drag", Drag);
		AngularDrag = EditorGUILayout.FloatField("Angular Drag", AngularDrag);
		UseGravity = GUILayout.Toggle(UseGravity, "Use Gravity");
		IsKinematic = GUILayout.Toggle(IsKinematic, "Is Kinematic");
		Interpolate = (RigidbodyInterpolation) EditorGUILayout.EnumPopup("Interpolate", Interpolate);
		CollisionDetectionMode = (CollisionDetectionMode) EditorGUILayout.EnumPopup("Collision Detection", CollisionDetectionMode);

		Constraints = EditorGUILayout.Foldout(Constraints, "Constraints");
		if (Constraints) 
			ConstraintFields();
	}

	private void FilterFields( ) {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label ("Filters: ", GUILayout.Width (50f));
		FilterCollider = GUILayout.Toggle (FilterCollider, "Collider", _toggleStyle, GUILayout.Width (70f));
		EditorGUIUtility.LookLikeControls (45f, 10f);
		FilterName = EditorGUILayout.TextField ("Name", FilterName);
		EditorGUILayout.EndHorizontal();
	}

	private void ProcessButton() {
		if (GUILayout.Button("Process")) {
			var children = Selection.activeTransform.GetComponentsInChildren<Transform>();
			foreach (Transform child in children) {
				if (FilterFailed(child.gameObject))
					continue;
				var rb = child.GetComponent<Rigidbody>() ?? child.gameObject.AddComponent<Rigidbody>();
				AssignRigidbodyValues(rb);
			}
			GetWindow(typeof(MacrosRigidBody)).Close();
		}
	}

	private bool FilterFailed(GameObject go) {
		if (FilterName != "" && !go.name.Contains(FilterName))
			return true;
		if (FilterCollider && (go.GetComponent(typeof(Collider)) == null))
			return true;
		return false;
	}

	private void AssignRigidbodyValues (Rigidbody rb) {
		RigidbodyConstraints rbConstraints =
								(FreezePosX ? RigidbodyConstraints.FreezePositionX : 0) |
								(FreezePosY ? RigidbodyConstraints.FreezePositionY : 0) |
								(FreezePosZ ? RigidbodyConstraints.FreezePositionZ : 0) |
								(FreezeRotX ? RigidbodyConstraints.FreezeRotationX : 0) |
								(FreezeRotY ? RigidbodyConstraints.FreezeRotationY : 0) |
								(FreezeRotZ ? RigidbodyConstraints.FreezeRotationZ : 0);
		rb.mass = Mass;
		rb.drag = Drag;
		rb.angularDrag = AngularDrag;
		rb.useGravity = UseGravity;
		rb.isKinematic = IsKinematic;
		rb.interpolation = Interpolate;
		rb.constraints = rbConstraints;
		rb.collisionDetectionMode = CollisionDetectionMode;
	}

	private void ConstraintFields( ) {
		EditorGUILayout.BeginHorizontal(); {

			GUILayout.Label ("Freeze Position", _headerStyle, GUILayout.Width (98f));
			FreezePosX = EditorGUILayout.BeginToggleGroup ("X", FreezePosX);
			EditorGUILayout.EndToggleGroup();
			FreezePosY = EditorGUILayout.BeginToggleGroup ("Y", FreezePosY);
			EditorGUILayout.EndToggleGroup();
			FreezePosZ = EditorGUILayout.BeginToggleGroup ("Z", FreezePosZ);
			EditorGUILayout.EndToggleGroup();

		} EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(); {

			GUILayout.Label ("Freeze Rotation", _headerStyle, GUILayout.Width (98f));
			FreezeRotX = EditorGUILayout.BeginToggleGroup ("X", FreezeRotX);
			EditorGUILayout.EndToggleGroup();
			FreezeRotY = EditorGUILayout.BeginToggleGroup ("Y", FreezeRotY);
			EditorGUILayout.EndToggleGroup();
			FreezeRotZ = EditorGUILayout.BeginToggleGroup ("Z", FreezeRotZ);
			EditorGUILayout.EndToggleGroup();

		} EditorGUILayout.EndHorizontal();
	}

	private void InitStyles() {
		if (_headerStyle == null)
			_headerStyle = new GUIStyle (EditorStyles.label) {padding = {top = 5}};
		if (_toggleStyle == null)
			_toggleStyle = new GUIStyle(EditorStyles.toggle) { padding = { top = 1 }};
	}

}
#endif
