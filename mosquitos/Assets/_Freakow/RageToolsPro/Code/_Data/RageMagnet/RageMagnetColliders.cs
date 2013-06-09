using UnityEngine;

public static class RageMagnetColliders {

	public static void FixCollidersCheck(this RageMagnet rm) {
		CheckColliderChange(rm);
		CalculateColliderAttributes(rm);
		CalculateInnerCollider(rm);
	}

	private static bool Contains(RageMagnet rm, Collider collider, ISpline spline, int index){
		Vector3 worldPos = spline.GetPointAt(index).Position;
		return Contains(rm, collider,  worldPos);
	}

	private static bool Contains(RageMagnet rm, Collider collider, Vector3 worldPointPos) {

		// ATTENTION: Collider radius and height is in local space! Bounds data is in world space.
		// To normalize, we'll convert everything-world to local space using InverseTransformPoint
		if(!collider.bounds.Contains(worldPointPos)) return false;
		Vector3 localPointPos = rm.transform.InverseTransformPoint(worldPointPos);
		Vector3 localColliderCenter = rm.transform.InverseTransformPoint(rm.ActiveCollider.bounds.center);

		if(rm.ActiveColliderIsSphere){
			var sphere = (SphereCollider) collider;
			return (localPointPos - localColliderCenter).magnitude <= sphere.radius;
		}

		var capsule = (CapsuleCollider) collider;
		//Debug.Log(" point local x: "+ localPointPos.x + " | local collider center x: "+ localColliderCenter.x + " | Capsule Radius: " + capsule.radius);

		if(Mathf.Abs(localPointPos.x - localColliderCenter.x) > capsule.radius) return false;

		float cylinderHalfHeight = (capsule.height / 2) - capsule.radius;
		if (cylinderHalfHeight <= 0)
			return (localPointPos - localColliderCenter).magnitude <= capsule.radius;

		float minY = localColliderCenter.y - cylinderHalfHeight;
		float maxY = localColliderCenter.y + cylinderHalfHeight;
		if (localPointPos.y >= minY && localPointPos.y <= maxY) return true;

		if (localPointPos.y > localColliderCenter.y)
			return Vector3.Distance(new Vector3(localColliderCenter.x, maxY, localColliderCenter.z), localPointPos) <= capsule.radius;

		return Vector3.Distance(new Vector3(localColliderCenter.x, minY, localColliderCenter.z), localPointPos) <= capsule.radius;
	}

	public static bool ActiveInnerColliderContains(this RageMagnet rm, ISpline spline, int index) {
		return Contains(rm, rm.ActiveInnerCollider, spline, index);
	}

	public static bool ActiveColliderContains(this RageMagnet rm, ISpline spline, int index) {
		return Contains(rm, rm.ActiveCollider, spline, index);
	}

	public static bool ActiveColliderContains(this RageMagnet rm, Vector3 worldPos) {
		return Contains(rm, rm.ActiveCollider, worldPos);
	}

	private static void CalculateColliderAttributes(RageMagnet rm) {
		if(!rm.ResetValues) return;
		rm.ResetValues = false;

		if(rm.ActiveCollider == null) return;
		CalculateInnerCollider(rm);
	}

	private static void CalculateInnerCollider(RageMagnet rm) {
		if(rm.ActiveCollider == null) return;
		rm.InnerCapsuleCollider.radius = rm.CapsuleCollider.radius * rm.InnerSize / 100;
		rm.InnerCapsuleCollider.direction = rm.CapsuleCollider.direction;
		rm.InnerCapsuleCollider.height = rm.CapsuleCollider.height - (2 * (rm.CapsuleCollider.radius - rm.InnerCapsuleCollider.radius));
		if (rm.InnerSphereCollider == null || rm.SphereCollider == null) return;
		rm.InnerSphereCollider.radius = rm.SphereCollider.radius * rm.InnerSize / 100;
	}

	private static void CheckColliderChange(RageMagnet rm) {
		if(rm.CapsuleCollider != null) {
			rm.ResetValues = true;
			rm.CapsuleCollider.enabled = (!rm.ActiveColliderIsSphere);
			rm.InnerCapsuleCollider.enabled = rm.CapsuleCollider.enabled;
// 			if(!rm.InnerCapsuleCollider.enabled) {
// 				rm.CapsuleCollider.radius = 0;
// 				rm.CapsuleCollider.height = 0;
// 				rm.InnerCapsuleCollider.radius = 0;
// 				rm.InnerCapsuleCollider.height = 0;
// 			}
		}

		if(rm.SphereCollider != null) {
			rm.ResetValues = true;
			rm.SphereCollider.enabled = (rm.ColliderType == RageMagnet.ColliderTypes.Sphere);
			rm.InnerSphereCollider.enabled = rm.SphereCollider.enabled;
// 			if(!rm.InnerSphereCollider.enabled) {
// 				rm.SphereCollider.radius = 0;
// 				rm.InnerSphereCollider.radius = 0;
// 			}
		}

		if(rm.SphereCollider != null) rm.InnerSphereCollider.center = rm.SphereCollider.center;
		if(rm.CapsuleCollider != null) rm.InnerCapsuleCollider.center = rm.CapsuleCollider.center;
		if(rm.ActiveCollider != null) return;

		AddColliders(rm.gameObject);
		rm.AddInnerColliders();
	}

	private static void AddColliders(GameObject parent) {
		var coll = (Collider)parent.AddComponent<SphereCollider>();
		coll.isTrigger = true;

		coll = parent.AddComponent<CapsuleCollider>();
		coll.isTrigger = true;

		var rb = parent.AddComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.useGravity = false;
	}

	public static void AddInnerColliders(this RageMagnet rm){ AddInnerColliders(rm, ref rm.InnerColliders); }
	public static void AddInnerColliders(this RageMagnet rm, ref GameObject innerColliders) {
		if(innerColliders != null) return;

		innerColliders = new GameObject { name = RageMagnet.InnerRadiusId };
		innerColliders.transform.position = rm.gameObject.transform.position;
		innerColliders.transform.rotation = rm.gameObject.transform.rotation;
		innerColliders.transform.parent = rm.gameObject.transform;

		AddColliders(innerColliders);
	}

	public static void CenterZ(RageMagnet rm, float z) {
		CenterZSphereCollider(rm, z);
		CenterZCapsuleCollider(rm, z);
	}

	private static void CenterZSphereCollider(RageMagnet rm, float z) {
		if (rm.SphereCollider == null) return;
		rm.SphereCollider.center = new Vector3(rm.SphereCollider.center.x, rm.SphereCollider.center.y, z);
		rm.InnerSphereCollider.center = rm.SphereCollider.center;
	}

	private static void CenterZCapsuleCollider(RageMagnet rm, float z) {
		if (rm.CapsuleCollider == null) return;
		rm.CapsuleCollider.center = new Vector3(rm.CapsuleCollider.center.x, rm.CapsuleCollider.center.y, z);
		rm.InnerCapsuleCollider.center = rm.CapsuleCollider.center;
	}
}
