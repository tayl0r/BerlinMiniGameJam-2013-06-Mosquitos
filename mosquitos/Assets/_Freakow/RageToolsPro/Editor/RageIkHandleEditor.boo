[CustomEditor(typeof(RageIkHandle))]
public class RageIkHandleEditor (RageToolsEdit): 
	
	private _rageIkHandle as RageIkHandle

	protected override def OnDrawInspectorHeaderLine():
		_rageIkHandle = target if _rageIkHandle == null
		
		LookLikeControls(20f, 1f)
		EasyToggle "IK", _rageIkHandle.On, MaxWidth(45f)
		LookLikeControls(60f, 1f)		
		EasyObjectField	"Rage Ik", _rageIkHandle.RageIk, typeof(RageIk)		

	protected override def OnDrawInspectorGUI():
		_rageIkHandle = target if _rageIkHandle == null
		return if _rageIkHandle.RageIk == null
		
		LookLikeControls(60f)
		EasyRow:		
			EasyToggle "Align End", _rageIkHandle.AlignEnd
			EasyToggle "Always", _rageIkHandle.RageIk.AlwaysUpdate;
			EasyToggle "Show Gizmo", _rageIkHandle.ShowGizmo

		EditorUtility.SetDirty (_rageIkHandle.RageIk)
