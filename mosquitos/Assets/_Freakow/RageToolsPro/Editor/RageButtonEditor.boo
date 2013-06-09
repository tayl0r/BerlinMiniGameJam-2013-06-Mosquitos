import UnityEngine
import UnityEditor

[CustomEditor(typeof(RageButton))]
public class RageButtonEditor (RageToolsEdit): 

	private static _showClickActions as bool
	private static _showAnimations as bool = true
	private static _showSounds as bool

	private _rageButton as RageButton
	private _deleteButton as Texture2D = Resources.Load('deletebutton', Texture2D)
	private _playReverseOn as Texture2D = Resources.Load('playreverseon', Texture2D)
	private _playReverseOff as Texture2D = Resources.Load('playreverseoff', Texture2D)
	private _resetOn as Texture2D = Resources.Load('reseton', Texture2D)
	private _resetOff as Texture2D = Resources.Load('resetoff', Texture2D)
	[SerializeField]private _playReverseClick as Texture2D = _playReverseOff
	[SerializeField]private _playReverseHoverIn as Texture2D = _playReverseOff
	[SerializeField]private _playReverseHoverOut as Texture2D = _playReverseOff
	[SerializeField]private _resetClick as Texture2D = _resetOff
	[SerializeField]private _resetHoverIn as Texture2D = _resetOff
	[SerializeField]private _resetHoverOut as Texture2D = _resetOff
	[SerializeField]private _data as RageButtonData
	[SerializeField]private _actionType as RageButtonData.ActionTypes

	//private _newToEnable as GameObject
	private _newTarget as GameObject
	private _newToDisable as GameObject
	private _newToInstantiate as GameObject
	private _newToDelete as GameObject
	private _newToExecute as MonoBehaviour

	public def OnDrawInspectorHeaderLine():
		_rageButton = target if _rageButton == null
		EasyToggle "Advanced", _rageButton.AdvancedOptions
			
	public def OnDrawInspectorGUI():
		_rageButton = target if _rageButton == null
		_data = _rageButton.Data 
		return if _data == null
		LookLikeControls(80f)
		EasyFoldout "Click Actions", _showClickActions:
			if _rageButton.AdvancedOptions:
				//EasyList "To Enable", _data.ToEnableOnClick, GameObject, _newToEnable, _deleteButton
				ToEnableList()
				ToDisableList()
				ToInstantiateList()
				ToDeleteList()
				AddClickActionLine()
			EasyList "To Execute", _data.ToExecuteOnClick, MonoBehaviour, _newToExecute, _deleteButton

		EasyFoldout "Tween Animations", _showAnimations:
			LookLikeControls(72f)
			EasyRow 5f:
				EasyTextField "Click", _data.Click.TweenId
				EasyToggleButton _playReverseClick, _playReverseOn, _playReverseOff, _data.Click.PlayReverse, "Play Reverse"
				EasyToggleButton _resetClick, _resetOn, _resetOff, _data.Click.Reset, "Reset before Play"
			if _rageButton.AdvancedOptions:
				EasyRow 20f:
					EasyObjectField	"Target", _data.Click.Target, typeof(GameObject)
			EasyRow 5f:
				EasyTextField "Hover In", _data.HoverIn.TweenId
				EasyToggleButton _playReverseHoverIn, _playReverseOn, _playReverseOff, _data.HoverIn.PlayReverse, "Play Reverse"
				EasyToggleButton _resetHoverIn, _resetOn, _resetOff, _data.HoverIn.Reset, "Reset before Play"
			if _rageButton.AdvancedOptions:
				EasyRow 20f:
					EasyObjectField	"Target", _data.HoverIn.Target, typeof(GameObject)
			EasyRow 5f:
				EasyTextField "Hover Out", _data.HoverOut.TweenId
				EasyToggleButton _playReverseHoverOut, _playReverseOn, _playReverseOff, _data.HoverOut.PlayReverse, "Play Reverse"
				EasyToggleButton _resetHoverOut, _resetOn, _resetOff, _data.HoverOut.Reset, "Reset before Play"
			if _rageButton.AdvancedOptions:
				EasyRow 20f:
					EasyObjectField	"Target", _data.HoverOut.Target, typeof(GameObject)

		EasyFoldout "Sounds", _showSounds:
			LookLikeControls(72f)
			EasyRow 5f:
				EasyObjectField	"Click", _data.Click.Sound, typeof(AudioSource)
			EasyRow 5f:
				EasyObjectField	"Hover In", _data.HoverIn.Sound, typeof(AudioSource)
			EasyRow 5f:
				EasyObjectField	"Hover Out", _data.HoverOut.Sound, typeof(AudioSource)

	private def ToEnableList():
		_data = _rageButton.Data if _data == null
		return if _data.ToEnableOnClick.Count == 0
		LookLikeControls(110f, 50f)
		EasyRow 10f:
			EasyCol:
				LookLikeControls(110f, 100f)
				for i in range(0, _data.ToEnableOnClick.Count):		
					EasyRow:
						LookLikeControls(82f, 1f)
						EasyGameObjectField	"Enable", _data.ToEnableOnClick[i], GUILayout.MaxWidth(220)
						if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
							_data.ToEnableOnClick.RemoveAt(i)
							break

	private def ToDisableList():
		_data = _rageButton.Data if _data == null
		return if _data.ToDisableOnClick.Count == 0
		LookLikeControls(110f, 50f)
		EasyRow 10f:
			EasyCol:
				LookLikeControls(110f, 100f)
				for i in range(0, _data.ToDisableOnClick.Count):		
					EasyRow:
						LookLikeControls(82f, 1f)
						EasyGameObjectField	"Disable", _data.ToDisableOnClick[i], GUILayout.MaxWidth(220)
						if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
							_data.ToDisableOnClick.RemoveAt(i)
							break

	private def ToInstantiateList():
		_data = _rageButton.Data if _data == null
		return if _data.ToInstantiateOnClick.Count == 0
		LookLikeControls(110f, 50f)
		EasyRow 10f:
			EasyCol:
				LookLikeControls(110f, 100f)
				for i in range(0, _data.ToInstantiateOnClick.Count):		
					EasyRow:
						LookLikeControls(82f, 1f)
						EasyGameObjectField	"Instantiate", _data.ToInstantiateOnClick[i], GUILayout.MaxWidth(220)
						if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
							_data.ToInstantiateOnClick.RemoveAt(i)
							break

	private def ToDeleteList():
		_data = _rageButton.Data if _data == null
		return if _data.ToDeleteOnClick.Count == 0
		LookLikeControls(110f, 50f)
		EasyRow 10f:
			EasyCol:
				LookLikeControls(110f, 100f)
				for i in range(0, _data.ToDeleteOnClick.Count):		
					EasyRow:
						LookLikeControls(82f, 1f)
						EasyGameObjectField	"Delete", _data.ToDeleteOnClick[i], GUILayout.MaxWidth(220)
						if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
							_data.ToDeleteOnClick.RemoveAt(i)
							break

	private def AddClickActionLine():
		EasyRow:
			GUILayout.Label ("+", GUILayout.MaxWidth(13f))
			LookLikeControls(15f, 10f)
			EasyPopup "", _actionType, MaxWidth(75f)
			LookLikeControls(15f, 42f)
			EasyGameObjectField	"", _newTarget

		if _newTarget != null:	
			//selectedList as typeof(_data.ToEnableOnClick)
			//selectedList = _data.ToEnableOnClick
			selectedList = ActionTypeList()
			if selectedList.Contains(_newTarget):
				_newTarget = null
				Separator()
				return			
			selectedList.Add(_newTarget);
			_newTarget = null
			Separator()

	private def ActionTypeList():
		return _data.ToEnableOnClick if _actionType == RageButtonData.ActionTypes.Enable
		return _data.ToDisableOnClick if _actionType == RageButtonData.ActionTypes.Disable
		return _data.ToInstantiateOnClick if _actionType == RageButtonData.ActionTypes.Instantiate
		return _data.ToDeleteOnClick if _actionType == RageButtonData.ActionTypes.Delete

//	private def ShowClickActions():


//		LookLikeControls(110f, 50f)
//		EasyRow:
//			EasySpacer 5f
//			GUILayout.Label ("To Enable (" + _rageButton.Data.ToEnableOnClick.Count + "):")
//		EasyRow:
//			EasySpacer 10f
//			EasyCol:
//				LookLikeControls(110f, 100f)
//				for i in range(0, _rageButton.Data.ToEnableOnClick.Count):		
//					EasyRow:
//						EasyGameObjectField	"", _rageButton.Data.ToEnableOnClick[i]
//						if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
//							_rageButton.Data.ToEnableOnClick.RemoveAt(i)
//							break
//				EasyRow:
//					LookLikeControls(15f, 10f)
//					EasyGameObjectField	"+", _newToEnable
//				
//		if _newToEnable != null:			
//			if _newToEnable == _rageButton.Data.gO or _rageButton.Data.ToEnableOnClick.Contains(_newToEnable):
//				_newToEnable = null
//				Separator()
//				return			
//			_rageButton.Data.ToEnableOnClick.Add(_newToEnable);
//			_newToEnable = null
//			Separator()
	
	protected override def OnGuiRendered():
		_rageButton = target if _rageButton == null
		SetDirty(target) if GUI.changed
