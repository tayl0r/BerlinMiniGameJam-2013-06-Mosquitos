[CustomEditor(typeof(RageIkJointLimiter))]
public class RageIkJointLimiterEditor (RageToolsEdit): 
	
	private _rageIkJointLimiter as RageIkJointLimiter

	protected override def OnDrawInspectorHeaderLine():
		_rageIkJointLimiter = target if _rageIkJointLimiter == null
		EasyRow:
			EasyToggle "Live", _rageIkJointLimiter.Live
			EasyFloatField "Rest Angle", _rageIkJointLimiter.RestAngle	
		
	protected override def OnDrawInspectorGUI():
		_rageIkJointLimiter = target if _rageIkJointLimiter == null
		EasyRow:
			LookLikeControls(70f, 30f)
			EasyFloatField "Min Angle", _rageIkJointLimiter.MinLimiterAngle
			EasyFloatField "Max Angle", _rageIkJointLimiter.MaxLimiterAngle
		EasyRow:
			EasyToggle "Draw gizmos", _rageIkJointLimiter.DrawGizmos
			if _rageIkJointLimiter.DrawGizmos:	
				LookLikeControls(50f,50f)	
				EasyFloatField "Radius", _rageIkJointLimiter.GizmoRadius	
		
	public def OnSceneGUI():
		_rageIkJointLimiter = target if _rageIkJointLimiter == null
		
		return if (not _rageIkJointLimiter.DrawGizmos)
		
		MaxAngle as Vector3
		MaxAngle = _rageIkJointLimiter.MaxAngle  * _rageIkJointLimiter.GizmoRadius
		MaxAngle = Handles.PositionHandle (MaxAngle + _rageIkJointLimiter.transform.position, Quaternion.identity) - _rageIkJointLimiter.transform.position
		_rageIkJointLimiter.MaxAngle = MaxAngle;
		
		MinAngle as Vector3
		MinAngle = _rageIkJointLimiter.MinAngle  * _rageIkJointLimiter.GizmoRadius
		MinAngle = Handles.PositionHandle (MinAngle + _rageIkJointLimiter.transform.position, Quaternion.LookRotation(Vector3.down)) - _rageIkJointLimiter.transform.position
		_rageIkJointLimiter.MinAngle = MinAngle;
			
		Handles.color = Color(1,1,1,0.2)	
		Handles.DrawSolidArc(_rageIkJointLimiter.transform.position, Vector3.forward, MaxAngle , _rageIkJointLimiter.AngleLimits(), _rageIkJointLimiter.GizmoRadius)
		
		Handles.color = Color(1,0,0,1) 
		if (_rageIkJointLimiter.ValidVector(_rageIkJointLimiter.RestDirection)):
	    	Handles.color = Color(0,1,0,1)
		Handles.DrawLine(_rageIkJointLimiter.transform.position, _rageIkJointLimiter.transform.position + _rageIkJointLimiter.RestDirection * _rageIkJointLimiter.GizmoRadius)
