import UnityEngine
import UnityEditor

[CustomEditor(typeof(RageGroup))]
public class RageGroupEditor (RageToolsEdit): 

	private _tweak as bool
	private _showList as bool
	private _showStyle as bool
	private _showPhysics as bool
	private _rageGroup as RageGroup
	private _newGroup as RageGroup
	private _newStyle as RageSplineStyle
	private _missingMemberItem as bool
	
	private _deleteButton as Texture2D = Resources.Load('deletebutton', Texture2D)

	public def OnDrawInspectorHeaderLine():
		_rageGroup = target if _rageGroup == null

		if GUILayout.Button("Reset", MaxWidth(60f), GUILayout.MinHeight(20f)):
			RegisterUndo("RageGroup: Reset")
			_rageGroup.Reset()
			
		if GUILayout.Button("Update", GUILayout.MinHeight(20f)):
			RegisterUndo("RageGroup: Update")
			_rageGroup.UpdatePathList()
			_rageGroup.AaMult = 1f
			_rageGroup.DensityMult = 1f
		

	public def OnDrawInspectorGUI():
		_rageGroup = target if _rageGroup == null			
		if _rageGroup.List == null or _rageGroup.List.Count==0:
			EasyRow:
				Warning(" * No Members. Click on 'Update' to initialize")

		Separator()
				
		EasyRow:
			LookLikeControls(40f)
			EasyIntField "Step", _rageGroup.UpdateStep, MaxWidth(65f)
			GUILayout.Space (3);
			EasyToggle "Draft", _rageGroup.Draft, MaxWidth(45f)
			GUILayout.Space (3);
			EasyToggle "Tweak", _rageGroup.Tweak, MaxWidth(60f)
			if _rageGroup.Tweak:
				_rageGroup.CheckForEdgetune()
			EasyToggle "Multiply", _rageGroup.Proportional, MaxWidth(90f)
		
		AddAntiAliasAndDensityCheck()
		EasyRow:
			EasyToggle "Visible", _rageGroup.Visible, MaxWidth(60f)
			if _rageGroup.Visible:
				if _rageGroup.Proportional:	
					LookLikeControls(110f,40f)
					EasyPercent "Opacity x", _rageGroup.OpacityMult, 1
				else:
					LookLikeControls(110f,40f)
					EasyPercent "Opacity", _rageGroup.Opacity, 1

		AddProportionalWarningCheck()
											
		AddRageStyleList()
		AddMemberList()	
		AddIgnoreGroupList()

		if (Event.current.type == EventType.ValidateCommand and Event.current.commandName == "UndoRedoPerformed"):
		  Repaint()
		EditorUtility.SetDirty (_rageGroup)
								
	private def AddIgnoreGroupList():
		LookLikeControls(110f, 50f)
		EasyFoldout "Ignore Groups (" + _rageGroup.ExcludedGroups.Count + ")", _rageGroup.GroupExclusion:
			EasyRow:
				EasyCol:
					GUILayout.Space (3);
				EasyCol:
					LookLikeControls(110f, 100f)
					for i in range(0, _rageGroup.ExcludedGroups.Count):		
						EasyRow:
							EasyObjectField	"", _rageGroup.ExcludedGroups[i], typeof(RageGroup)
							if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
								_rageGroup.ExcludedGroups.RemoveAt(i)
								_rageGroup.UpdatePathList()
								break
					EasyRow:
						LookLikeControls(15f, 10f)
						EasyObjectField	"+", _newGroup, typeof(RageGroup)
				
		if _newGroup != null:			
			if _newGroup == _rageGroup or _rageGroup.ExcludedGroups.Contains(_newGroup):
				_newGroup = null
				Separator()
				AddMemberList()
				return			

			_rageGroup.AddExcludedGroupIfPossible(_newGroup)
			_rageGroup.UpdatePathList()
			_newGroup = null
	
		
	private def AddRageStyleList():
		_rageGroup = target if _rageGroup == null
		return if _rageGroup.Styles == null
		EasyFoldout "Styles (" + _rageGroup.Styles.Count + ")", _rageGroup.UseStyles:
			LookLikeControls(0f,0f)		
			EasyRow:
				EasyCol:
					for i in range(0, _rageGroup.Styles.Count):
						GUILayout.Label("Name") if i==0
						EasyObjectField	"", _rageGroup.Styles[i], typeof(ScriptableObject)
				EasyCol:
					for i in range(0, _rageGroup.Styles.Count):	
						GUILayout.Label("Filter") if i==0
						EasyRow:
							EasyTextField	"", _rageGroup.StyleNames[i]
							if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
								_rageGroup.Styles.RemoveAt(i)
								_rageGroup.StyleNames.RemoveAt(i)
								break
			EasyRow:
				LookLikeControls(20f, 1f)	
				EasyObjectField	"+", _newStyle, typeof(ScriptableObject)									
			EasyRow:
				Separator()
			EasyRow:
				if _rageGroup.Styles.Count > 0:
					if GUILayout.Button("Apply Styles"):
						RegisterUndo("RageGroup: Apply Styles")
						_rageGroup.ApplyStyle()
					if GUILayout.Button("Apply Physics"):
						RegisterUndo("RageGroup: Apply Style Physics")
						_rageGroup.ApplyStylePhysics()
						return;
																				
		if _newStyle != null:
			_rageGroup.AddStyle(_newStyle)
			_newStyle = null
						
	private def AddAntiAliasAndDensityCheck():
		return if not _rageGroup.Tweak
		
		LookLikeControls(70f, 10f)	
		
		EasyRow:		
			if _rageGroup.Proportional:		
				EasyFloatField "AntiAlias x", _rageGroup.AaMult, MinWidth(120f)
				EasyFloatField "Density x", _rageGroup.DensityMult, MinWidth(120f)
				return
				
			EasyFloatField "AntiAlias", _rageGroup.AntiAlias, MinWidth(120f)
			EasyIntField "Density", _rageGroup.Density, MinWidth(120f)


	private def AddProportionalWarningCheck():
		return if not _rageGroup.Tweak
		return if not _rageGroup.Proportional
		EasyRow:
			Warning("* Proportional On. Click 'Update' when done.")

	private def AddMemberList():
		_rageGroup = target if _rageGroup == null
		EasyFoldout "Members (" + _rageGroup.List.Count + ")", _showList:
			EasyRow:
				Separator()
				EasyCol:
					GUILayout.Label("Name", EasyStyles.ListTitle(), GUILayout.MaxWidth(120f))
					for item in _rageGroup.List:
						continue if item == null or item.Spline == null 
						if item.Spline.Rs == null:
							_missingMemberItem = true
							continue
						EasyRow:
							GUILayout.Label(item.Spline.GameObject.name, EasyStyles.ListItem(), GUILayout.MaxWidth(120f))
					if _missingMemberItem == true:
						_rageGroup.UpdatePathList()
						_missingMemberItem = false
				Separator()
				EasyCol:
					GUILayout.Label("Def.AA", EasyStyles.ListTitle(), GUILayout.MaxWidth(50f))
					for item in _rageGroup.List:
						continue if item == null or item.Spline == null
						EasyRow:
							GUILayout.Label(item.DefaultAa.ToString(), EasyStyles.ListItem(), GUILayout.MaxWidth(50f))
				Separator()
				EasyCol:
					GUILayout.Label("Def.Dns", EasyStyles.ListTitle(), GUILayout.MaxWidth(50f))
					for item in _rageGroup.List:
						continue if item == null or item.Spline == null
						EasyRow:
							GUILayout.Label(item.DefaultDensity.ToString(), EasyStyles.ListItem(), GUILayout.MaxWidth(50f))
			Separator()

	protected override def OnGuiRendered():
		_rageGroup = target if _rageGroup == null
		SetDirty(target) if GUI.changed or _rageGroup.Tweak or _rageGroup.IsRefreshing
