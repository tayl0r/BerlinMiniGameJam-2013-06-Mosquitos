import UnityEngine

[CustomEditor(typeof(RageMagnet))]
public class RageMagnetEditor (RageToolsEdit):

	//private static _tuning as bool
	private _isDirty as bool
	private _magnet as RageMagnet
	private _scheduleUpdateRestPosition as bool
	private _wasEnabled as bool
	private _lastKeyCode as KeyCode
	private _gizmos as bool

	public def OnDrawInspectorHeaderLine():
		_magnet = target if _magnet == null		

		if  _magnet.Group != null and _magnet.Group.MagnetData != null:
			LookLikeControls(20f, 1f)
			EasyToggle "Live", _magnet.Live, MaxWidth(45f)	
			
		LookLikeControls(75f, 50f)
		EasyObjectField "RageGroup", _magnet.Group, typeof(RageGroup)	
													
	public def OnDrawInspectorGUI():
		_magnet = target if _magnet == null
		
		if _magnet.Group == null:
			Warning ("Please set the RageGroup")
			return;
		
		EasyRow:		
			_magnet.UpdateGroupRestPosition() if GUILayout.Button("Set Rest Position", GUILayout.MinHeight(20f))
			
		return if _magnet.Group.MagnetData == null
		
		if not Application.isPlaying and Application.isEditor:		
			RageMagnetColliders.FixCollidersCheck(_magnet) 	
			if _magnet.ColliderType == RageMagnet.ColliderTypes.Capsule:	
				RageMagnetColliders.FixCollidersCheck(_magnet)			

		EasyRow:
			LookLikeControls(50f, 1f)
			EasyToggle "Show Weights", _magnet.ShowWeights, MaxWidth(110f)
			EasyToggle "Normalize", _magnet.Normalize, MaxWidth(110f)

		EasyFoldout "Local Settings", _settings:					
			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(10f), MinWidth(10f))
				LookLikeControls(90f,1f)
				EasyPopup "Collider Type", _magnet.ColliderType		

			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(10f), MinWidth(10f))
				LookLikeControls(50f,20f)
				EasyFloatField "Radius", _magnet.ColliderRadius
				if not _magnet.ActiveColliderIsSphere:	
					EasyFloatField "Height", _magnet.ColliderHeight

			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(10f), MinWidth(10f))
				LookLikeControls(50f,20f)			
				GUILayout.Label("Center", EditorStyles.label)
				LookLikeControls(20f,5f)
				EasyFloatField "X", _magnet.CenterX, MaxWidth(55f)
				EasyFloatField "Y", _magnet.CenterY, MaxWidth(55f)
				EasyFloatField "Z", _magnet.CenterZ, MaxWidth(55f)

			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(10f), MinWidth(10f))		
				LookLikeControls(110f,40f)
				EasyPercent "Inner Size (%)", _magnet.InnerSize, 100
					
			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(10f), MinWidth(10f))
				EasyToggle "Vary Strength", _magnet.VaryStrength, MaxWidth(100f)
				if _magnet.VaryStrength:	
					LookLikeControls(30f,1f)
					EasyFloatField "Min", _magnet.StrengthMin, MaxWidth(55f)
					EasyFloatField "Max", _magnet.StrengthMax, MaxWidth(55f)

		EasyFoldout "Gizmos", _gizmos:	
			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(10f), MinWidth(10f))		
				EasyToggle "Local Weights", _magnet.ShowLocalWeights, MaxWidth(100f)	
				EasyToggle "Show Gizmos", _magnet.ShowGizmos, MaxWidth(100f)

			EasyRow:
				GUILayout.Label("", EditorStyles.label, MaxWidth(10f), MinWidth(10f))
				LookLikeControls(130f)
				EasyFloatField "Gizmo Size", _magnet.GizmoSizeMultiplier


				
		AddMemberList()
		EditorUtility.SetDirty (_magnet)
							
	
	private  def AddMemberList():
		
		if _isDirty:
			_magnet.Group.UpdatePathList()
			_isDirty = false
		
	public def OnSceneGUI ():
		
		return if Event.current.type != EventType.KeyDown
	
		_magnet = target if _magnet == null			
		_lastKeyCode = Event.current.keyCode
		if _lastKeyCode == KeyCode.C: 
			_magnet.UpdateGroupRestPosition()
			EditorUtility.SetDirty(_magnet)
			return
			
		if _lastKeyCode == KeyCode.V: 		
			return if  _magnet.Group.MagnetData == null
			_magnet.Live = not _magnet.Live
			EditorUtility.SetDirty(_magnet)
