using UnityEngine;

public abstract class Spline {

	public enum PhysicsType { None = 0, Boxed, MeshCollider, OutlineMeshCollider };
	public enum FillType { None = 0, Solid, Gradient, Landscape };
	public enum OutlineType { None = 0, Loop, Free };
	public enum CornerType { Default = 0, Beak };

	public static ISpline[] GetSplinesInChildren(GameObject go) {
		RageSpline[] tmp = go.GetComponentsInChildren<RageSpline>();
		ISpline[] result = new ISpline[tmp.Length];
		for (int i = 0; i < tmp.Length; i++)
			result[i] = Adapt(tmp[i]);

		return result;
	}

	public static ISpline GetSpline(GameObject go){
		return Adapt(go.GetComponent<RageSpline>());
	}

	public static ISpline GetSplineInChildren(GameObject go) {
		return Adapt(go.GetComponentInChildren <RageSpline>());
	}

	public static ISpline Add(GameObject gameObject){
		return Adapt(gameObject.AddComponent<RageSpline>());
	}

	private static ISpline Adapt(RageSpline rageSpline) {
		if (rageSpline==null) return null;
		var adapter = (RageSplineAdapter)ScriptableObject.CreateInstance(typeof(RageSplineAdapter));
		adapter.SetSpline(rageSpline);
		return adapter;
	}
}
