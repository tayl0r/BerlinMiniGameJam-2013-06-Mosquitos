import UnityEngine
import UnityEditor

[CustomEditor(typeof(RageSprite))]
public class RageSpriteEditor (RageToolsEdit):

	[SerializeField]private static _showFrames as bool
	
	private _rageSprite as RageSprite
	private _deleteButton as Texture2D = Resources.Load('deletebutton', Texture2D)
	private _moveUpButton as Texture2D = Resources.Load('moveup', Texture2D)
	private _moveDownButton as Texture2D = Resources.Load('movedown', Texture2D)
	private _playButton as Texture2D
	private _playButtonOn as Texture2D = Resources.Load('playon', Texture2D)
	private _playButtonOff as Texture2D = Resources.Load('playoff', Texture2D)
	private _multiButton as Texture2D
	private _multiButtonOn as Texture2D = Resources.Load('multion', Texture2D)
	private _multiButtonOff as Texture2D = Resources.Load('multioff', Texture2D)
	[SerializeField]private static _multiAdd as bool
	private _enablePreview as bool = true

	private _newTarget as GameObject	

	public def OnDrawInspectorHeaderLine():
		_rageSprite = target if _rageSprite == null

	public def OnDrawInspectorGUI():

		if (Application.isPlaying):
			GUILayout.Label("* Inspector disabled during play", EditorStyles.toolbarButton)
			return

		_rageSprite = target if _rageSprite == null
		EasyRow:
			if GUILayout.Button("New Animation", GUILayout.MinHeight(20f)):
				newRsAnimation = RageSpriteAnimation()
				newRsAnimation.HostRageSprite = _rageSprite
				_rageSprite.Animations.Add(newRsAnimation)
				return;
	
			if GUILayout.Button(GUIContent("Generate Clips", "Generate All Clips"), GUILayout.MinHeight(20f)):
				RegisterUndo("RageSprite: Generate All Clips")
				for animation in _rageSprite.Animations.ToArray():
					_rageSprite.GenerateClip(animation)
				return;

		ListAnimations() if not _rageSprite.Animations == null

		Separator()
			
		EasySettings:
			EasyRow:
				EasySpacer 8
				EasyToggle "Enable Preview", _enablePreview
				EasyToggle "Draft Preview", _rageSprite.DraftPreview
		
		EditorUtility.SetDirty(_rageSprite) if GUI.changed
		Repaint() if (not Application.isPlaying)

	public def ListAnimations():

		for animation in _rageSprite.Animations.ToArray():
			
			if (not Application.isPlaying):
	            animation.CheckPreviewFrame()

			EasyRow:
				EasySpacer 5
				EasyCol:
					EasyFoldout animation.Name, animation.ExpandAnimation, 2F:

						//If open, minimize all other animation foldouts
						for otherAnimation in _rageSprite.Animations.ToArray():
							if (otherAnimation == animation): continue
							otherAnimation.ExpandAnimation = false
							otherAnimation.IsPreviewing = false

						preview as RageSpritePreviewData
						EasyRow:
							EasySpacer 2
							EasyCol:
								if animation.Frames.Count > 0 and _enablePreview:
									rect = EditorGUILayout.BeginVertical()
									rect.height = 80f
									rect.width = 80f
									rect.y += 10
									rect.x += 10
	
									preview = animation.Frames[animation.CurrentFrameIdx].GetPreview()

									preview.Draw(rect, _rageSprite.DraftPreview)
									EditorGUILayout.EndVertical()
									GUILayout.Box("", EditorStyles.label, GUILayout.MinHeight(100f), MinWidth(100f), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false))
								else:
									GUILayout.Box("", GUILayout.MinHeight(100f), MinWidth(100f), GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false))
						
							EasyCol:
								EasyRow:
									LookLikeControls(40f)
									EasyTextField "", animation.Name, GUILayout.MaxWidth(105f)
									if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
										RegisterUndo("RageSprite: Delete Animation")
										animation.RemoveAllFrames()
										_rageSprite.Animations.Remove(animation)
		
								EasyRow:
									EasyToggleButton _playButton, _playButtonOn, _playButtonOff, animation.IsPreviewing, "Preview Animation":
										return
									if GUILayout.Button("Generate Clip", GUILayout.MinHeight(21f), GUILayout.MaxWidth(100f)):
										RegisterUndo("RageSprite: Generate Clip")
										_rageSprite.GenerateClip(animation)
								
								EasyRow:
									Separator()
									
								EasyRow:
									if animation.Frames.Count > 0:
										animation.CurrentFrameIdx = GUILayout.HorizontalSlider(animation.CurrentFrameIdx, 0, animation.Frames.Count-1, GUILayout.MaxWidth(120f))
							
						EasyRow:
							EasySpacer 1
							EasyCol:
								EasyFoldout "Frames", animation.ExpandFrames:														
									EasyRow:
										EasySpacer 2
										LookLikeControls(161f, 25f)
										EasyFloatField "Base Frame Delay:", animation.FrameDuration, GUILayout.MaxWidth(200f)
									DrawFrameList (animation)
			Separator(1)
			EasyLine 8
	
	protected override def OnGuiRendered():
		_rageSprite = target if _rageSprite == null
	
	public def DrawFrameList(animation as RageSpriteAnimation):
		frameList as System.Collections.Generic.List[of RageSpriteFrame] = animation.Frames
		newAnimFrame as GameObject
		LookLikeControls(110f, 50f)
		EasyRow 2f:
			GUILayout.Label ("Frames (" + frameList.Count + "):")
		EasyRow 2f:
			EasyCol:
				LookLikeControls(110f, 100f)
				for i in range(0, frameList.Count):
					EasyRow:
						if GUILayout.Button(_moveDownButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
							if Event.current.shift:
								animation.MoveFrameToBottom(i)
							else:
								animation.MoveFrameDown(i);
							break
						if GUILayout.Button(_moveUpButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
							if Event.current.shift:
								animation.MoveFrameToTop(i)
							else:
								animation.MoveFrameUp(i)
							break
						EasyGameObjectField	"", frameList[i].gO, GUILayout.MaxWidth(109f)
						EasyFloatField	"", frameList[i].Delay, GUILayout.MaxWidth(40f)
						if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
							if Event.current.shift:
								animation.RemoveAllFrames()
							else:
								animation.RemoveCheck(i)
							break
				EasyRow:
					LookLikeControls(15f, 10f)
					EasyGameObjectField	"+", newAnimFrame, GUILayout.MaxWidth(201f)
					EasyToggleButton _multiButton, _multiButtonOn, _multiButtonOff, _multiAdd, "Add Immediate Children"

		if newAnimFrame != null:			
			if (_multiAdd):
				animation.AddChildFrames(newAnimFrame)
			else:
				if (Selection.gameObjects.Length == 1):
					animation.AddFrame(newAnimFrame)
				else:
					animation.AddFrames(Selection.gameObjects)
			newAnimFrame = null

