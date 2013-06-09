using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RageGroup))]
[AddComponentMenu("RageTools/Rage Pivotools")]
public class RagePivotools: MonoBehaviour {

	public enum CenteringMode { Geometric = 0, Reference, PerItem, PerBranch };

	public CenteringMode CenteringType;
	public GameObject ReferenceGO;
	public bool InPlace = true;
	public bool DeletePivotReferences = true;
	public RageGroup Group;

	public void Awake() { FindRageGroup(); }

	//public void OnEnable() { FindRageGroup(); }

	public void CenterPivot() {
		switch (CenteringType) {
			case CenteringMode.Geometric:
				ProcessDefaultCentering();
				break;
			case CenteringMode.Reference:
				if (ReferenceGO == null)
					return;
				ProcessReferenceCentering();
				break;
			case CenteringMode.PerItem:
				ProcessPerItemCentering();
				break;
			case CenteringMode.PerBranch:
				ProcessPerBranchCentering();
				break;
		}
		// Recalculates the center to prevent problems with successive centering operations
		if (Group != null) Group.UpdateCenter();
	}

	public void ProcessDefaultCentering() {
		if (!FindRageGroup()) return;
		Vector3 origCenter = Group.Center;
		if (!InPlace) {
			transform.position = Vector3.zero;
			Group.UpdatePathList();			// Recalculates the group Center
		}
		var centerOffset = transform.position - Group.Center;
		PointsPositionOffset(Group, centerOffset);
		if (InPlace) {
			transform.position = origCenter;
			// Restore Children Positions
			foreach (var item in Group.List) {
				var thisPos = item.Spline.Rs.transform.position;
				var cachePos = item.PositionCache;
				item.Spline.Rs.transform.position = new Vector3(thisPos.x, thisPos.y, cachePos.z);
			}
		}
	}

	public void ProcessReferenceCentering() {
		if (!FindRageGroup()) return;
		if (InPlace) {
			var centerOffset = transform.position - ReferenceGO.transform.position;
			PointsPositionOffset(Group, centerOffset);
			transform.position = new Vector3(ReferenceGO.transform.position.x, ReferenceGO.transform.position.y, transform.position.z);
		} else {
			var centerOffset = ReferenceGO.transform.position - transform.position;
			PointsPositionOffset(Group, centerOffset);
		}
	}

	public void ProcessPerItemCentering() {
		if (!FindRageGroup()) return;
		foreach (var groupItem in Group.List) {
			var rageSpline = groupItem.Spline;
			var origCenter = GetSplineCenter(rageSpline);
			var origPos = rageSpline.GameObject.transform.position;
			PointsPositionOffset(groupItem, -origCenter);
			if (InPlace)
				rageSpline.GameObject.transform.position = origCenter + origPos;
		}
	}

	/// <summary> Processes every Root-level branch - ie. torso, left leg, etc </summary>
	public void ProcessPerBranchCentering() {
		//Just for Geometric Centering (Pivot not found within branch)
		foreach (Transform child in transform) {
			var branchPivot = SearchPivotInBranch(child);
			if (branchPivot == null)
				ApplyPivotools(child.gameObject, null);
		}
		var pivotTree = AssemblePivotTree (transform);
		pivotTree.Apply(DeletePivotReferences);
		Group.UpdatePathList();
	}

	private static GameObject SearchPivotInBranch(Transform rootItem) {
		GameObject pivotGO = null;
		var childSplines = rootItem.GetComponentsInChildren<RageSpline>();
		foreach (var childSpline in childSplines) {
			if (childSpline.gameObject.name.Contains("pivot")) {
				pivotGO = childSpline.gameObject;
				break;
			}
		}
		return pivotGO;
	}

	/// <summary> Apply Pivotools with Referencing or Geometric centering (referenceGO = null) </summary>
	private static void ApplyPivotools(GameObject pivotGO, GameObject referenceGo) {
		bool hadGroup = pivotGO.GetComponent<RageGroup>();
		bool hadPivotools = pivotGO.GetComponent<RagePivotools>();

		var group = pivotGO.GetComponent<RageGroup>() ?? pivotGO.AddComponent<RageGroup>();
		group.UpdatePathList();

		var pivotools = pivotGO.GetComponent<RagePivotools>() ?? pivotGO.AddComponent<RagePivotools>();
		if (referenceGo == null)
			pivotools.ProcessDefaultCentering();
		else {
			pivotools.ReferenceGO = referenceGo;
			pivotools.ProcessReferenceCentering();
		}

		if (!hadPivotools) pivotools.SmartDestroy();
		if (!hadGroup) group.SmartDestroy();
	}

	private Pivot AssemblePivotTree(Transform root) {
		var rootPivot = new Pivot(null, root);
		RecursiveSearchPivot(root, rootPivot);
		return rootPivot;
	}

	private void RecursiveSearchPivot(Transform root, Pivot parent) {
		//Debug.Log (root.gameObject.name);
		if (root.gameObject.name.Contains("pivot")) {
			var foundPivot = new Pivot(parent, root);
			parent.Children.Add(foundPivot);
			parent = foundPivot;
		}
		foreach (Transform item in root.transform) {
			RecursiveSearchPivot(item, parent);
		}
	}

	public class Pivot: IComparable<Pivot> {
		public Transform Transform;
		public Pivot Parent;
		public int Level;
		public List<Pivot> Children = new List<Pivot>();

		public Pivot (Pivot parent, Transform transform) { 
			Parent = parent;
			Level = CalcLevel (transform, 0);
			Transform = transform;
		}

		private int CalcLevel (Transform thisTransform, int level) {
			if (thisTransform.parent == null)
				return level;
			return CalcLevel (thisTransform.parent, ++level);
		}

		public void Apply(bool deleteReferences) {
			Children.Sort();

			foreach(Pivot pivot in Children) {
				ApplyPivotools(pivot.Transform.gameObject, null);
				ApplyPivotools(pivot.Transform.parent.gameObject, pivot.Transform.gameObject);
			}
			foreach (Pivot pivot in Children)
				pivot.Apply(deleteReferences);
			
			if (deleteReferences && Transform.gameObject.name.Contains("pivot"))
				Transform.gameObject.SmartDestroy();
		}

		public int CompareTo(Pivot other) {
			return Level.CompareTo(other.Level);
		}
	}

	/// <summary> Applies a given offset to the position of all points of a RageGroup</summary>
	private static void PointsPositionOffset(RageGroup group, Vector3 offset) {
		foreach (var groupItem in group.List)
			PointsPositionOffset(groupItem, offset);

		group.UpdatePathList();
	}

	/// <summary> Applies a given position offset to the points and the gradient of a given RageSpline</summary>
	private static void PointsPositionOffset(RageGroupElement groupItem, Vector3 offset) {
		var spline = groupItem.Spline;
		var hasGradient = (	spline.FillType == Spline.FillType.Gradient || 
							spline.Rs.GetOutlineGradient() != RageSpline.OutlineGradient.None);
		var hasTexture = (spline.Rs.GetTexturing1() != RageSpline.UVMapping.None ||
							spline.Rs.GetTexturing2() != RageSpline.UVMapping.None);

		Vector2 gradientOffsetWorld, textureOffsetWorld;
		Vector2 textureOffset2World = gradientOffsetWorld = textureOffsetWorld = Vector2.zero;
		if (hasGradient) gradientOffsetWorld = groupItem.GradientOffsetCache;
		if (hasTexture) {
			textureOffsetWorld = groupItem.TextureOffsetCache;
			textureOffset2World = groupItem.TextureOffsetCache2;
		}

		OffsetSplinePoints(spline, offset);

		if (hasGradient) {
			gradientOffsetWorld = new Vector2(gradientOffsetWorld.x + offset.x, gradientOffsetWorld.y + offset.y);
			spline.FillGradient.Offset = spline.GameObject.transform.InverseTransformPoint(gradientOffsetWorld);
		}
		if (hasTexture) {
			textureOffsetWorld = new Vector2 (textureOffsetWorld.x + offset.x, textureOffsetWorld.y + offset.y);
			textureOffset2World = new Vector2(textureOffset2World.x + offset.x, textureOffset2World.y + offset.y);
			spline.Rs.SetTextureOffset (spline.GameObject.transform.InverseTransformPoint (textureOffsetWorld));
			spline.Rs.SetTextureOffset2(spline.GameObject.transform.InverseTransformPoint(textureOffset2World));
		}
		spline.Rs.RefreshMeshInEditor(true, true, true);
	}

	/// <summary>Moves all points of a spline by a given offset </summary>
	private static void OffsetSplinePoints(ISpline rageSpline, Vector3 offset) {
		for (int i = 0; i < rageSpline.Rs.GetPointCount(); i++) {
			var pointPos = rageSpline.Rs.GetPositionWorldSpace(i);
			pointPos = new Vector3(
				pointPos.x + offset.x,
				pointPos.y + offset.y,
				pointPos.z + offset.z);
			rageSpline.Rs.SetPointWorldSpace(i, pointPos);
		}
	}

	/// <summary> Sets the rotation to zero and scale to one of all Game Objects in the hierarchy,
	/// while maintaining the points in the same position. </summary>
	public void FreezeRotationAndScale() {
		if (!FindRageGroup()) return;
		Group.UpdatePathList(); // Updates Point World Coords Cache
		ResetRotationAndScale(gameObject.transform);

		if (Group.List != null)
			foreach (var groupItem in Group.List) {
				var spline = groupItem.Spline;

				for (int i = 0; i < spline.PointsCount; i++) {
					ISplinePoint point = spline.GetPointAt(i);
					point.Position = groupItem.GroupPointCache[i].PointPos;
					point.InTangent = groupItem.GroupPointCache[i].InCtrlPos;
					point.OutTangent = groupItem.GroupPointCache[i].OutCtrlPos;
				}

				if (spline.FillType == Spline.FillType.Gradient) {
					//scale
					var gradientScale = spline.FillGradient.Scale;
					var scaleOffset = 1 / groupItem.ScaleCache.x;
					spline.FillGradient.Scale = gradientScale * scaleOffset;

					//rotate
					spline.FillGradient.Angle += -groupItem.RotationCache.eulerAngles.z;

					//translate
					spline.FillGradient.Offset = spline.GameObject.transform.InverseTransformPoint(groupItem.GradientOffsetCache);
				}
				spline.Rs.RefreshMeshInEditor(true, true, true);
			}
	}

	/// <summary> Recursive Function to Reset the Rotation and Scale of all nested game objects </summary>
	public void ResetRotationAndScale(Transform thisTransform) {
		if (thisTransform == null)
			return;

		thisTransform.localScale = Vector3.one;
		thisTransform.rotation = Quaternion.identity;

		foreach (Transform childTransform in thisTransform) {
			childTransform.localRotation = Quaternion.identity;
			childTransform.localScale = Vector3.one;
			if (childTransform.gameObject.transform.childCount > 0) {
				ResetRotationAndScale(childTransform);
			}
		}
	}

	/// <summary> Finds the geometric center of a spline </summary>
	private static Vector3 GetSplineCenter(ISpline spline) {
		var meshFilter = spline.GameObject.GetComponent<MeshFilter>();
		return new Vector3(meshFilter.sharedMesh.bounds.center.x * spline.GameObject.transform.lossyScale.x,
							meshFilter.sharedMesh.bounds.center.y * spline.GameObject.transform.lossyScale.y,
							meshFilter.sharedMesh.bounds.center.z * spline.GameObject.transform.lossyScale.z);
	}

	/// <summary> Tries to find a group in the Game gO and updates its list if found</summary>
	public bool FindRageGroup() {
		if (Group == null) Group = GetComponent<RageGroup>();
		if (Group == null) {
			Debug.Log ("Error: No group found. Aborting.");
			return false;
		}
		Group.UpdatePathList();
		return true;
	}
}
