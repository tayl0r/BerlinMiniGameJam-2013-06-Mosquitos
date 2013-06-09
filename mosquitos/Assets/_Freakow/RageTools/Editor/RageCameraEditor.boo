import UnityEngine
import UnityEditor

[CustomEditor(typeof(RageCamera))]
public class RageCameraEditor (RageToolsEdit): 

	private _rageCamera as RageCamera

	public def OnDrawInspectorHeaderLine():
		_rageCamera = target if _rageCamera == null
		if GUILayout.Button("Initialize", GUILayout.MinHeight(20f)):
			RegisterUndo("RageCamera: Initialize")
			_rageCamera.Initialize()
			
	public def OnDrawInspectorGUI():
		_rageCamera = target if _rageCamera == null
		LookLikeControls(120f)
		EasyIntField "Default Ortho Size", _rageCamera.DefaultOrthoSize
		EasyIntField "Default Res Height", _rageCamera.DefaultResolutionHeight
		EasyFloatField "Update Threshold", _rageCamera.UpdateThreshold
		EasyToggle "Editor Mode Update", _rageCamera.EditorRealtimeUpdate

	protected override def OnGuiRendered():
		_rageCamera = target if _rageCamera == null
		SetDirty(target) if GUI.changed
