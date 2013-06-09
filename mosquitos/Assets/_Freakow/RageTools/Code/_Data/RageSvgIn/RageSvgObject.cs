using UnityEngine;
using System.Collections.Generic;

public class RageSvgObject : ScriptableObject {

	public string Id;
	public List<RageSvgPathElement> Paths;
	public int PathIdx { get {return Paths.Count-1;} }
	public Transform Parent;
	public int PointIdx; //TODO: move to svgpathelement
	public RageSvgPathElement CurrentPath {
		get {
			if (PathIdx < 0) 
				return Paths[0];
			return Paths[PathIdx];
		}
	}
	public Vector3 CursorPos;
	public int GroupId;
	public int PathId;
	public RageSvgStyle Style;
	public RageSvgGradient Gradient;
	public string StyleString;
	public string TransformString;

	private RageSvgObject() {
		Id = "";
		Paths = new List<RageSvgPathElement>();
		Parent = null;
		PointIdx = 0;
		CursorPos = Vector3.zero;
		GroupId = 0;
		PathId = 0;
		Style = RageSvgStyle.NewInstance();
		Gradient = null;
		StyleString = "";
	}

	public static RageSvgObject NewInstance() {
		return CreateInstance(typeof(RageSvgObject)) as RageSvgObject;
	}

	public void AddPath(RageSvgPathElement newPath) {
		Paths.Add (newPath);
	}

}
