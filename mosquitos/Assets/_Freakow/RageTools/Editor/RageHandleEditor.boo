[CustomEditor(typeof(RageHandle))]
public class RageHandleEditor (RageToolsEdit): 
	
	private _rageHandle as RageHandle	

	protected override def OnDrawInspectorHeaderLine():
		_rageHandle = target if _rageHandle == null
		
		LookLikeControls(20f, 1f)
		EasyToggle "Live", _rageHandle.Live, MaxWidth(45f)
		LookLikeControls(60f, 1f)
		GUILayout.Label("Gizmo File:", GUILayout.MaxWidth(65f))		
		_rageHandle.GizmoFile = GUILayout.TextField(_rageHandle.GizmoFile)

		EditorUtility.SetDirty (_rageHandle)
