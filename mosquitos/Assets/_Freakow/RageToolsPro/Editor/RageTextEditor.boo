import UnityEngine

[CustomEditor(typeof (RageText))]
public class RageTextEditor (RageToolsEdit): 

	[SerializeField] private _currentEdgetune as bool
	
	private _rageText as RageText
	
	protected override def OnDrawInspectorHeaderLine():
		_rageText = target if _rageText == null	
		
		EasyCol:
			EasyRow:		
				EasyToggle "Live", _rageText.On, MaxWidth(60f)
				EasyToggle "Quick Copy", _rageText.QuickMode, MaxWidth(90f)
		
	protected override def OnDrawInspectorGUI():

		_rageText = target as RageText if _rageText == null
		_rageText.Text = " " if _rageText.Text == null
					
		EasyRow:
			LookLikeControls(35f, 90f)
			EasyObjectField "Font", _rageText.RageFont, typeof(GameObject)
			if (_rageText.RageFontWasChanged):
				_rageText.RageFontIsPrefab = ((PrefabUtility.GetPrefabParent(_rageText.RageFont) == null) and
					(not PrefabUtility.GetPrefabObject(_rageText.RageFont) == null))
				_rageText.RageFontWasChanged = false
			if ((not _rageText.RageFontIsPrefab) and (_rageText.QuickMode)):
				PrefabUtility.DisconnectPrefabInstance(_rageText.RageFont)

			LookLikeControls(45f, 1f)			
			EasyIntField "Buffer", _rageText.DisplayBufferSize, GUILayout.MaxWidth(70f)		
						
		EasyRow:
			LookLikeControls(35f)
			EasyTextField "Text", _rageText.Text
		
		EasyRow:
			LookLikeControls(70f, 40f)
			EasyPopup "Alignment", _rageText.Alignment		
			LookLikeControls(60f, 30f)
			EasyFloatField "Tracking", _rageText.Tracking

		EasyRow:
			LookLikeControls(70f, 40f)
			EasyObjectField "Container", _rageText.Container, typeof(RageCanvasAlign)

		if (_rageText.RageFont == null):
			EasyRow:
				Warning("* RageFont not assigned!")
		else:
			EasyRow:
				Warning("* One or more characters not found!") if (_rageText.CharNotFound and _rageText.On)

		EasyRow:
			Warning("* Quick Copy only works with Instantiated Fonts") if (_rageText.QuickMode and _rageText.RageFontIsPrefab)
			
		Repaint() if GUI.changed
		return if (_rageText.Container == null) 
		EditorUtility.SetDirty (_rageText.Container)
