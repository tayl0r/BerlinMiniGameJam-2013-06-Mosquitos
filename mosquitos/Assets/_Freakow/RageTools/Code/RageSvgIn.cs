using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
[AddComponentMenu("RageTools/Rage SVG In")]
public partial class RageSvgIn : MonoBehaviour {

	public char Separator = '/';
	public string Directory = "RageTools/Demo/SvgFiles";
	public Object SvgFile;

	public int MinVertexDensity = 3;
	public int MaxVertexDensity = 5;
	public float AntialiasWidth = 1f;
	public float ZsortOffset = -0.1f;
	public float MergeRadius = 0.01f;
	public bool ImportOnStart;
	public bool UseUrl;
	public bool DebugMeshCreation;
	public bool DebugStyleCreation;
	public bool MidlineControls = true;
	public bool OutlineBehindFill;
	public bool AutoLayering;
	public bool AutoLayeringGroup;
	public bool AutoLayeringMaterials;
	private bool AdaptiveDensity {
		get { return (MinVertexDensity != MaxVertexDensity); }
	}
	private enum Poly { Line = 0, Gon };
	private Dictionary<string, RageSvgGradient> _gradients;
	public bool CreateHoles = true;
	public string UrlPath = "";
	private string _debugMessage = "";
	public bool _showDebugMessage = false;
	public bool PerspectiveMode;

	public IEnumerator Start() {
		if (!Application.isPlaying) yield break;
		if(!ImportOnStart) yield break;
		ImportSvg();
	}

	/// <summary> Change the return line below if you need a different path for your project's SVG files.
	/// 'ds' is the directory separator, it's set to forward slash since we'll use a platform-independent www call
	/// </summary>
	public string AbsoluteSvgFilePath {
		get{
			if(SvgFile == null) return "";
			return AbsoluteDirectory + SvgFile.name + ".svg";
		}
	}

	public string AbsoluteDirectory {
		get {
			if(Directory == null) return "";
			return Application.dataPath + Separator + Directory + Separator;
		}
	}

	public string SvgFilePath {
		get {
			if(SvgFile == null) return "";
			return Directory + Separator + SvgFile.name + ".svg";
		}
	}

	public void FixPath() {
		if (SvgFile == null) return;
		Separator = Path.AltDirectorySeparatorChar;
#if UNITY_EDITOR
		var dirName = Path.GetDirectoryName (AssetDatabase.GetAssetPath (SvgFile));
#else
		string dirName = null;
#endif
		if (String.IsNullOrEmpty(dirName)) return;
		var delimiters = new[] { Separator };
		// Removes the initial 'Assets/' from the directory, if not at root level
		var folderParts = dirName.Split (delimiters, 2);
		dirName = (folderParts.Length > 1) ? folderParts[1] : "";
		Directory = dirName;

		//Directory = Directory.Replace ("\\".ToCharArray (0, 1)[0], Separator);
		//Directory = Directory.Replace ("/".ToCharArray (0, 1)[0], Separator);
		//if (!File.Exists(AbsoluteSvgFilePath)) return;
	}

	public void OnGUI() {
		if (!_showDebugMessage) return;
		GUI.Label (new Rect (10, 10, 450, 100), _debugMessage);
	}

	/// <summary> Overload used by the editor, applies the editor-defined path </summary>
	public void ImportSvg() {
		StartCoroutine (ImportSvg (GetPathFromEditor()));
	}

	/// <summary> Takes a path to the SVG to be imported, useful for button callbacks </summary>
	public IEnumerator ImportSvg(string path) {

		if (!UseUrl && SvgFile == null) {
			Debug.Log("RageTools: SVG File not set, canceling import.");
			yield break;
		}

		// Initializes the stuff we'll need
		var svgData = RageSvgObject.NewInstance();
		_gradients = new Dictionary<string, RageSvgGradient>();

		// The starting parent is the current one, ie. the root
		svgData.Parent = gameObject.transform;

		// Actual importing is started here
		yield return StartCoroutine(SvgLoad(svgData, path));

		// If it has a RageGroup attached, auto-update it after import
		var group = GetComponent<RageGroup>();
		if (group != null) group.UpdatePathList();

		if (PerspectiveMode) {
			var childSpline = GetComponentInChildren<RageSpline>();
			childSpline.AllRageSplinesTo3D();
		}
	}

	/// <summary> Loads the SVG file using a www call (to make it platform-agnostic) </summary>
	public IEnumerator SvgLoad(RageSvgObject svgData, string path) {

		Debug.Log("RageSvgIn importing: " + path);

		var xml = new WWW(path);
		yield return StartCoroutine (LoadXml (xml));

		XmlNode rootNode = null;
		bool urlError = false;
		try { rootNode = RageXmlParser.XmlToDOM(xml.text); } 
		catch {
			Debug.Log("RageSvgIn Error: URL couldn't be parsed.");
			urlError = true;
		} finally {
			if (!urlError) {
				FindAndParseSvg(rootNode, svgData);
				if (AutoLayering && AutoLayeringMaterials) {
					foreach (RageLayer layer in GetComponentsInChildren<RageLayer>())
						layer.SetMaterialRenderQueue();
					Debug.LogWarning("RageTools: Please disregard error messages above.");
				}
				Debug.Log("RageSvgIn file imported!");
			}
		}
	}

	public IEnumerator LoadXml(WWW xml) {
		while (!xml.isDone)
			yield return null;
	}

	public string GetPathFromEditor() {
		string path;

		if (UseUrl) {
			if (!UrlPath.Contains("/")) {
				if (Application.isEditor)
					path = "file://" + Application.dataPath + "/" + UrlPath;
				else
					path = Application.dataPath + "/" + UrlPath;
			} else
				path = UrlPath;
		} else
			path = "file://" + AbsoluteSvgFilePath;

		_debugMessage = path;
		return path;
	}

	private void FindAndParseSvg(XmlNode node, RageSvgObject svgData) {
		ParseSvg(node["svg"], svgData, subPath:false, level: 0);
		// DON'T REMOVE: Line above still in tests
// 		foreach(XmlNode n in node.ChildNodes) {
// 			if(n.Name.Trim() == "svg") {
// 				ParseSvg(n, ref svgData, false, 0);
// 				break;
// 			}
// 			foreach(XmlNode childNode in n.ChildNodes)
// 				FindAndParseSvg(childNode, svgData);
// 		}
	}

	/// <summary> SVG XML node parser. svgEntry can be: 'path', 'rect', 'line', etc </summary>
	/// <param name="svgNode">XML node to start parsing from</param>
	/// <param name="svgData"> Current SVG gO </param>
	/// <param name="subPath">Is this SVG node a compound element of a path?</param>
	/// <param name="level"> </param>
	/// <param name="groupStyle">If null, starts a fresh new style</param>
	private void ParseSvg(XmlNode svgNode, RageSvgObject svgData, bool subPath, int level) {
		if (svgNode == null) {
			Debug.Log("Error: SVG Node not found or invalid");
			return;
		}
		if (DebugMeshCreation){
			Debug.Log(svgNode.Name.Trim() + " parse start: \n=================");
			Debug.Log("(Parse SVG) Level " + level);
		}

		if(svgData.Style == null || level <= 1) svgData.Style = RageSvgStyle.NewInstance();

		switch(svgNode.Name.Trim()) {                       // Starts parsing the valid commands' attributes
			case ("g"):
				ParseGroup(svgNode, svgData, subPath, ref level);
				break;
			//case ("defs"):
			//    ParseChildNodes(svgNode);
			//    break;
			case ("linearGradient"):
				ParseGradient(svgNode, svgData, RageSvgGradient.GradientType.Linear, subPath, level);
				break;
			case ("radialGradient"):
				ParseGradient(svgNode, svgData, RageSvgGradient.GradientType.Radial, subPath, level);
				break;
			case ("stop"):
				ParseGradientStop(svgNode, svgData);
				break;
			case ("rect"):
				ParseRect(svgNode, ref svgData, subPath);
				break;
			case ("circle"):
				ParseCircle(svgNode, ref svgData, subPath);
				break;
			case ("ellipse"):
				ParseEllipse(svgNode, ref svgData, subPath);
				break;
			case ("line"):
				ParseLine(svgNode, ref svgData, subPath);
				break;
			case ("polyline"):
				ParsePolyLine(svgNode, ref svgData, Poly.Line, subPath);
				break;
			case ("polygon"):
				ParsePolyLine(svgNode, ref svgData, Poly.Gon, subPath);
				break;
			case ("path"):
				ParsePathPrepare(svgNode, ref svgData, subPath, false);
				break;
		}
		// if some style was found in the svgEntry declaration, apply it
		if(!String.IsNullOrEmpty(svgData.StyleString)) 
			ParseStyle(ref svgData);

		// Prevents double-parsing if it's a group or gradient node
		if (svgNode.Name.Trim() == "g" ||
			svgNode.Name.Trim() == "linearGradient" ||
			svgNode.Name.Trim() == "radialGradient" ||
			svgNode.Name.Trim() == "pattern" )
			return;

		// Recurses through child nodes
		if (svgNode.HasChildNodes) {
			level++;
			foreach(XmlNode svgEntry in svgNode.ChildNodes){
				//Debug.Log (svgEntry.Name.Trim() + "(child) level: "+level);
				ParseSvg(svgEntry, svgData, subPath, level);
			}
		}
	}

	private void ParsePathPrepare (XmlNode svgNode, ref RageSvgObject svgData, bool subPath, bool derivedPath) {
		svgData.Paths = new List<RageSvgPathElement>();
		ParsePath(svgNode, ref svgData, subPath, derivedPath);
	}

	/// <summary> SVG Path parsing main function. All RageSplines are created from Path instructions. </summary>
	/// <param name="svgEntry">SVG Text string</param>
	/// <param name="svgData"> </param>
	/// <param name="subPath"> </param>
	/// <param name="derivedPath"> Derives from a non-path command like rect, circle, etc </param>
	private void ParsePath(XmlNode svgEntry, ref RageSvgObject svgData, bool subPath, bool derivedPath) {
		CreateGameObject(svgData.Parent, ref svgData, false, subPath, derivedPath);
		if (!derivedPath) svgData.TransformString = "";

		if (svgEntry.Attributes != null)
			foreach (XmlAttribute svgAttribute in svgEntry.Attributes) {
				string svgCommand = svgAttribute.Name;
				string svgValue = svgAttribute.Value;
				if (ParseSvgStyle(svgCommand, svgValue, ref svgData)) {
					ParseRageStyle(ref svgData);
					continue;
				}
				switch (svgCommand) {
					case ("id"):
						svgData.CurrentPath.gO.name = svgValue;
						break;
					case ("transform"):
						svgData.TransformString = svgValue;
						break;
					// if it's a Draw entry, split and treat the commandsets
					case ("d"):
						ParseDrawCommand(ref svgData, svgAttribute, false);
						break;
				}
			}
		ApplyStyle(svgData);
	}

	private void ParseGroup(XmlNode svgEntry, RageSvgObject svgData, bool subPath, ref int level) {
		var origParent = svgData.Parent;                       // Accounts for Nested nodes
		var hideGroup = false;
		var groupTransformString = "";

		svgData.Parent = CreateGameObject(svgData.Parent, ref svgData, true, false, false);		// All contained nodes will be parented to this guy
		svgData.Id = "";
		var currentGroup = svgData.Parent.gameObject;
		//var groupStyle = svgData.Style ?? RageSvgStyle.NewInstance();							
		RageSvgStyle groupStyle = RageSvgStyle.NewInstance();
		if (level > 1) groupStyle.CopyDataFrom(svgData.Style);									// Stores the current Style to allow proper style nesting

		XmlAttributeCollection attributes = svgEntry.Attributes;
		if(attributes != null)
			foreach(XmlAttribute svgAttribute in attributes) {
				string svgCommand = svgAttribute.Name;
				string svgValue = svgAttribute.Value;
				if (ParseSvgStyle(svgCommand, svgValue, ref svgData)) continue;

				switch(svgCommand) {
					case ("id"):
						svgData.Id = svgValue;
						break;
					case ("display"):
						hideGroup = (svgValue == "none");
						break;
					case ("transform"):
						groupTransformString = svgValue;
						break;
				}
			}

		currentGroup.name = (svgData.Id == "") ? "Group" : svgData.Id;
		ParseRageStyle(ref svgData);
		groupStyle.CopyDataFrom(svgData.Style);

		if(DebugMeshCreation) 
			Debug.Log("######### Group children Count: " + svgEntry.ChildNodes.Count + " (level "+level+")");
		if(svgEntry.HasChildNodes) {
			level++;
			foreach(XmlNode childNode in svgEntry.ChildNodes) {
				svgData.Style.CopyDataFrom(groupStyle);             // Restores the group style (safe check)
				//svgData.Style.HasOutline = false;
				//svgData.Style = RageSvgStyle.NewInstance();
				ParseSvg(childNode, svgData, subPath, level);
			}
		}
		if(DebugMeshCreation) Debug.Log("\tGroup transform: " + groupTransformString + " :: Group = " + currentGroup.name);

		// Only Apply transformations after creating child objects
		ApplyTransform(currentGroup, groupTransformString);
		svgData.TransformString = "";

		if(hideGroup) {                                    // If needed, hide the Group and its descendants
			var transforms = currentGroup.GetComponentsInChildren<Transform>();
			foreach (Transform thisTransform in transforms)
#if UNITY_4_0
				thisTransform.gameObject.SetActive(false);
			currentGroup.SetActive(false);
#else
				thisTransform.gameObject.active = false;
			currentGroup.active = false;
#endif
		}
		svgData.Parent = origParent;                       // Restores the default parent to the one prior to this entry
	}

	private void ParseLine(XmlNode svgEntry, ref RageSvgObject svgData, bool subPath) {
		svgData.Paths[svgData.PathIdx].IsLinear = true;

		float x2, y1, y2;
		float x1 = x2 = y1 = y2 = 0f;
		string toPath;

		if(svgEntry.Attributes != null)
			foreach(XmlAttribute svgAttribute in svgEntry.Attributes) {
				string svgCommand = svgAttribute.Name;
				string svgValue = svgAttribute.Value;

				if(ParseSvgStyle(svgCommand, svgValue, ref svgData))
					continue;

				switch(svgCommand) {
					// <line x1="100" y1="300" x2="300" y2="100" stroke-width="5"/>
					case ("x1"):
						x1 = svgValue.SvgToFloat();
						break;
					case ("y1"):
						y1 = svgValue.SvgToFloat();
						break;
					case ("x2"):
						x2 = svgValue.SvgToFloat();
						break;
					case ("y2"):
						y2 = svgValue.SvgToFloat();
						break;
					case ("transform"):
						svgData.TransformString = svgValue;
						break;
					case ("id"):
						svgData.Id = svgValue;
						break;
				}
			}

		if(svgData.StyleString == "")
			toPath = "<path d=\"";
		else
			toPath = "<path style=\"" + svgData.StyleString + "\" d=\"";

		toPath += "M " + x1.ToString() + "," + y1.ToString() + " ";
		toPath += "L " + x2.ToString() + "," + y2.ToString() + "\"/>";

		if(DebugMeshCreation)
			Debug.Log("svgLineToPath: " + toPath);
		XmlNode node = RageXmlParser.XmlToDOM(toPath);
		svgData.Style.CornersType = Spline.CornerType.Beak;
		ParsePathPrepare(node.ChildNodes[0], ref svgData, subPath, true);
	}

	private void ParsePolyLine(XmlNode svgEntry, ref RageSvgObject svgData, Poly polyType, bool subPath) {

		svgData.Style.CornersType = Spline.CornerType.Beak;
		string points = "";
		string toPath;

		if(svgEntry.Attributes != null)
			foreach(XmlAttribute svgAttribute in svgEntry.Attributes) {
				string svgCommand = svgAttribute.Name;
				string svgValue = svgAttribute.Value;

				if(ParseSvgStyle(svgCommand, svgValue, ref svgData))
					continue;
				// <polygon fill="lime" points="850,75 958,137.5 958,262.5 850,325" />
				switch(svgCommand) {
					case ("points"):
						points = svgValue;
						break;
					case ("transform"):
						svgData.TransformString = svgValue;
						break;
					case ("id"):
						svgData.Id = svgValue;
						break;
				}
			}

		if(svgData.StyleString == "")
			toPath = "<path d=\"";
		else
			toPath = "<path style=\"" + svgData.StyleString + "\" d=\"";

		toPath += "M " + points;
		if(polyType == Poly.Line)
			toPath += "\"/>";
		else
			toPath += " z\"/>";

		if(DebugMeshCreation)
			Debug.Log("svgPolygon: " + toPath);

		XmlNode node = RageXmlParser.XmlToDOM(toPath);
		svgData.Style.CornersType = Spline.CornerType.Beak;
		ParsePathPrepare(node.ChildNodes[0], ref svgData, subPath, true);
		svgData.CurrentPath.IsLinear = true;
	}

	private void ParseRect(XmlNode svgEntry, ref RageSvgObject svgData, bool subPath) {
		// Initializes a new Style string
		//xxx if (!subPath) current.StyleString = "";
		//currentStyle.Outline = RageSpline.Outline.None;
		svgData.Style.CornersType = Spline.CornerType.Beak;

		//var thisParent = parent;
		float y, width, height;
		float x = y = width = height = 0f;
		string svgRectToPath;

		if(svgEntry.Attributes != null)
			foreach(XmlAttribute svgAttribute in svgEntry.Attributes) {
				string svgCommand = svgAttribute.Name;
				string svgValue = svgAttribute.Value;

				if(ParseSvgStyle(svgCommand, svgValue, ref svgData)) continue;

				switch(svgCommand) {
					// x="21.778" y="22.11" fill="none" stroke="#000000"  width="100" height="100"
					case ("x"):
						x = svgValue.SvgToFloat();
						break;
					case ("y"):
						y = svgValue.SvgToFloat();
						break;
					case ("width"):
						width = svgValue.SvgToFloat();
						break;
					case ("height"):
						height = svgValue.SvgToFloat();
						break;
					case ("transform"):
						svgData.TransformString = svgValue;
						break;
					case ("id"):
						svgData.Id = svgValue;
						break;
				}
			}

		if(Mathf.Approximately(width, 0f) || Mathf.Approximately(height, 0f))
			return;
		//  Eg.:     d="m 10,12.362183 c 0,0 11,18 10,10 -2,-2 10,10 10,10 l -20,0 z"
		if(svgData.StyleString == "")
			svgRectToPath = "<path d=\"";
		else
			svgRectToPath = "<path style=\"" + svgData.StyleString + "\" d=\"";
		//* perform an absolute moveto operation to location (x+rx,y), where x is the value of the �rect� element's �x� attribute converted to user space, rx is the effective value of the �rx� attribute converted to user space and y is the value of the �y� attribute converted to user space
		svgRectToPath += "M " + x.ToString() + "," + y.ToString() + " ";

		//* perform an absolute horizontal lineto operation to location (x+width-rx,y), where width is the �rect� element's �width� attribute converted to user space
		svgRectToPath += "L " + (x + width).ToString() + "," + y.ToString() + " ";

		//* perform an absolute elliptical arc operation to coordinate (x+width,y+ry), where the effective values for the �rx� and �ry� attributes on the �rect� element converted to user space are used as the rx and ry attributes on the elliptical arc command, respectively, the x-axis-rotation is set to zero, the large-arc-flag is set to zero, and the sweep-flag is set to one
		//* perform a absolute vertical lineto to location (x+width,y+height-ry), where height is the �rect� element's �height� attribute converted to user space
		svgRectToPath += "L " + (x + width).ToString() + "," + (y + height).ToString() + " ";

		//* perform an absolute elliptical arc operation to coordinate (x+width-rx,y+height)
		//* perform an absolute horizontal lineto to location (x+rx,y+height)
		svgRectToPath += "L " + x.ToString() + "," + (y + height).ToString() + " ";

		//* perform an absolute elliptical arc operation to coordinate (x,y+height-ry)
		//* perform an absolute absolute vertical lineto to location (x,y+ry)
		svgRectToPath += "L " + x.ToString() + "," + y.ToString() + " ";

		//* perform an absolute elliptical arc operation to coordinate (x+rx,y)
		svgRectToPath += " z\" />";

		if(DebugMeshCreation) Debug.Log("svgRectToPath: " + svgRectToPath);

		XmlNode node = RageXmlParser.XmlToDOM(svgRectToPath);
		svgData.Style.CornersType = Spline.CornerType.Beak;
		ParsePathPrepare(node.ChildNodes[0], ref svgData, subPath, true);
	}

	private void ParseCircle(XmlNode svgEntry, ref RageSvgObject svgData, bool subPath) {
		svgData.Style.CornersType = Spline.CornerType.Default;

		float cy, r;
		float cx = cy = r = 0f;
		string svgCircleToPath;

		if(svgEntry.Attributes != null)
			foreach(XmlAttribute svgAttribute in svgEntry.Attributes) {
				string svgCommand = svgAttribute.Name;
				string svgValue = svgAttribute.Value;

				if(ParseSvgStyle(svgCommand, svgValue, ref svgData))
					continue;

				switch(svgCommand) {
					// <circle fill="none" stroke="#000000" stroke-miterlimit="10" cx="100" cy="100" r="50"/>
					case ("cx"):
						cx = svgValue.SvgToFloat();
						break;
					case ("cy"):
						cy = svgValue.SvgToFloat();
						break;
					case ("r"):
						r = svgValue.SvgToFloat();
						break;
					case ("transform"):
						svgData.TransformString = svgValue;
						break;
					case ("id"):
						svgData.Id = svgValue;
						break;
				} // switch (svgCommand)
			}

		if(Mathf.Approximately(r, 0f))
			return;

		if(svgData.StyleString == "")
			svgCircleToPath = "<path d=\"";
		else
			svgCircleToPath = "<path style=\"" + svgData.StyleString + "\" d=\"";
		//tangent length: http://en.wikipedia.org/wiki/File:Circle_and_cubic_bezier.svg
		var tanlen = (float)(0.5522847498307934 * r);

		//* top point = (cx, cy+r) 	//eg(100, 50) 
		//        svgCircleToPath += "m " + cx.ToString() + "," + (cy+r).ToString();

		//* <bottom> point = (cx-tanlen, cy+r ,cx, cy+r) 	//eg(100, 50) 
		svgCircleToPath += " S " + (cx - tanlen).ToString() + "," + (cy + r).ToString() + " " + cx.ToString() + "," + (cy + r).ToString();

		//smooth curve syntax: Sx1,y1 x,y
		//right point = (cx+r, cy+tanlen, cx+r, cy) 	//eg(150, 100)
		svgCircleToPath += "S " + (cx + r).ToString() + "," + (cy + tanlen).ToString() + " " + (cx + r).ToString() + "," + cy.ToString();

		// <top> (reflected) point = (cx+tanlen, cy-r, cx, cy-r)	//eg(100, 150)
		svgCircleToPath += " S " + (cx + tanlen).ToString() + "," + (cy - r).ToString() + " " + cx.ToString() + "," + (cy - r).ToString();

		// left point = (cx-r, cy-tanlen, cx-r, cy) 	//eg(50, 100)
		svgCircleToPath += " S " + (cx - r).ToString() + "," + (cy - tanlen).ToString() + " " + (cx - r).ToString() + "," + cy.ToString();

		//* closing <bottom> point = (cx-tanlen, cy+r ,cx, cy+r) 	//eg(100, 50) 
		svgCircleToPath += " S " + (cx - tanlen).ToString() + "," + (cy + r).ToString() + " " + cx.ToString() + "," + (cy + r).ToString();

		svgCircleToPath += " z\" />";

		if(DebugMeshCreation)
			Debug.Log("svgCircleToPath: " + svgCircleToPath);

		XmlNode node = RageXmlParser.XmlToDOM(svgCircleToPath);
		svgData.Style.CornersType = Spline.CornerType.Beak;

		ParsePathPrepare(node.ChildNodes[0], ref svgData, subPath, true);
	}

	private void ParseEllipse(XmlNode svgEntry, ref RageSvgObject svgData, bool subPath) {
		// Initializes a new Style string
		//xxx if (!subPath) current.StyleString = "";
		svgData.Style.CornersType = Spline.CornerType.Default;

		float cy, rx, ry;
		float cx = cy = rx = ry = 0f;
		string svgEllipseToPath;

		if(svgEntry.Attributes != null)
			foreach(XmlAttribute svgAttribute in svgEntry.Attributes) {
				string svgCommand = svgAttribute.Name;
				string svgValue = svgAttribute.Value;

				if(ParseSvgStyle(svgCommand, svgValue, ref svgData))
					continue;

				switch(svgCommand) {
					//   eg.: <ellipse cx="40" cy="40" rx="30" ry="15" style="stroke:#006600; fill:#00cc00"/>
					case ("cx"):
						cx = svgValue.SvgToFloat();
						break;
					case ("cy"):
						cy = svgValue.SvgToFloat();
						break;
					case ("rx"):
						rx = svgValue.SvgToFloat();
						break;
					case ("ry"):
						ry = svgValue.SvgToFloat();
						break;
					case ("transform"):
						svgData.TransformString = svgValue;
						break;
					case ("id"):
						svgData.Id = svgValue;
						break;
				}
			}

		// Any zero-radius disables the rendering of the element        );
		if(Mathf.Approximately(rx, 0f) || Mathf.Approximately(ry, 0f))
			return;

		if(svgData.StyleString == "") svgEllipseToPath = "<path d=\"";
		else svgEllipseToPath = "<path style=\"" + svgData.StyleString + "\" d=\"";
		//tangent length: http://en.wikipedia.org/wiki/File:Circle_and_cubic_bezier.svg
		var tanLenX = (float)(0.5522847498307934 * rx);
		var tanLenY = (float)(0.5522847498307934 * ry);

		//* top point = (cx, cy+r) 	//eg(100, 50) 
		//        svgCircleToPath += "m " + cx.ToString() + "," + (cy+ry).ToString();

		//* <bottom> point = (cx-tanLenX, cy+ry ,cx, cy+ry) 	//eg(100, 50) 
		svgEllipseToPath += " S " + (cx - tanLenX).ToString() + "," + (cy + ry).ToString() + " " +
							cx.ToString() + "," + (cy + ry).ToString();
		//right point = (cx+rx, cy+tanLenY, cx+rx, cy) 	//eg(150, 100)
		svgEllipseToPath += "S " + (cx + rx).ToString() + "," + (cy + tanLenY).ToString() + " " +
							(cx + rx).ToString() + "," + cy.ToString();
		// <top> (reflected) point = (cx+tanLenX, cy-ry, cx, cy-ry)	//eg(100, 150)
		svgEllipseToPath += " S " + (cx + tanLenX).ToString() + "," + (cy - ry).ToString() + " " +
							cx.ToString() + "," + (cy - ry).ToString();
		// left point = (cx-rx, cy-tanLenY, cx-rx, cy) 	//eg(50, 100)
		svgEllipseToPath += " S " + (cx - rx).ToString() + "," + (cy - tanLenY).ToString() + " " +
							(cx - rx).ToString() + "," + cy.ToString();
		//* closing <bottom> point = (cx-tanlen, cy+ry ,cx, cy+ry) 	//eg(100, 50) 
		svgEllipseToPath += " S " + (cx - tanLenX).ToString() + "," + (cy + ry).ToString() + " " +
							cx.ToString() + "," + (cy + ry).ToString();

		svgEllipseToPath += " z\" />";

		if(DebugMeshCreation)
			Debug.Log("svgCircleToPath: " + svgEllipseToPath);

		XmlNode node = RageXmlParser.XmlToDOM(svgEllipseToPath);
		svgData.Style.CornersType = Spline.CornerType.Beak;
		ParsePathPrepare(node.ChildNodes[0], ref svgData, subPath, true);
	}

	private void ParseDrawCommand (ref RageSvgObject svgData, XmlAttribute svgAttribute, bool derivedPath) {
		bool finalized = false; // Tells if the path command was truncated or not
		bool firstPath = true;
		var path = svgData.CurrentPath;

		foreach (var commandSet in SplitCommands (svgAttribute.Value.TrimEnd (null))) {
			if (DebugMeshCreation) Debug.Log ("commandset: " + commandSet);
			var coords = new List<float> (ParseCoordinates (commandSet.Trim()));
			var command = commandSet[0];
			var isRelative = char.IsLower (command);

			switch (char.ToUpper(command)) {
					//      moveto (relative)
				case 'M':
					if (firstPath) {
						MoveTo (coords, ref svgData, isRelative, false, false);
						firstPath = false;
						break;
					}
					if (DebugMeshCreation) Debug.Log ("\t ### Starting Sub-Path");
					svgData.PointIdx = 0;
					if (!finalized) {
						FinalizePath(ref svgData, svgData.CurrentPath, keepTransform: true, subPath: true);
						finalized = true;
					}
					CreateGameObject(svgData.Parent, ref svgData, false, true, derivedPath);
					MoveTo (coords, ref svgData, isRelative, false, false);
					break;
					//      lineto (relative)
				case 'L':
					MoveTo (coords, ref svgData, isRelative, false, false);
					break;
				case 'H':
					MoveTo (coords, ref svgData, isRelative, false, true);
					break;
				case 'V':
					MoveTo (coords, ref svgData, isRelative, true, false);
					break;
				case 'C':
					CurveTo (coords, ref svgData, isRelative);
					break;
				case 'S':
					//      rageSpline.AddPoint(index, vPos, vOutCtrl)
					SmoothCurveTo (coords, ref svgData, isRelative);
					break;
					//      Close Path, generally at end (if followed by M, next subpath starts there)
				case 'Z':
					svgData.CurrentPath.IsClosed = true;
					FinalizePath (ref svgData, path, keepTransform: false, subPath: !firstPath);
					finalized = true;
					break;
			}
		}
		
		if (!finalized)
			FinalizePath(ref svgData, svgData.CurrentPath, keepTransform: false, subPath: !firstPath);
		if (CreateHoles) RageSvgInHoles.ProcessHoles(svgData);
	}

	private void FinalizePath(ref RageSvgObject svgData, RageSvgPathElement path, bool keepTransform, bool subPath) {
		if (path.Spline.PointsCount == 0) {
			if (DebugMeshCreation) Debug.Log ("Parsing Error: Path has no valid points");
			return;
		}
		if (subPath) svgData.Paths.Add (path);

		// Render outlines behind (not-compatible with emboss) or not
		path.Spline.OutlineBehindFill = OutlineBehindFill;
		bool hasMerged = path.Spline.Rs.MergeStartEndPoints(DebugMeshCreation);
		if (!path.IsClosed && hasMerged)
			path.IsClosed = true;
		MergePointsCheck(ref svgData);

		ApplyTransform(path.gO, svgData.TransformString);
		if (!keepTransform) svgData.TransformString = "";
	}

	private void MergePointsCheck (ref RageSvgObject svgData) {
		var currentPath = svgData.Paths[svgData.PathIdx];
		if (MergeRadius > 0f && currentPath.Spline.Rs.GetPointCount() <= 3) return;
		bool hasMerged = false;
		do {
			int pointCount = currentPath.Spline.Rs.GetPointCount();
			int i = 0; // Skips the start point (processed by MergeStartEndPoints)
			while (i < pointCount) {
				hasMerged = currentPath.Spline.Rs.RemoveOverlap(i - 1, i, MergeRadius, DebugMeshCreation);
				i++;
			}
		} while (hasMerged && currentPath.Spline.Rs.GetPointCount() > 3);
	}

	/// <summary> Responsible for actually creating the RageSpline-containing Game Objects </summary>
	/// <param name="parentTransform">This will be the parent of the new Game gO</param>
	/// <param name="svgData">SVG Path Data</param>
	/// <param name="isGroup">Is this new Game gO a Group?</param>
	/// <param name="subPath"> </param>
	/// <param name="derivedPath"> </param>
	/// <returns>The Transform of new Game gO</returns>
	private Transform CreateGameObject(Transform parentTransform, ref RageSvgObject svgData, bool isGroup, bool subPath, bool derivedPath) {
		var newElement = RageSvgPathElement.NewInstance();
		newElement.gO = new GameObject();
		// Actually parent the path's game object
		newElement.gO.transform.parent = parentTransform;

		if (isGroup) {
			newElement.Spline = null;
			if (AutoLayeringGroup) svgData.GroupId++;
			return newElement.gO.transform;
		}
		// Not a group - actual shape
		CreateShape(ref svgData, subPath, derivedPath, newElement);

		return newElement.gO.transform;
	}

	private void CreateShape(ref RageSvgObject svgData, bool subPath, bool derivedPath, RageSvgPathElement newElement) {
		string newName = "<Path" + svgData.PathId + ">";
		if (!derivedPath) newElement.gO.name = newName;
		else {
			if (svgData.Id != "") {
				newElement.gO.name = svgData.Id;
				svgData.Id = "";
			}
			else
				newElement.gO.name = newName;
		}

		svgData.AddPath(newElement);
		SetZordering(newElement.gO, ref svgData);

		//Initializes a new RageSpline gO
		newElement.Spline = Spline.Add(newElement.gO);

		newElement.Spline.RemoveAllPoints();
		newElement.Spline.Outline.AntialiasingWidth = AntialiasWidth;

		svgData.PointIdx = 0;
		// According to the SVG spec, the first move command is always absolute
		if (!subPath) svgData.CursorPos = new Vector3(0f, 0f);
	}

	private void SetZordering(GameObject gO, ref RageSvgObject svgData) {
		var currentPath = svgData.Paths[svgData.PathIdx];
		if (!AutoLayering) { 
			gO.transform.position = new Vector3	(currentPath.gO.transform.position.x,
												currentPath.gO.transform.position.y,
												NewZOffset(ref svgData));
			return;
		}
		var rageLayer = gO.AddComponent<RageLayer>();
		if (rageLayer == null) return;
		if (AutoLayeringGroup)
			rageLayer.Zorder = svgData.GroupId * Mathf.CeilToInt(ZsortOffset * -1);
		else
			rageLayer.Zorder = (int)NewZOffset(ref svgData);
		rageLayer.ForceRefresh = AutoLayeringMaterials;
	}

	/// <summary>Iterates a new Z Offset and updates the variable </summary>
	/// <param name="svgData"> </param>
	/// <returns>The new z offset value</returns>
	private float NewZOffset(ref RageSvgObject svgData) {
		var oldPathId = svgData.PathId;
		svgData.PathId++;
		if (!AutoLayering) return (oldPathId * ZsortOffset);
		return (oldPathId * Mathf.CeilToInt(ZsortOffset * -1));
	}

	/// <summary> Draws a circle to represent a single point, according to https://bugzilla.mozilla.org/show_bug.cgi?id=322976 </summary>
	/// <param name="path"> </param>
	/// <param name="style"> </param>
	/// <param name="spline"> </param>
	private void CreateDotCircle(ref RageSvgPathElement path, RageSvgStyle style) {
		// <circle fill="none" stroke="#000000" stroke-miterlimit="10" cx="100" cy="100" r="50"/>
		string svgCircle = "<circle ";
		//Stores the current Outline color to later apply it as a fill
		var originalOutlineColor = style.OutlineColor1;
		var originalIsClosed = style.HasOutline;
		var objectName = path.Spline.Rs.gameObject.name;

		svgCircle += "stroke=\"none\" ";
		Vector3 position = path.Spline.GetPointAt(0).Position;
		svgCircle += "cx=\"" + position.x + "\" ";
		svgCircle += "cy=\"" + (-1 * position.y) + "\" ";
		svgCircle += "r=\"" + path.Spline.Outline.GetWidth(0) + "\"/>";

		if(DebugMeshCreation)
			Debug.Log("svgDotCircle: " + path.Spline.Rs.gameObject.name + " " + svgCircle);
		// Destroy the initially created path container, gonna start fresh
		var parent = path.Spline.Rs.transform.parent;
		//path.Spline.Rs.gameObject.SmartDestroy();

		XmlNode node = RageXmlParser.XmlToDOM(svgCircle);
		style.CornersType = Spline.CornerType.Beak;
		var svgData = RageSvgObject.NewInstance();
		svgData.Style = style;
		ParseCircle(node.ChildNodes[0], ref svgData, false);
		svgData.CurrentPath.gO.transform.parent = parent;

		// Only fills the shape if it's closed (found "z")
		// TODO: single "l" command to the same position of a move also makes it a dot shape
		if(originalIsClosed) {
			path.Spline.FillType = Spline.FillType.Solid;
			path.Spline.FillColor = originalOutlineColor;
		}
		else
			path.Spline.FillType = Spline.FillType.None;

		path.Spline.Rs.gameObject.name = objectName;
	}

	private void ApplyVertexDensity(ref RageSvgPathElement path) {
		if (path.Spline.PointsCount < 2) return;		// Not supported by RageSpline
		if (path.IsLinear) {							// If it's a line or polygon
			OptimizeLinearShape(ref path);
			return;
		}
		int finalVertexDensity = MaxVertexDensity;

		// If AdaptiveDensity is on, compares the shape size with the viewport size
		if (AdaptiveDensity) {
			var area = path.Spline.Bounds.width * path.Spline.Bounds.height;
			float ratio = Mathf.Clamp(((area / Camera.mainCamera.GetCameraArea()) * 10), 0f, 1f);
			finalVertexDensity = Mathf.RoundToInt(Mathf.Lerp(MinVertexDensity, MaxVertexDensity, ratio));  // upper limit is user-set vertex density
// 			if(DebugStyleCreation) {
// 				Debug.Log("Area = " + area + "\tRatio = " + ratio + 
// 					"\nFinal Density for " + path.Spline.Rs.gameObject.name + " = " + finalVertexDensity);
// 			}
		}
		path.Spline.VertexDensity = (finalVertexDensity);
		//^-RagetoolsCommon extension method, equals to: rageSpline.SetVertexCount(rageSpline.GetPointCount() * finalVertexDensity);
	}

	/// <summary> Apply the Transform parameters and process Transform Matrices </summary>
	/// <param name="targetGO"></param>
	/// <param name="newTransformString"></param>
	private void ApplyTransform(GameObject targetGO, string newTransformString) {
		if(string.IsNullOrEmpty(newTransformString))
			return;

		if(DebugMeshCreation)
			Debug.Log("Transformation String: " + newTransformString);
		//<g transform="translate(-10,-20) scale(2) rotate(45) translate(5,10)">
		Regex r = new Regex(@",\s*", RegexOptions.IgnoreCase);
		newTransformString = r.Replace(newTransformString, " ");
		var transformCommand = newTransformString.Split(new[] { ' ', ',', '(', ')', '\r', '\n' }); 
		transformCommand.RemoveEmptyEntries();

		var posOffset = Vector3.zero;
		var rotOffset = Vector3.zero;
		float scaleFactor = 1f;
		for(var i = 0; i < transformCommand.Length; i++) {
			if(transformCommand[i] == "matrix") {
				ApplyTransformMatrix(transformCommand, targetGO);
				break;
			}
			if(transformCommand[i] == "translate") {
				if(DebugMeshCreation)
					Debug.Log("\tTransform translate: " + transformCommand[i + 1].SvgToFloat() +
							  "," + transformCommand[i + 2].SvgToFloat());
				posOffset += new Vector3(transformCommand[i + 1].SvgToFloat(),
										 transformCommand[i + 2].SvgToFloat(),
										 0);
				i = i + 2;
			}
			if(transformCommand[i] == "rotate") {
				if(DebugMeshCreation)
					Debug.Log("\tTransform Rotate: " + transformCommand[i + 1].SvgToFloat());
				rotOffset += new Vector3(0f, 0f, transformCommand[i + 1].SvgToFloat());
				i = i + 1;
			}
			if(transformCommand[i] == "scale") {
				if(DebugMeshCreation)
					Debug.Log("\tTransform scale: " + transformCommand[i + 1].SvgToFloat());
				scaleFactor *= transformCommand[i + 1].SvgToFloat();
				i = i + 1;
			}
		}

		targetGO.transform.eulerAngles -= rotOffset;
		targetGO.transform.localScale *= scaleFactor;
		var pos = targetGO.transform.position + posOffset;

		targetGO.transform.position = new Vector3(pos.x, -1*pos.y, pos.z); // Mirrors on Y
	}

	private void ApplyTransformMatrix(string[] transformCommand, GameObject targetGO) {
		string temp = "";
		if (DebugMeshCreation) {
			foreach (string str in transformCommand)
				temp += str + "|| ";
			Debug.Log(temp);
		}
		// Eg.: <g transform="matrix(0.240147,0.000000,0.000000,0.240147,650.7029,5.991577)">
		// transformCommand :: [1 2 3 4 5 6]
		//                     | a  b  tx |       | a c e |
		//          Inkscape = | c  d  ty | SVG = | b d f |
		//                     | 0  0  1  |       | 0 0 1 |
		var a = transformCommand[1].SvgToFloat(); // x Scale
		var b = transformCommand[2].SvgToFloat(); // x Skew
		var c = transformCommand[3].SvgToFloat(); // y Skew
		var d = transformCommand[4].SvgToFloat(); // y Scale
		var tx = transformCommand[5].SvgToFloat();
		var ty = transformCommand[6].SvgToFloat();

		var pos = targetGO.transform.position;
		var finalPos = new Vector2(	(pos.x * a) + (pos.y * c) + tx,
									(pos.x * b) + (pos.y * d) + ty);
		float xSign = Mathf.Sign(a);
		float ySign = Mathf.Sign(d);
		// from matrix(a,-b,b,a,c,d), where a and b are not both zero => rotation angle is atan2(b,a).
		var r = Mathf.Atan2 (c,a) * Mathf.Rad2Deg;
		targetGO.transform.eulerAngles = new Vector3(0f, 0f, xSign * ySign* r);

		// a,b,c,d,tx,ty => the scale is sx=sqrt(a^2+b^2) and sy=sqrt(c^2+d^2)
		// In SVG: a,c,b,d,e,f => the scale is sx=sqrt(a^2+c^2) and sy=sqrt(b^2+d^2)
		var sx = Mathf.Sqrt ((Mathf.Pow (a, 2)) + (Mathf.Pow (c, 2)));
		var sy = Mathf.Sqrt ((Mathf.Pow (b, 2)) + (Mathf.Pow (d, 2)));

		targetGO.transform.position = new Vector3(finalPos.x, finalPos.y, targetGO.transform.position.z);
		targetGO.transform.localScale = new Vector3((xSign*sx), (ySign*sy), 1);
	}

	private void MoveTo(List<float> coords, ref RageSvgObject svgData, bool isRelative, bool keepX, bool keepY) {
		int k = 0;

		while(k < coords.Count) {
			Vector3 newPos = Vector3.zero;

			// if it shouldn't keep the current X or Y (ie. H or V commands), parse the new coord
			if(keepX) {
				newPos.x = svgData.CursorPos.x;
				newPos.y = -coords[k]; // reflects on Y
				if(isRelative)
					newPos.y += svgData.CursorPos.y;
			} else {
				if(keepY) {
					newPos.x = coords[k];
					newPos.y = svgData.CursorPos.y;
					if(isRelative)
						newPos.x += svgData.CursorPos.x;
				} else {
					newPos.x = coords[k];
					newPos.y = -coords[k + 1]; // reflects on Y
					if(isRelative) {
						newPos.x += svgData.CursorPos.x;
						newPos.y += svgData.CursorPos.y;
					}
				}
			}

			if(DebugMeshCreation)
				Debug.Log("\tcurrent x/y: " + svgData.CursorPos.x + " " + svgData.CursorPos.y);

			AddSplinePoint(ref svgData, Vector3.zero, newPos);

			if(DebugMeshCreation)
				Debug.Log("Move To (" + (svgData.PointIdx) + ") X/Y: " + newPos.x + "," + newPos.y +
						  " InCtrl: " + svgData.Paths[svgData.PathIdx].Spline.Rs.GetInControlPositionWorldSpace(svgData.PointIdx));
// 			current.Spline.Rs.SetPoint(current.PointIdx, current.Spline.Rs.GetPosition(current.PointIdx),
// 										current.Spline.Rs.GetInControlPosition(current.PointIdx),
// 										current.Spline.Rs.GetOutControlPosition(current.PointIdx), false);
			//current.Spline.GetPointAt(current.PointIdx).Smooth = false;
			// Sets the in control to be 1/2 of the vector to the previous point
			if(MidlineControls)
				CreateHalfwayTangents(ref svgData);

			svgData.CursorPos = newPos;
			svgData.PointIdx++;

			// will parse the next coordinate if it's an H or V command, otherwise skips two coords
			k = (keepX || keepY) ? k + 1 : k + 2;
		}

	}

	private void CurveTo(List<float> coords, ref RageSvgObject svgData, bool isRelative) {
		int k = 0;
		// if (!_isPathStarted) _isPathStarted = true;

		while(k < coords.Count) {
			// Relative curves are somewhat tricky. Example:
			//c 0,0 11,18 10,10
			// The first Pair is the in-tangent of the previous point, absolute and reflected on Y.
			//
			// The Position (third Pair) is added to the previous point coords, also reflected on Y. 
			// Like so: (20,30) => (20+10, (30+10) = (30, 40)
			//
			// (Second Pair - Third Pair), reflected on Y, gives the out tangent point. 
			// Like so: (11-10, 18-10) = (1, -8)

			// Sets the previous point Out Tangent

			var currentPath = svgData.Paths[svgData.PathIdx];

			var point = currentPath.Spline.GetPointAt(svgData.PointIdx - 1);
			if(isRelative) point.OutTangentLocal = new Vector3(coords[k], -coords[k + 1]);
			else point.OutTangent =  new Vector3(coords[k], -coords[k + 1]);

			// Calculate new position coordinates
			var newPos = new Vector3(coords[k + 4], -coords[k + 5]);

			// Calculate in-control vector coordinates
			var inCtrl = new Vector3(coords[k + 2], -coords[k + 3]);
			inCtrl -= newPos;

			if(isRelative) newPos += svgData.CursorPos;

			AddSplinePoint(ref svgData, inCtrl, newPos);
			//newPoint.InTangentLocal = inCtrl;

			if(DebugMeshCreation)
				Debug.Log ("(c)index: " + (svgData.PointIdx - 1) + " Position: "
						   + newPos.x + "," + newPos.y + " Pos: " + newPos.x + "," + newPos.y); 
					//" InCtrl: " + newPoint.InTangentLocal.x + "," + newPoint.InTangentLocal.y);

			svgData.PointIdx++;
			svgData.CursorPos = newPos;
			k += 6;
		}
	}

	private ISplinePoint AddSplinePoint(ref RageSvgObject svgData, Vector3 inCtrl, Vector3 newPos) {
		var currentPath = svgData.Paths[svgData.PathIdx];
		return currentPath.Spline.AddPointLocal(svgData.PointIdx, newPos, inCtrl, Vector3.zero, 1.0f, false);
	}

	private void SmoothCurveTo(List<float> coords, ref RageSvgObject svgData, bool isRelative) {
		int k = 0;
		bool startingPoint = false;
		var currentPath = svgData.Paths[svgData.PathIdx];

		if(svgData.PointIdx > 0)
			currentPath.Spline.GetPointAt(svgData.PointIdx - 1).Smooth = true;
		else
			startingPoint = true;

		while(k < coords.Count) {
			//      Coords: (x2 y2 x y); (x2,y2) = out control point position, (x,y) point position

			// Calculate new position coordinates
			var newPos = new Vector3(coords[k + 2], -coords[k + 3]);

			if(isRelative)
				newPos += svgData.CursorPos;

			// Calculate in-control point coordinates
			var inCtrl = new Vector3(coords[k], -coords[k + 1]);
			var inCtrl2 = new Vector3(coords[k + 2], -coords[k + 3]);
			inCtrl -= inCtrl2;

			// We add the point and set its in-control position
			AddSplinePoint(ref svgData, inCtrl, newPos);

			svgData.PointIdx++;

			if(DebugMeshCreation)
				Debug.Log("(s)index: " + svgData.PointIdx +
					" Pos: " + newPos.x + "," + newPos.y +
					" InCtrl: " + inCtrl.x + "," + inCtrl.y
					);

			svgData.CursorPos = newPos;

			k += 4;
		}

		// This is just a safe check. A well-formed SVG file won't ever use this.
		if(startingPoint)
			currentPath.Spline.GetPointAt(svgData.PointIdx).Smooth = true;
	}

	private void OptimizeLinearShape(ref RageSvgPathElement path) {
		if(path.Spline.PointsCount == 2) {
			path.Spline.VertexDensity = 24;
			path.Spline.Outline.CornerType = Spline.CornerType.Default;
		}

		if(path.Spline.PointsCount == 4 && path.Spline.Outline.Type == Spline.OutlineType.Loop) 
			path.Spline.VertexDensity = 1;
	}

	private void CreateHalfwayTangents(ref RageSvgObject svgData) {
		if(svgData.PointIdx <= 0) return;
		var currentPath = svgData.Paths[svgData.PathIdx];

		var p0 = currentPath.Spline.GetPointAt(svgData.PointIdx - 1);
		var p1 = currentPath.Spline.GetPointAt(svgData.PointIdx);

		p0.OutTangent = new Vector3((p1.Position.x + p0.Position.x) / 2,
									(p1.Position.y + p0.Position.y) / 2,
									(p1.Position.z + p0.Position.z) / 2);
	}


	private static IEnumerable<string> SplitCommands(string path) {

		var commandStart = 0;

		for(var i = 0; i < path.Length; i++) {
			string command;
			if (path[i].IsPathCommand()) {
				command = path.Substring(commandStart, i - commandStart).Trim();
				commandStart = i;

				if(!string.IsNullOrEmpty(command))
					yield return command;

				if(path.Length == i + 1)
					yield return path[i]+"";
			} else if(path.Length == i + 1) {
				command = path.Substring(commandStart, i - commandStart + 1).Trim();

				if(!string.IsNullOrEmpty(command))
					yield return command;
			}
		}
	}

	private static IEnumerable<float> ParseCoordinates(string coords) {
		coords = coords.Remove(0, 1);
		// Replaces "-" by " -", to help parsing, unless it's an exponential index
		//([^e\s]|[\d])-(\d)
		Regex rx = new Regex(@"([\d])-(\d)", RegexOptions.IgnoreCase);
		coords = rx.Replace(coords, "$1 -$2");
		coords = rx.Replace(coords, "$1 -$2");

		var parts = coords.Split(new[] { ',', ' ', '\t', '\r', '\n' }).RemoveEmptyEntries();

		foreach(string t in parts) {
			yield return float.Parse(t.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
		}
	}
};
