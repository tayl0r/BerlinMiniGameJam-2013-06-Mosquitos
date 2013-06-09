using UnityEngine;

public static class RageIkSolver {

	const int MaxIterations = 20;
	const float Accuracy = 0.2f;

	private static bool IsTargetUnreachable(RageIkChain chain) {
		float rootToTargetDist = Vector3.Distance(chain.Joints[0].position, chain.Target.position);
		return (rootToTargetDist > chain.Length);
	}

	public static void Solve(RageIkChain chain) {
		if (chain.Joints.Count < 2) return;
		chain.Init();

		if (IsTargetUnreachable(chain)) {
			for (int i = 0; i < chain.Joints.Count - 1; i++) {
				Quaternion rotation = Quaternion.FromToRotation(chain.Joints[i + 1].position - chain.Joints[i].position,
					chain.Target.position - chain.Joints[i].position);
				AddRotation(chain.Joints[i], rotation, chain);

				var limiter = chain.Joints[i].GetComponent<RageIkJointLimiter>();
				if (limiter == null || !limiter.Live) continue;

				if (limiter.ValidVector(limiter.RestDirection)) continue;
				if (Vector3.Angle(limiter.RestDirection, limiter.MaxAngle) > Vector3.Angle(limiter.RestDirection, limiter.MinAngle)) {
					AddRotation(chain.Joints[i], Quaternion.FromToRotation(limiter.RestDirection, limiter.MinAngle), chain);
					continue;
				}
				AddRotation(chain.Joints[i], Quaternion.FromToRotation(limiter.RestDirection, limiter.MaxAngle), chain);
			}
			ChangeLastElementRotation(chain);
			return;
		}

		int tries = 0;

		Vector3 rootInitial = chain.Joints[0].position;

		float targetDelta = Vector3.Distance(chain.Joints[chain.Joints.Count - 1].position, chain.Target.position);

		var desiredPositions = new Vector3[chain.Joints.Count];
		for (int i = 0; i < chain.Joints.Count; i++) {
			desiredPositions[i] = chain.Joints[i].position;
		}

		while (targetDelta > Accuracy && tries < MaxIterations) {
			ForwardReachingPhase (chain, desiredPositions);
			BackwardReachingPhase (chain, desiredPositions, rootInitial);

			targetDelta = Vector3.Distance(desiredPositions[chain.Joints.Count - 1], chain.Target.position);
			tries++;
		}
	}

	private static void ForwardReachingPhase (RageIkChain chain, Vector3[] desiredPositions) {
		desiredPositions[chain.Joints.Count - 1] = chain.Target.position;
		
		for (int i = desiredPositions.Length - 2; i > 0; i--){
			float lambda = chain.SegmentLengths[i] / Vector3.Distance (desiredPositions[i + 1], desiredPositions[i]);
			desiredPositions[i] = (1 - lambda) * desiredPositions[i + 1] + lambda * desiredPositions[i];
		}
	}

	private static void BackwardReachingPhase (RageIkChain chain, Vector3[] desiredPositions, Vector3 rootInitial) {
		desiredPositions[0] = rootInitial;

		for (int i = 0; i < chain.Joints.Count - 1; i++) {
			float lambda = chain.SegmentLengths[i] / Vector3.Distance (desiredPositions[i + 1], desiredPositions[i]);
			desiredPositions[i + 1] = (1 - lambda) * desiredPositions[i] + lambda * desiredPositions[i + 1];

			Quaternion rotation = Quaternion.FromToRotation(chain.Joints[i + 1].position - chain.Joints[i].position,
															desiredPositions[i+1] - chain.Joints[i].position);
			AddRotation(chain.Joints[i], rotation, chain);

			var limiter = chain.Joints[i].GetComponent<RageIkJointLimiter>();
			if (limiter == null) continue;

			if (limiter.ValidVector(limiter.RestDirection)) continue;
			if (Vector3.Angle(limiter.RestDirection, limiter.MaxAngle) > Vector3.Angle(limiter.RestDirection, limiter.MinAngle)) {
				RotateToLimit(chain, desiredPositions, i, limiter.RestDirection, limiter.MinAngle);
				continue;
			}
			RotateToLimit(chain, desiredPositions, i, limiter.RestDirection, limiter.MaxAngle);
		}
		ChangeLastElementRotation(chain);
	}

	private static void RotateToLimit (RageIkChain chain, Vector3[] desiredPositions, int idx, Vector3 fromDirection, Vector3 toDirection) {
		var rotation = Quaternion.FromToRotation (fromDirection, toDirection);
		AddRotation (chain.Joints[idx], rotation, chain, false);
		desiredPositions[idx + 1] = chain.Joints[idx + 1].position;
	}

	private static void ChangeLastElementRotation(RageIkChain chain) {
		if (!chain.AlignEnd) return;

		Transform lastElement = (chain.Joints[chain.Joints.Count - 1 ].name == "endOffset") ? 
								chain.Joints[chain.Joints.Count - 2] : chain.Joints[chain.Joints.Count - 1];
		
		Quaternion lastElementRotation = chain.Target.rotation;

		SetRotation(lastElement, lastElementRotation, chain);

		var lastElementLimiter = lastElement.GetComponent<RageIkJointLimiter>();
		if (lastElementLimiter == null) return;

		if (lastElementLimiter.ValidVector(lastElementLimiter.RestDirection)) return;
		if (Vector3.Angle(lastElementLimiter.RestDirection, lastElementLimiter.MaxAngle) > 
			Vector3.Angle(lastElementLimiter.RestDirection, lastElementLimiter.MinAngle)) {
			AddRotation(lastElement, Quaternion.FromToRotation(lastElementLimiter.RestDirection, lastElementLimiter.MinAngle), chain, false);
			return;
		}

		AddRotation(lastElement, Quaternion.FromToRotation(lastElementLimiter.RestDirection, lastElementLimiter.MaxAngle), chain);
	}

	private static void AddRotation(Transform joint, Quaternion rotation, RageIkChain chain, bool smoothRotations = true) {
		SetRotation(joint, rotation * joint.rotation, chain, smoothRotations);
	}

	private static void SetRotation(Transform joint, Quaternion rotation, RageIkChain chain, bool smoothRotations = true) {
		if (chain.TwoDeeMode) {
			var smoothRotation = rotation;
			if (smoothRotations && chain.Snap < 1f) 
				smoothRotation = Quaternion.Slerp(joint.rotation, rotation, Time.deltaTime * chain.Snap);
			joint.eulerAngles = new Vector3(0, 0, smoothRotation.eulerAngles.z);
			return;
		}
		joint.rotation = rotation;
	}
}
