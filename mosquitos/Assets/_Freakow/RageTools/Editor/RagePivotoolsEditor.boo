import UnityEngine
import UnityEditor

[CustomEditor(typeof (RagePivotools))]
public class RagePivotoolsEditor (RageToolsEdit): 

	private _pivotools as RagePivotools

	protected override def OnDrawInspectorHeaderLine():
		_pivotools = target if _pivotools == null
		//return if _pivotools.FindRageGroup():
		EasyToggle "In Place", _pivotools.InPlace, MaxWidth(65f)
		if GUILayout.Button("Freeze Rotation & Scale", GUILayout.MinHeight(23f), GUILayout.MaxHeight(16f)):
			Undo.RegisterSceneUndo("RagePivotools: Freeze Rotation+Scale")
			_pivotools.FreezeRotationAndScale()
	
	protected override def OnDrawInspectorGUI():
		_pivotools = target if _pivotools == null
		EasyRow:
			//EasyCol:
			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(120f))
			GUILayout.Space(4f)
			LookLikeControls(38f, 20f)
			EasyPopup "Mode", _pivotools.CenteringType, MinWidth(120f), MaxWidth(120f)
			EditorGUILayout.EndVertical()
			EasyCol:
				if GUILayout.Button("Center", GUILayout.MaxHeight(20f)): 
					Undo.RegisterSceneUndo("RagePivotools: Center Selection Change")
					_pivotools.CenterPivot()				

		if _pivotools.CenteringType == RagePivotools.CenteringMode.Reference:
			EasyRow:
				GUILayout.Label("Reference:", GUILayout.MaxWidth(70f))
				EasyGameObjectField "", _pivotools.ReferenceGO
			if _pivotools.ReferenceGO==null:
				Warning("Please assign the reference GameObject.")

		if _pivotools.CenteringType == RagePivotools.CenteringMode.PerBranch:
			EasyRow:
				EasyToggle "Delete Pivot References", _pivotools.DeletePivotReferences		