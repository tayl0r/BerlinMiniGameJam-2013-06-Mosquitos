using Microsoft.Xna.Framework;
using UnityEngine;

/// <summary> Constrains (follows) a certain object transform position, rotation and scale, selectively </summary>
[AddComponentMenu("RageTools/Rage Constraint")]
[ExecuteInEditMode]
public class RageConstraint : MonoBehaviour {

	[SerializeField]private GameObject _follower;
	public GameObject Follower {
		get { return _follower; }
		set {
			if (_follower == value) return;
			_follower = value;
			FollowerGroup = _follower.GetComponent<RageGroup>();
		}
	}
	[SerializeField]private Transform _followerTransform;
	public Transform FollowerTransform {
		get {	if (_followerTransform == null) _followerTransform = Follower.transform;
				return _followerTransform; }
		set {	if (_followerTransform == value) return;
				_followerTransform = value; }
	}
	public bool GroupVisible {
		get {
			if (FollowerGroup == null) return false;
			Visible = FollowerGroup.Visible;
			return FollowerGroup.Visible;
		}
		set {
			SetGroupVisibility(value);
		}
	}
	public bool Live;
	public bool FollowPosition, FollowPositionX = true, FollowPositionY = true, FollowPositionZ = true;
	public bool FollowRotation, FollowRotationX = true, FollowRotationY = true, FollowRotationZ = true;
	public bool FollowScale, FollowScaleX = true, FollowScaleY = true, FollowScaleZ = true;
	public bool Local;
	public bool Visible;
	public float RotationSnap;
	public float PositionSnap;
	public float ScaleSnap;
	public RageGroup FollowerGroup;
	private RageEdgetune _edgetune;

	public void Update() {
		UpdateFollower();
		if (!Live || Follower == null) return;
		if (_edgetune == null)
			_edgetune = Follower.GetComponent<RageEdgetune>();
		if (_edgetune == null) return;
		_edgetune.Group.QueueRefresh();
	}

	private void UpdateFollower( ) {
		if (!Live) return;
		if (!Follower) return;

		SetGroupVisibility (Visible);

		if (FollowPosition) CopyPosition();

		if (FollowRotation) CopyRotation();

		if (FollowScale) CopyScale();
	}

	private void CopyScale( ) {
		if (!FollowerTransform.localScale.Equals (transform.localScale))
			FollowerTransform.localScale = Mathf.Approximately(ScaleSnap, 0f) ? transform.localScale
											: Vector3.Lerp(Follower.transform.localScale, transform.localScale, ScaleSnap * Time.deltaTime);
		//TODO: .lossyScale
	}

	private void SetGroupVisibility (bool value) {
		if (FollowerGroup == null) return;
		if (FollowerGroup.Visible == value) return;
		FollowerGroup.Visible = value;
		Visible = value;
	}

	private void CopyPosition( ) {
		if (Local) {
			if (!FollowerTransform.localPosition.Equals(transform.localPosition))
				FollowerTransform.localPosition = transform.localPosition;
			return;
		}
		var fsBody = Follower.GetComponent<FSBodyComponent>();
		if (fsBody != null && fsBody.body != null)
			fsBody.body.Position = new FVector2(transform.position.x, transform.position.y);
		else
			CopyTransformPosition();
	}

	private void CopyTransformPosition( ) {
		if (FollowerTransform.position.Equals(transform.position)) return;
		Vector3 targetPositon = (FollowPositionX && FollowPositionY && FollowPositionZ)? 
								transform.position
		                        : new Vector3 (	FollowPositionX? transform.position.x : FollowerTransform.position.x,
												FollowPositionY? transform.position.y : FollowerTransform.position.y,
												FollowPositionZ? transform.position.z : FollowerTransform.position.z);
		FollowerTransform.position = Mathf.Approximately(PositionSnap, 0f)
										? targetPositon
										: Vector3.Lerp(FollowerTransform.position, targetPositon, PositionSnap * Time.deltaTime);
	}

	private void CopyRotation( ) {
		if (Local) {
			if (!FollowerTransform.localRotation.Equals(transform.localRotation))
				FollowerTransform.localRotation = transform.localRotation;
			return;
		}
		var fsBody = Follower.GetComponent<FSBodyComponent>();
		if (fsBody != null && fsBody.body != null)
			fsBody.body.Rotation = transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
		else
			if (!FollowerTransform.rotation.Equals(transform.rotation))
				FollowerTransform.rotation = Mathf.Approximately(RotationSnap, 0f) ? transform.rotation 
					: Quaternion.Slerp (FollowerTransform.rotation, transform.rotation, RotationSnap*Time.deltaTime);
	}

}
