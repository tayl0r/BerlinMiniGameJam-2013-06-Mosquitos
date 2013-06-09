import UnityEngine

[CustomEditor(typeof(RageIk))]
public class RageIkEditor (RageToolsEdit):
	
	private _rageIk as RageIk
	private _autoSetup as bool
	private _deleteButton as Texture2D = Resources.Load('deletebutton', Texture2D)
	private _newTransform as Transform

	public def OnDrawInspectorHeaderLine():
		_rageIk = target if _rageIk == null
		
		LookLikeControls(20f, 1f)
		EasyToggle "Live", _rageIk.On, MaxWidth(45f)
		EasyToggle "Always", _rageIk.AlwaysUpdate, MaxWidth(60f)
		EasyToggle "Align End", _rageIk.Chain.AlignEnd, MaxWidth(80f)

	public def OnDrawInspectorGUI():
		_rageIk = target if _rageIk == null
		
		LookLikeControls(110f, 50f)
		
		EasyRow:
			EasyObjectField	"Controller Handle", _rageIk.Chain.Target, typeof(Transform)

		EasyFoldout "Joints (" + _rageIk.Chain.Joints.Count + ")", _rageIk.ShowJoints:
			for i in range(0, _rageIk.Chain.Joints.Count):
				EasyRow:
					GUILayout.Label("", EditorStyles.label, MaxWidth(5f), MinWidth(5f))	
					LookLikeControls(1f, 10f)
					EasyObjectField	"", _rageIk.Chain.Joints[i], typeof(Transform)
					if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
						_rageIk.Chain.Joints.RemoveAt(i)
						break
			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(5f), MinWidth(5f))	
				LookLikeControls(20f, 1f)
				EasyObjectField	"+", _newTransform, typeof(Transform)
			if _newTransform != null:			
				if _rageIk.Chain.Joints.Contains(_newTransform):
					_newTransform = null
					Separator()
					return			
				_rageIk.AddTransformIfPossible(_newTransform)
				_newTransform = null
			GUILayout.Space (3);
		GUILayout.Space (3);
		EasyFoldout "Auto-Setup", _autoSetup:
			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(5f), MinWidth(5f))
				LookLikeControls(60f, 63f)
				EasyObjectField	"End Joint", _rageIk.AutoSetupEndJoint, typeof(GameObject)
				EasyToggle "End Offset", _rageIk.EndOffset
			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(5f), MinWidth(5f))
				LookLikeControls(40f, 1f)
				EasyToggle "Limiter", _rageIk.JointLimiterLive
				if _rageIk.JointLimiterLive:
					LookLikeControls(80f, 30f)
					EasyFloatField "Angle Delta", _rageIk.JointLimiterAngleDelta
			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(5f), MinWidth(5f))
				if (GUILayout.Button("Apply Auto-Setup")):
					if (_rageIk.AutoSetupEndJoint == null):
						Debug.Log("Error: End Joint not set")
					else:
						_rageIk.AutoSetup()

		GUILayout.Space (3);
		EasySettings:
			EasyRow:
				EasyToggle "Debug Line", _rageIk.DebugRay
				LookLikeControls(40f, 23f)
				EasyFloatField "Snap", _rageIk.Chain.Snap
