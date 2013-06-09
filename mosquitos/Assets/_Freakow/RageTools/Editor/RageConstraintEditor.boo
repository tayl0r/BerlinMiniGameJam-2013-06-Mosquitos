[CustomEditor(typeof(RageConstraint))]
public class RageConstraintEditor (RageToolsEdit): 
	
	private _RageConstraint as RageConstraint
	private _Edgetune as RageEdgetune

	public def OnDrawInspectorHeaderLine():
		_RageConstraint = target if _RageConstraint == null
		
		LookLikeControls(20f, 1f)
		EasyToggle "Live", _RageConstraint.Live, MaxWidth(45f)
		LookLikeControls(60f, 1f)		
		EasyObjectField	"Follower:", _RageConstraint.Follower, typeof(GameObject)		

	public def OnDrawInspectorGUI():
		_RageConstraint = target if _RageConstraint == null
		
		LookLikeControls(60f)
		EasyRow:		
			EasyToggle "Position", _RageConstraint.FollowPosition
			EasyToggle "Rotation", _RageConstraint.FollowRotation		
			EasyToggle "Scale", _RageConstraint.FollowScale
			EasyToggle "Local", _RageConstraint.Local

		if not _RageConstraint.FollowerGroup == null:
			EasyRow:
				EasyToggle "Visible", _RageConstraint.GroupVisible, MaxWidth(80f)
				if _RageConstraint.GroupVisible:
					if _RageConstraint.FollowerGroup.Proportional:	
						LookLikeControls(110f,30f)
						EasyPercent "Opacity x", _RageConstraint.FollowerGroup.OpacityMult, 1
					else:
						LookLikeControls(110f,30f)
						EasyPercent "Opacity", _RageConstraint.FollowerGroup.Opacity, 1

		EasySettings:
			EasyRow:
				LookLikeControls(70f, 10f)
				EasyFloatField "Snap: Pos", _RageConstraint.PositionSnap, MaxWidth(100f)
				LookLikeControls(30f, 1f)
				EasyFloatField "Rot", _RageConstraint.RotationSnap, MaxWidth(60f)
				EasyFloatField "Scl", _RageConstraint.ScaleSnap, MaxWidth(60f)
			if (_RageConstraint.FollowPosition):
				EasyRow:
					GUILayout.Label("Position:", MaxWidth(60f))
					EasyToggle "x", _RageConstraint.FollowPositionX, MaxWidth(30f)
					EasyToggle "y", _RageConstraint.FollowPositionY, MaxWidth(30f)
					EasyToggle "z", _RageConstraint.FollowPositionZ, MaxWidth(30f)
			if (_RageConstraint.FollowRotation):
				EasyRow:
					GUILayout.Label("Rotation:", MaxWidth(60f))
					EasyToggle "x", _RageConstraint.FollowRotationX, MaxWidth(30f)
					EasyToggle "y", _RageConstraint.FollowRotationY, MaxWidth(30f)
					EasyToggle "z", _RageConstraint.FollowRotationZ, MaxWidth(30f)
			if (_RageConstraint.FollowScale):
				EasyRow:
					GUILayout.Label("Scale:", MaxWidth(60f))
					EasyToggle "x", _RageConstraint.FollowScaleX, MaxWidth(30f)
					EasyToggle "y", _RageConstraint.FollowScaleY, MaxWidth(30f)
					EasyToggle "z", _RageConstraint.FollowScaleZ, MaxWidth(30f)

		EditorUtility.SetDirty (_RageConstraint)
		if not _RageConstraint.FollowerGroup == null:
			EditorUtility.SetDirty (_RageConstraint.FollowerGroup)

