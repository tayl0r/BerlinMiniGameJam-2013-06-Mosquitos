using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("RageTools/Rage IK")]
[ExecuteInEditMode]
public class RageIk : MonoBehaviour {
		
	public bool On;

	public bool FlipBias;
	public bool AlwaysUpdate = true;

	public bool DebugRay = true;
	public bool TwoDmode = true;
	public bool EndOffset = true;

	private Vector3 _lastControllerPosition;
	private Quaternion _lastControllerRotation;
	private Vector3 _lastControllerScale;

	public RageIkChain Chain = new RageIkChain();
	public bool ShowJoints;
	public GameObject AutoSetupEndJoint;
	public bool JointLimiterLive;
	public float JointLimiterAngleDelta;

	public void Update() {
		if (!Application.isPlaying) return;
		UpdateActions();
	}

	public void OnDrawGizmos() {
		if (Application.isPlaying) return;
		UpdateActions();
	}

	private void UpdateActions( ) {
		if (!On) return;
		if (!InitChainCheck()) return;

		try {
			if (!AlwaysUpdate) {
				if (Chain.Target.transform.position.Equals (_lastControllerPosition)
				    && Chain.Target.transform.rotation.Equals (_lastControllerRotation))
					return;
			}
		} finally {
			_lastControllerPosition = Chain.Target.transform.position;
			_lastControllerRotation = Chain.Target.transform.rotation;
		}

		RageIkSolver.Solve (Chain);
	}

	private bool InitChainCheck() {
		if (Chain == null) return false;
		return Chain.Target != null;
	}

	public void AddTransformIfPossible(Transform newTransform) {
		if (Chain.Joints.Contains(newTransform)) return;
		Chain.Joints.Add(newTransform);
	}

	public void AutoSetup() {
		if (AutoSetupEndJoint == null) return;
		var joints = new List<Transform>();
		joints = AddIntermediateObjects(joints);
		if (EndOffset) joints = AddEndOffset(joints);
		var ikParent = GameObject.Find ("_IkControllers");
		if (ikParent == null) ikParent = new GameObject {name = "_IkControllers"};
		Chain = new RageIkChain(joints, ikParent.transform, TwoDmode);
		Chain.Init();
		Chain.Target = CreateController(ikParent, joints[joints.Count - 1].gameObject, "IkTarget_" + gameObject.name).transform;
		SetupJointLimiters (Chain);
	}

	/// <summary> Adds an extra object to the joints list, offset from the last joint the vector difference between the last two current joints </summary>
	private List<Transform> AddEndOffset(List<Transform> joints) {
		if (joints.Count < 2) return joints;
		var lastJoint = joints[joints.Count - 1]; var preLastJoint = joints[joints.Count - 2];

		Transform endOffset = lastJoint.FindChild("endOffset");
		if (endOffset == null) endOffset = (new GameObject()).transform;
		var offset = lastJoint.position - preLastJoint.position;
		endOffset.position = lastJoint.position + offset;
		endOffset.name = "endOffset";
		endOffset.parent = joints[joints.Count - 1];
		joints.Add(endOffset);
		return joints;
	}

	/// <summary> Sets up Joint Limiter components (if none found) and values for all joints</summary>
	private void SetupJointLimiters (RageIkChain chain) {
		int lastJoint = EndOffset ? (chain.Joints.Count - 1) : chain.Joints.Count;
		for (int i = 0; i < lastJoint; i++ ) {
			var thisJoint = chain.Joints[i];
			var jointLimiter = thisJoint.GetComponent<RageIkJointLimiter>();
			if (jointLimiter == null && !JointLimiterLive) continue;
			if (jointLimiter == null)
				jointLimiter = thisJoint.gameObject.AddComponent<RageIkJointLimiter>();
			jointLimiter.Live = JointLimiterLive;
			Transform childTransform = GetValidChild(chain.Joints[i], AutoSetupEndJoint);
			if (childTransform == null) childTransform = chain.Target;
			var deltaVector = childTransform.position - chain.Joints[i].position;
			jointLimiter.RestAngle = jointLimiter.GetRestDirectionAngle(deltaVector);
			jointLimiter.MinAngle = jointLimiter.MaxAngle = deltaVector;
			jointLimiter.MinLimiterAngle -= JointLimiterAngleDelta;
			jointLimiter.MaxLimiterAngle += JointLimiterAngleDelta;
		}
	}

	private List<Transform> AddIntermediateObjects (List<Transform> joints) {
		var rootObject = gameObject.transform;
		joints.Add(rootObject);
		joints = AddJointRecursive(joints, rootObject);
		return joints;
	}

	/// <summary> Finds the last child game object, or the first child rage magnet </summary>
	private List<Transform> AddJointRecursive(List<Transform> joints, Transform thisTransform) {
		var foundTransform = GetValidChild(thisTransform, AutoSetupEndJoint);
		if (foundTransform == null) return joints;
		joints.Add(foundTransform);
		if (foundTransform != AutoSetupEndJoint.transform)
			AddJointRecursive(joints, foundTransform);
		return joints;
	}

	/// <summary> Among the immediate children, returns the end joint, a magnet or the last child found, in this order of priority </summary>
	private static Transform GetValidChild (Transform thisTransform, GameObject autoSetupEndJoint) {
		Transform lastTransform = null, foundEndJoint = null, foundMagnet = null;
		foreach (Transform child in thisTransform) {
			if (child.name == "InnerRadius") continue;
			lastTransform = child.gameObject.transform;
			if (lastTransform == autoSetupEndJoint.transform) foundEndJoint = lastTransform;
			if (lastTransform.GetComponent<RageMagnet>()) foundMagnet = lastTransform;
		}
		if (foundEndJoint != null) return foundEndJoint;
		if (foundMagnet != null) return foundMagnet;
		return lastTransform;
	}

	/// <summary> Finds the last child game object, or the first child rage magnet </summary>
	private GameObject FindObject(Transform rootTransform) {
		GameObject foundObject = null;
		foreach (Transform child in rootTransform) {
			foundObject = child.gameObject;
			if (foundObject.GetComponent<RageMagnet>())
				break;
		}
		return foundObject;
	}

	/// <summary> Creates a controller (or find it, if already exists one with same name) and parent it to a given game object</summary>
	/// <param name="parent">Game gO to parent this controller to</param>
	/// <param name="reference">Game gO to take the position from</param>
	/// <param name="controllerName">Name to give to the controller</param>
	private GameObject CreateController(GameObject parent, GameObject reference, string controllerName) {
		GameObject ikController = null;
		foreach (Transform child in parent.transform) {
			if (child.name == controllerName) {
				ikController = child.gameObject;
				break;
			}
		}
		if (ikController == null) ikController = new GameObject();
		ikController.transform.position = reference.transform.position;
		ikController.transform.rotation = reference.transform.rotation;
		ikController.transform.parent = parent.transform;
		ikController.name = controllerName;
		var ikHandle = ikController.GetComponent<RageIkHandle>() ?? ikController.AddComponent<RageIkHandle>();
		ikHandle.RageIk = this;
		return ikController;
	}

	public static float FullAngle(Vector3 origin, Vector3 destiny) {
		float angle = Vector3.Angle(origin, destiny);
		if (origin.y * destiny.x > origin.x * destiny.y)
			angle = 360 - angle;
		return angle;
	}
}
