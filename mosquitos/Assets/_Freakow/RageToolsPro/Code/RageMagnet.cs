using UnityEngine;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
[AddComponentMenu("RageTools/Rage Magnet")]
public class RageMagnet: MonoBehaviour {

	public enum ColliderTypes { Sphere = 0, Capsule = 1 }
	public enum ColliderDirections { XAxis = 0, YAxis = 1, ZAxis = 2 }

	public ColliderTypes ColliderType;
	public RageMagnetPoint[] Points = new RageMagnetPoint[0];

	public bool On;
	public float InnerSize = 50;
	public bool VaryStrength;
	public float StrengthMin = 0.2f;
	public float StrengthMax = 1f;
	public bool ShowGizmos = true;
	public bool ShowLocalWeights = true;
	public static float GizmoSizeMultiplier = 1f;
	public bool Changed;

	[SerializeField] private RageGroup _group;
	[SerializeField] private RageGroup _lastGroup;
	public RageGroup Group {
		get {
		    return !CheckGroup() ? null : _group;
		}

	    set {
			if (_lastGroup == _group) return;
			_lastGroup = _group;
			_group = value;
			RefreshGroupDataCheck();
		}
	}

	[SerializeField] internal GameObject InnerColliders;
	[SerializeField] internal Quaternion RestRotation;
	[SerializeField] internal Vector3 RestPosition;
	[SerializeField] internal Vector3 RestScale;
	[SerializeField] internal bool ResetValues;

	public ISpline[] TouchingSplines = new ISpline[0];
	//[SerializeField] private RageMagnetGroupData _myGroupData;

	private Vector3 _lastScale = Vector3.zero;
	private Vector3 _lastPos = Vector3.zero;
	private Quaternion _lastRotation = Quaternion.identity;
	public bool ForceRefresh;

	internal const string InnerRadiusId = "InnerRadius";
	private const float InitialSize = 20;

	#region properties

	public bool Live {
		get {
			if (Group == null) return false;
			if (Group.MagnetData != null && Group.MagnetData.Live != On)
				On = Group.MagnetData.Live;
			return On;
		}
		set {
		    RageGroup rageGroup = Group;
            if(rageGroup == null) return;
		    rageGroup.MagnetData.Live = value;
			On = value; 
        }
	}

	public bool DraftMode {
		get { return Group.MagnetData.DraftMode; }
		set { Group.MagnetData.DraftMode = value; }
	}

	public bool ShowWeights {
		get { return Group.MagnetData.ShowGizmos; }
		set { Group.MagnetData.ShowGizmos = value; }
	}

// 	public RageMagnetGroupData MyGroupData {
// 		get{
// 			if(_myGroupData == null)
// 				RefreshGroupDataCheck();
// 			return _myGroupData;
// 		}
// 		set { _myGroupData = value; }
// 	}

	public bool Normalize {
		get { return Group.MagnetData.Normalize; }
		set { Group.MagnetData.Normalize = value; }
	}

	public Collider ActiveInnerCollider {
		get {
			if (ColliderType == ColliderTypes.Capsule)
				return InnerColliders.GetComponent<CapsuleCollider>();
			return InnerColliders.GetComponent<SphereCollider>();
		}
	}

	public bool ActiveColliderIsSphere {
		get {return (ColliderType == ColliderTypes.Sphere);}
	}

	public Collider ActiveCollider {
		get {
			if (ActiveColliderIsSphere) return GetComponent<SphereCollider>();
			return GetComponent<CapsuleCollider>();
		}
	}

	public float ColliderRadius {
		get {
			if (ActiveColliderIsSphere) return GetComponent<SphereCollider>().radius;
			return GetComponent<CapsuleCollider>().radius;
		}
		set {
			GetComponent<SphereCollider>().radius = value;
			GetComponent<CapsuleCollider>().radius = value;
		}
	}

	public float ColliderHeight {
		get { return GetComponent<CapsuleCollider>().height; }
		set { GetComponent<CapsuleCollider>().height = value; }
	}

	public float CenterX {
		get {
			return ActiveColliderIsSphere ? ((SphereCollider) ActiveCollider).center.x : ((CapsuleCollider) ActiveCollider).center.x;
		}
		set {
			var currCenter = ActiveColliderIsSphere ? GetComponent<SphereCollider>().center : GetComponent<CapsuleCollider>().center;
			if (ActiveColliderIsSphere) {
				GetComponent<SphereCollider>().center = new Vector3(value, currCenter.y, currCenter.z);
			} else {
				GetComponent<CapsuleCollider>().center = new Vector3(value, currCenter.y, currCenter.z);
			}
		}
	}

	public float CenterY {
		get {
			return ActiveColliderIsSphere ? ((SphereCollider) ActiveCollider).center.y : ((CapsuleCollider) ActiveCollider).center.y;
		}
		set {
			var currCenter = ActiveColliderIsSphere ? GetComponent<SphereCollider>().center : GetComponent<CapsuleCollider>().center;
			if (ActiveColliderIsSphere) {
				GetComponent<SphereCollider>().center = new Vector3(currCenter.x, value, currCenter.z);
			} else {
				GetComponent<CapsuleCollider>().center = new Vector3(currCenter.x, value, currCenter.z);
			}
		}
	}

	public float CenterZ {
		get {
			return ActiveColliderIsSphere ? ((SphereCollider) ActiveCollider).center.z : ((CapsuleCollider) ActiveCollider).center.z;
		}
		set {
			var currCenter = ActiveColliderIsSphere ? GetComponent<SphereCollider>().center : GetComponent<CapsuleCollider>().center;
			if (ActiveColliderIsSphere) {
				GetComponent<SphereCollider>().center = new Vector3(currCenter.x, currCenter.y, value);
			} else {
				GetComponent<CapsuleCollider>().center = new Vector3(currCenter.x,currCenter.y, value);
			}
		}
	}

	public ColliderDirections ColliderDirection {
		get { return IntToDirection(GetComponent<CapsuleCollider>().direction); }
		set { GetComponent<CapsuleCollider>().direction = DirectionToInt(value); }
	}

	private static ColliderDirections IntToDirection(int direction) {
		switch (direction) {
			case 2:
				return ColliderDirections.ZAxis;
			case 1:
				return ColliderDirections.YAxis;
			default:
				return ColliderDirections.XAxis;
		}
	}

	private static int DirectionToInt(ColliderDirections direction) {
		if (ColliderDirections.YAxis == direction)
			return 1;
		return ColliderDirections.ZAxis == direction ? 2 : 0;
	}

	public float InnerColliderRadius {
		get {
			if (ActiveInnerCollider is SphereCollider)
				return ((SphereCollider)ActiveInnerCollider).radius;
			if (ActiveInnerCollider is CapsuleCollider)
				return ((CapsuleCollider)ActiveInnerCollider).radius;
			return 0f;
		}
	}

	public SphereCollider SphereCollider { get { return GetComponent<SphereCollider>(); } }
	public CapsuleCollider CapsuleCollider { get { return GetComponent<CapsuleCollider>(); } }
	public SphereCollider InnerSphereCollider { get { return InnerColliders.GetComponent<SphereCollider>(); } }
	public CapsuleCollider InnerCapsuleCollider { get { return InnerColliders.GetComponent<CapsuleCollider>(); } }
	#endregion

	public void Start() { 
		UpdateGroupRestPosition();
	}

// 	public bool ExecutionCheck() {
// 		if(_group == null) return false;
// 
// 		RefreshGroupDataCheck();
// 		this.FixCollidersCheck();
// 		return true;
// 	}

	public void Update() {
		if (!Live) return;

		Vector3 scaleOffset = transform.localScale - RestScale;
		Vector3 posOffset = ActiveCollider.bounds.center - RestPosition;

		if (Points == null) UpdateRestPosition();
		if (Points == null) return;

		Changed = ChangedCheck();
		if (!Changed && !ForceRefresh) return;

		Group.MagnetsForceRefresh();
		foreach (RageMagnetPoint ragePoint in Points)
			this.CalculateDeformations(ragePoint, posOffset, scaleOffset);
	}
 
	public void LateUpdate() {
		if (!Live) return;
		if (!Changed && !ForceRefresh) return;

		AveragePointsInfluence();
		RefreshAffectedSplines();

		_lastScale = transform.localScale;
		_lastPos = ActiveCollider.bounds.center;
		_lastRotation = transform.rotation;
		//TODO: Must set ForceRefresh to False *in grouppro*
		//ForceRefresh = false;
	}

	public bool ChangedCheck( ) {
		return !_lastScale.Equals(transform.localScale)
			   || !_lastPos.Equals(ActiveCollider.bounds.center)
			   || !_lastRotation.Equals(transform.rotation);
	}

    private bool CheckGroup(){
        if (_group != null) return true;
        _group = FindParentRageGroup(gameObject);
        if (_group != null) return true;
        return false;
    }

	private void RefreshGroupDataCheck() {
        if (!CheckGroup()) return;
		
		// Finds and adds all magnets associated to this RageGroup
		Object[] magnets = FindSceneObjectsOfType(typeof(RageMagnet));
		foreach (RageMagnet magnet in magnets) {
			if (magnet.Group != Group) continue;
			if (!Group.MagnetList.Contains(magnet))
				Group.MagnetList.Add(magnet);
		}

// 		//TODO: Check if MyGroupData is still needed => Remove surgically
// 		if (_myGroupData != null) {
// 			if (_myGroupData.Group == null) _myGroupData.Group = Group;
// 			if (_lastGroup == _group) return;
// 		}
// 
// 		if(_myGroupData != null && Group == _myGroupData.Group) return;
// 
// 		foreach(RageMagnet magnet in magnets) {
// 			if (magnet == this) continue;
// 			if (magnet.Group != Group) continue;
// 			if (magnet._myGroupData == null) continue;
// 			
// 			_myGroupData = magnet._myGroupData;
// 			return;
// 		}
// 		_myGroupData = (RageMagnetGroupData)ScriptableObject.CreateInstance(typeof(RageMagnetGroupData));
// 		_myGroupData.Group = Group;
	}

	public void UpdateGroupRestPosition(){
        if(Group == null) return;
		RefreshGroupDataCheck();
		this.FixCollidersCheck();
		Group.MagnetsUpdateRestPositon();
		if(Normalize) NormalizeInfluences();
	}

	public void UpdateRestPosition() {
		if (Group == null) {
			Debug.Log("No RageGroup set for " + gameObject.name + ". Please assign it.");
			return;
		}
		Group.UpdatePathList();

		if (ActiveCollider == null) {
			Debug.Log("RageMagnet error: Missing collider.");
			return;
		}
		RestPosition = ActiveCollider.bounds.center;
		RestRotation = transform.rotation;
		RestScale = transform.localScale;

		CalculateContainedPoints();
	}

	private void RefreshAffectedSplines() {
		if(TouchingSplines == null) return;
		foreach(ISpline spline in TouchingSplines) {
			if(!spline.Changed) continue;
			spline.IsDirty = true;
		}
		Group.QueueDirtyRefresh();
	}

	private static RageGroup FindParentRageGroup(GameObject go) {
		var rageGroup = go.transform.GetComponent<RageGroup>();
		if (rageGroup != null) return rageGroup;
		if(go.transform.parent == null) return null;
		var parentGameObject = go.transform.parent.gameObject;
		return parentGameObject == null ? null : FindParentRageGroup(parentGameObject);
	}

	private void CalculateContainedPoints() {
		TouchingSplines = null;
		RageMagnetPoint.RemoveMagnetsFromPoint(this, Points);

		List<ISpline> splines = new List<ISpline>();
		List<RageMagnetPoint> points = new List<RageMagnetPoint>();

		foreach (RageGroupElement item in Group.List) {
			ISpline spline = item.Spline;
			if(spline == null) continue;
			for (int i = 0; i < (spline).PointsCount; i++){
				FeedColliderData(points, spline, i);
				if (!splines.Contains(spline))
					splines.Add(spline);
			}
		}

		TouchingSplines = splines.ToArray();
		Points = points.ToArray();
	}

	private void NormalizeInfluences() {

		var points = RageMagnetPoint.GetAllMagnetPoints(Group);
		foreach(RageMagnetPoint magnetPoint in points) {
			var pointOffsetSum = 0f;
			var inCtrlOffsetSum = 0f;
			var outCtrlOffsetSum = 0f;
			var pointsData = magnetPoint.RelatedData;
			int count = 0;

			foreach (RageMagnetPointData point in pointsData) {
				pointOffsetSum += point.PointOffset;
				inCtrlOffsetSum += point.InCtrlOffset;
				outCtrlOffsetSum += point.OutCtrlOffset;
				count++;
			}

			if(pointOffsetSum < count && count > 1){
				foreach (var point in pointsData){
					point.PointOffset = 1;
					point.InCtrlOffset = 1;
					point.OutCtrlOffset = 1;
				}
				continue;
			}

			bool normalizePos = pointOffsetSum > 1f;
			foreach(var point in pointsData) {
				if(!normalizePos)
					continue;
				point.PointOffset *= count / pointOffsetSum;
				point.InCtrlOffset *= count / inCtrlOffsetSum;
				point.OutCtrlOffset *= count / outCtrlOffsetSum;
			}
		}
	}

	private void FeedColliderData(ICollection<RageMagnetPoint> pointList, ISpline spline, int index)
	{
		ISpline rageSpline = spline;
		if (!this.ActiveColliderContains (rageSpline, index)) return;

		float pointOffset = 1;
		float inCtrlOffset = 1;
		float outCtrlOffset = 1;

		var point = rageSpline.GetPointAt(index);
		var inCtrlPos = point.InTangent;
		var outCtrlPos = point.OutTangent;
		var pointPos = point.Position;

		if (VaryStrength)
			RageMagnetMath.AdjustOffsetCheck (ActiveCollider, pointPos, inCtrlPos, outCtrlPos, 
							ref pointOffset, ref inCtrlOffset, ref outCtrlOffset, StrengthMin, StrengthMax,
							Quaternion.Inverse(transform.rotation));

		if(!this.ActiveInnerColliderContains (rageSpline, index)) {
			var nullifyRotation = Quaternion.Inverse(transform.rotation);
			var centerPosition = ActiveCollider.bounds.center;
			
			pointOffset *= Mathf.InverseLerp (ColliderRadius, InnerColliderRadius,
							this.SphericalProjectionDistance(centerPosition, pointPos, nullifyRotation));

			if(!ActiveInnerCollider.bounds.Contains (inCtrlPos))
				inCtrlOffset *= Mathf.InverseLerp(ColliderRadius, InnerColliderRadius,
								 this.SphericalProjectionDistance(centerPosition, inCtrlPos, nullifyRotation));

			if (!ActiveInnerCollider.bounds.Contains (outCtrlPos))
				outCtrlOffset *= Mathf.InverseLerp (ColliderRadius, InnerColliderRadius,
								  this.SphericalProjectionDistance(centerPosition, outCtrlPos, nullifyRotation));
		}

		var containedPoint = RageMagnetPoint.Instantiate (this, rageSpline, index, pointOffset, inCtrlOffset,
												outCtrlOffset, pointPos, inCtrlPos, outCtrlPos, RestPosition);
		pointList.Add(containedPoint);
	}

	private void AveragePointsInfluence(){
		if (Points == null) return;
		foreach (RageMagnetPoint point in Points) {
			if (point == null) continue;
			if (!point.Changed) continue;

			point.AverageMagnetPoint();
			point.Spline.Changed = true;
			point.Changed = false;
		}
	}

	public void OnDrawGizmos() {
		CheckDrawGizmoSettings("magnetunselectedicon.png", "magnetdisabledicon.png");
	}

	public void OnDrawGizmosSelected() {
		CheckDrawGizmoSettings("magnetselectedicon.png","magnetdisabledicon.png");
	}

	public void CheckDrawGizmoSettings(string liveGizmoIcon, string notLiveGizmoIcon) {
		if (ShowGizmos) 
			Gizmos.DrawIcon(transform.position, Live ? liveGizmoIcon : notLiveGizmoIcon);

        if(Group == null) return;
		if (Group.MagnetData == null) return;
		if (!Group.MagnetData.ShowGizmos) return;
		if (!ShowLocalWeights) return;

		DrawWeights();  
	}

	private void DrawWeights(){		
		try{
			float gizmoWidth = GizmoSizeMultiplier;
			foreach (RageMagnetPoint point in Points){
				var relatedData = point.FindRelatedData(this);
				if (relatedData == null) continue;
				if (relatedData.PointOffset > 0){
					Gizmos.color = new Color(relatedData.PointOffset, 0, 1 - relatedData.PointOffset, .2f);
					Gizmos.DrawSphere(point.Spline.GetPointAt(point.Index).Position, gizmoWidth);
					continue;
				}

				Gizmos.color = Color.black;
				Gizmos.DrawWireSphere(relatedData.AbsolutePointPos, gizmoWidth/2);
			}
		// ReSharper disable EmptyGeneralCatchClause
		} catch {
		// ReSharper restore EmptyGeneralCatchClause
			//ignore
		}
	}

}
