import UnityEngine
import UnityEditor

[CustomEditor(typeof(RageLayer))]
public class RageLayerEditor (RageToolsEdit): 

	private _rageLayer as RageLayer

	public def OnDrawInspectorHeaderLine():
		_rageLayer = target if _rageLayer == null

		LookLikeControls(60f, 20f)
		EasyIntField "Z Order", _rageLayer.Zorder, MaxWidth(90f)
		if GUILayout.Button("Refresh", GUILayout.MinHeight(20f), MaxWidth(90f)):
			RegisterUndo("RageLayer: Refresh")
			_rageLayer.SetMaterialRenderQueue()
		if (Event.current.type == EventType.ValidateCommand and Event.current.commandName == "UndoRedoPerformed"):
		  Repaint()

	protected override def OnGuiRendered():
		SetDirty(target) if GUI.changed
