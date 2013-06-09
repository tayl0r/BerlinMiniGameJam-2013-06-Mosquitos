import UnityEngine
//import Com.Freakow.BooInspector

[CustomEditor(typeof(RageEdgetune))]
public class RageEdgetuneEditor (RageToolsEdit): 

	private _edgetune as RageEdgetune
	
	public def OnDrawInspectorHeaderLine():
		_edgetune = target if _edgetune == null	
		
		EasyToggle "Live", _edgetune.On, MaxWidth(60f)
		EasyToggle "Start Only", _edgetune.StartOnly
				
	public def OnDrawInspectorGUI():
		_edgetune = target if _edgetune == null
		_edgetune.RefreshCheck()
		
		EasyRow:
			if _edgetune.Data == null:
				_edgetune.Data = ScriptableObject.CreateInstance(typeof(RageEdgetuneData))
			
			Undo.SetSnapshotTarget(target, "RageEdgetune Initialize")
						
			if GUILayout.Button("Initialize"):
				CreateAndRegisterUndoSnapshot()
				_edgetune.ScheduleInitialize()
			
		EasyRow:
			EasyToggle "Auto Density", _edgetune.Data.AutomaticLod, MinWidth(100f), MaxWidth(100f)
			if _edgetune.Data.AutomaticLod:			
				LookLikeControls(30, 10)		
				EasyIntField "Max", _edgetune.Data.MaxDensity, MaxWidth(56)
				LookLikeControls(120f)
				if GUILayout.Button("Guess"):
					CreateAndRegisterUndoSnapshot()
					_edgetune.GuessMaxDensity()
		
		EasySettings:
			EasyRow:
				LookLikeControls(73f)
				EasyObjectField "RageGroup", _edgetune.Group, typeof(RageGroup)
					
			EasyRow:
				LookLikeControls(65f, 1f)		
				EasyFloatField "AA Factor", _edgetune.Data.AaFactor, MaxWidth(105f)						
				LookLikeControls(90f, 1f)
				EasyFloatField "Density Factor", _edgetune.Data.DensityFactor

			EasyRow:
				LookLikeControls(105f, 1f)
				EasyFloatField "Perspective Blur", _edgetune.Data.PerspectiveBlur

			EasyRow:
				LookLikeControls(100f, 60f)
				EasyObjectField "Debug TextMesh", _edgetune.DebugTextMesh, typeof(TextMesh)
				EasyToggle "Density", _edgetune.DebugDensity, MaxWidth(60f)

		EditorUtility.SetDirty(_edgetune);
		EditorUtility.SetDirty(_edgetune.Group);
