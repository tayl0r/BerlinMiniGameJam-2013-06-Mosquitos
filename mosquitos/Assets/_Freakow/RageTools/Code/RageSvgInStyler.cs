using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public partial class RageSvgIn : MonoBehaviour {

	private void ParseStyle(ref RageSvgObject pathData) {
		if (DebugStyleCreation)
			Debug.Log("svgStyle String: " + pathData.StyleString);
		ParseRageStyle(ref pathData);

		pathData.StyleString = "";
	}

	private void ApplyStyle (RageSvgObject pathData) {
		//Debug.Log ("path count: "+pathData.PathIdx+1);
		//ApplyRageStyle(pathData.CurrentPath, pathData.Style);
		foreach (RageSvgPathElement path in pathData.Paths) {
			//Debug.Log ("Apply style to: "+ (path == null ? "null" : path.gO.name));
			ApplyRageStyle(path, pathData.Style);
		}
	}

	/// <summary> Parses the style string to the 'currentStyle' object that SVG-In uses </summary>
	/// <param name="styleString"> </param>
	/// <param name="style"> </param>
	/// <param name="current"> </param>
	private void ParseRageStyle(ref RageSvgObject pathData) { //string styleString, ref RageSvgStyle style) {
		var style = pathData.Style;
		var styleString = pathData.StyleString;
		// Parse object example:
		// "fill:none;stroke:#FF0000;stroke-width:1px;stroke-linecap:butt;stroke-linejoin:miter;stroke-opacity:1"
		if (string.IsNullOrEmpty(styleString)) return;
		foreach (var styleEntry in GetSvgStyleCommands(styleString)) {
			switch (styleEntry.Command) {
				case ("stroke-miterlimit"):
					if (styleEntry.Value != "0")
						style.CornersType = Spline.CornerType.Beak;
					break;
				case ("stroke-linejoin"):
					style.CornersType = styleEntry.Value == "miter" ? Spline.CornerType.Beak : Spline.CornerType.Default;
					break;
				case ("stroke-linecap"):
					// 	if (style.Value == "round") currentStyle.Corners = RageSpline.Corner.Default;
					break;
				case ("opacity"):
					style.FillColor1Alpha = styleEntry.Value.SvgToFloat();
					style.OutlineAlpha = styleEntry.Value.SvgToFloat();
					break;
				case "fill":
					if (styleEntry.Value == "none" || styleEntry.Value == "transparent") {
						style.HasFill = false;
						break;
					}
					style.HasFill = true;											// default for all color parameter types

					if (styleEntry.Value[0] == '#') {
						style.FillColor1 = styleEntry.Value.Substring(1).ToColor();
						break;
					}
					if (styleEntry.Value.StartsWith("url")) {						// eg.: url(#SVGID_1_)
						var length = styleEntry.Value.Length - 6; //-5 + (-1)
						var gradientId = styleEntry.Value.Substring(5, length);		// Debug.Log("gradient Id to Search: " + gradientId);
						RageSvgGradient gradient;
						if (_gradients.TryGetValue(gradientId, out gradient)) {		// Note: For dictionaries, TryGetValue is much faster than GetKey
							style.HasGradient = true;								// style.FillType = Spline.FillType.Gradient;
							style.RageSvgGradient = gradient;
						}
						break;
					}
					style.FillColor1 = styleEntry.Value.KeywordToHex().ToColor();  	// Tries parsing a named color
					break;
				case "stroke":
					if (styleEntry.Value == "none" || styleEntry.Value == "transparent") {
						style.HasOutline = false;									// current.Style.OutlineType = Spline.OutlineType.None;
						break;
					}
					style.OutlineColor1 = styleEntry.Value[0] == '#' ?
														styleEntry.Value.Substring(1).ToColor()
													  : styleEntry.Value.KeywordToHex().ToColor();
					style.HasOutline = true;
					break;
				case "stroke-width":												// if there's a measurement unit, remove the trailing alphabetic characters (eg.: px, en)
					var width = styleEntry.Value.TrimTrailingAlphas();
					style.OutlineWidth = width.SvgToFloat();
					if (DebugMeshCreation)
						Debug.Log("stroke width ===> " + width);
					break;
				case "stroke-opacity":
					style.OutlineAlpha = styleEntry.Value.SvgToFloat();				// current.Style.HasOutline = !Mathfx.Approximately(current.Style.OutlineWidth,0f);
					break;
				case "fill-opacity":
					style.FillColor1Alpha = styleEntry.Value.SvgToFloat();
					break;
			}
		}
		if (DebugStyleCreation) 
			Debug.Log ("### After Parse Style ###" +
					   "\n>>> Current Fill: " + style.FillType + " :: Color: " + style.FillColor1 + " :: Has Fill? " + style.HasFill);
	}

	/// <summary> Parses the RageStyle and applies the Vertex Density during import  </summary>
	/// <param name="spline"> </param>
	private void ApplyRageStyle(RageSvgPathElement path, RageSvgStyle style) {
		if (style.HasOutline)
			style.OutlineType = path.IsClosed ? Spline.OutlineType.Loop : Spline.OutlineType.Free;
		else
			style.OutlineType = Spline.OutlineType.None;

		if (style.HasFill)
			style.FillType = style.HasGradient ? Spline.FillType.Gradient : Spline.FillType.Solid;
		else
			style.FillType = Spline.FillType.None;

		if (DebugStyleCreation) {
			Debug.Log("\tOutline: " + style.OutlineType + "; Has Outline: " + style.HasOutline);
			Debug.Log("\tFill: " + style.FillType + "; Has Fill: " + style.HasFill);
			Debug.Log(style.Debug());
		}
		if (path.Spline == null) return;

		ApplyStyleToSpline(path, style);
		// Parse single point, according to: https://bugzilla.mozilla.org/show_bug.cgi?id=322976
		if (path.Spline.PointsCount == 1)
			CreateDotCircle(ref path, style);					// Has to create a circle even without a "z" command to prevent RageSpline errors

		ApplyVertexDensity(ref path);
		path.Spline.Rs.RefreshMeshInEditor(true, true, true);
	}

	/// <summary> Applies a given style to a RageSpline  </summary>
	private void ApplyStyleToSpline(RageSvgPathElement path, RageSvgStyle style) {
		path.Spline.FillType = style.FillType;
		path.Spline.Outline.Type = style.OutlineType;
		path.Spline.Outline.Width = style.OutlineWidth;
		path.Spline.Outline.CornerType = style.CornersType;

		Color cFillColor1 = style.FillColor1;
		cFillColor1.a = style.FillColor1Alpha;

		if (style.FillType == Spline.FillType.Gradient)
			ApplyFillGradient(path, style);
		else
			path.Spline.FillColor = cFillColor1;

		Color cStrokeColor = style.OutlineColor1;
		cStrokeColor.a = style.OutlineAlpha;
		path.Spline.Outline.StartColor = cStrokeColor;
	}

	private void ParseGradient(XmlNode svgEntry, RageSvgObject pathData, RageSvgGradient.GradientType gradientType, bool subPath, int level) {
		// Initializes a new Gradient
		pathData.Gradient = RageSvgGradient.NewInstance();
		pathData.Gradient.Type = gradientType;
		float radius = 0f;
		var gradientTransformString = "";

		XmlAttributeCollection attributes = svgEntry.Attributes;
		if (attributes != null)
			foreach (XmlAttribute svgAttribute in attributes) {
				string svgCommand = svgAttribute.Name;
				string svgValue = svgAttribute.Value;
				// Parse valid Svg style commands, if any found, iterate
				// if (ParseSvgStyle(svgCommand, svgValue)) continue;

				switch (svgCommand) {
					//<linearGradient id="SVGID_1_" gradientUnits="userSpaceOnUse" x1="170.0791" y1="85.0396" x2="170.0791" y2="28.3472">
					case ("id"):
						pathData.Gradient.Id = svgValue;
						break;
					case ("xlink:href"):
						ParseGradientLink(ref pathData.Gradient, svgValue.Substring(1));
						break;
					case ("x1"):
						pathData.Gradient.X1 = svgValue.SvgToFloat();
						break;
					case ("y1"):
						pathData.Gradient.Y1 = svgValue.SvgToFloat();
						break;
					case ("x2"):
						pathData.Gradient.X2 = svgValue.SvgToFloat();
						break;
					case ("y2"):
						pathData.Gradient.Y2 = svgValue.SvgToFloat();
						break;
					case ("cx"):
						pathData.Gradient.X2 = svgValue.SvgToFloat();
						break;
					case ("cy"):
						pathData.Gradient.Y2 = svgValue.SvgToFloat();
						break;
					case ("fx"):
						pathData.Gradient.X1 = svgValue.SvgToFloat();
						break;
					case ("fy"):
						pathData.Gradient.Y1 = svgValue.SvgToFloat();
						break;
					case ("r"):
						radius = svgValue.SvgToFloat();
						break;
					case ("gradientTransform"):
						gradientTransformString = svgValue;
						break;
				}
			}

		if (!(Mathfx.Approximately(radius, 0f))) {
			pathData.Gradient.X2 = pathData.Gradient.X1 + radius;
			pathData.Gradient.Y2 = pathData.Gradient.Y2 + radius;
		}

		if (DebugStyleCreation)
			Debug.Log("######### Gradient children Count: " + svgEntry.ChildNodes.Count);

		if (svgEntry.ChildNodes.Count != 0) {
			level++;
			foreach (XmlNode childNode in svgEntry.ChildNodes)
				ParseSvg(childNode, pathData, subPath, level);
		}
		ApplyGradientTransform(gradientTransformString, pathData);

		if (_gradients.ContainsKey(pathData.Gradient.Id))
			return;

		if (DebugStyleCreation)
			Debug.Log("Gradient entry added: " + pathData.Gradient.Id);
		// After all "stop" child nodes are parsed, actually add the new dictionary item
		_gradients.Add(pathData.Gradient.Id, pathData.Gradient);
	}
	private void ParseGradientLink(ref RageSvgGradient currentGradient, string gradientKey) {

		RageSvgGradient refGradient;
		if (_gradients.TryGetValue(gradientKey, out refGradient)) {
			currentGradient.CopyDataFrom(refGradient);
		}
	}

	private void ParseGradientStop(XmlNode svgEntry, RageSvgObject pathData) {
		var offset = "";
		var stopColor = "";
		var stopOpacity = "";

		XmlAttributeCollection attributes = svgEntry.Attributes;
		if (attributes != null)
			foreach (XmlAttribute svgAttribute in attributes) {
				string svgCommand = svgAttribute.Name;
				string svgValue = svgAttribute.Value;

				if (DebugStyleCreation)
					Debug.Log("Gradient Stop: " + svgCommand + " " + svgValue);

				switch (svgCommand) {
					//<stop  offset="0" style="stop-color:#FFFFFF;stop-opacity:1"/>
					case ("offset"):
						offset = svgValue;
						break;
					case ("stop-color"):
						stopColor = svgValue.Substring(1);
						break;
					case ("stop-opacity"):
						stopOpacity = svgValue;
						break;
					case ("style"):
						var gradientStyleString = svgValue;

						foreach (var style in GetSvgStyleCommands(gradientStyleString)) {
							switch (style.Command) {
								case ("stop-color"):
									stopColor = style.Value.Substring(1);
									break;
								case ("stop-opacity"):
									stopOpacity = style.Value;
									break;
							}
						}
						break;
				}
			}
		var thisColor = stopColor.ToColor();
		if (stopOpacity != "")
			thisColor.a = float.Parse(stopOpacity);

		//SVG gradient stops are defined in reverse order.
		// TODO: Reduce the gradient radius according to the valid offsets 'delta'
		if (float.Parse(offset) < 0.5f)
			pathData.Gradient.EndColor = thisColor;
		else
			pathData.Gradient.StartColor = thisColor;
	}

	private bool ParseSvgStyle(string svgCommand, string svgValue, ref RageSvgObject pathData) {
		switch (svgCommand) {
			// TODO: Add style id to the class and store it
			//case ("id"):
			//    current.gO.name = svgValue;
			//    return true;
			case ("opacity"):
			case ("stroke-miterlimit"):
			case ("stroke-linejoin"):
			case ("stroke"):
			case ("stroke-opacity"):
			case ("stroke-width"):
			case ("stroke-linecap"):
			case ("fill"):
			case ("fill-opacity"):
				pathData.StyleString += svgCommand + ":" + svgValue + ";";
				return true;
			// style="fill:none;stroke:#FF0000;stroke-width:1px;stroke-linecap:butt;stroke-linejoin:miter;stroke-opacity:1"
			case ("style"):
				if (pathData.StyleString != "")
					if (pathData.StyleString[pathData.StyleString.Length - 1] != ';')
						pathData.StyleString += ";";
				pathData.StyleString += svgValue;
				return true;
		}
		return false;
	}

	private void ApplyGradientTransform(string gradientTransformString, RageSvgObject pathData) {
		if (gradientTransformString == "") {
			pathData.Gradient.Y1 *= -1;
			pathData.Gradient.Y2 *= -1;
			return;
		}
		/*Debug.Log(currentGradient.Id + "\nG1 orig pos: " + currentGradient.X1 + " " + currentGradient.Y1);*/

		var gradientStop = new Vector2(pathData.Gradient.X1, pathData.Gradient.Y1);
		gradientStop = ApplyTransform(gradientStop, gradientTransformString);
		pathData.Gradient.SetStartPos(gradientStop);
		/*Debug.Log("G1 final pos: " + currentGradient.X1 + " " + currentGradient.Y1);*/

		gradientStop = new Vector2(pathData.Gradient.X2, pathData.Gradient.Y2);
		gradientStop = ApplyTransform(gradientStop, gradientTransformString);
		pathData.Gradient.SetEndPos(gradientStop);
	}

	/// <summary> Sets the Gradient Fill parameters of the RageSpline </summary>
	/// <param name="spline"> </param>
	private void ApplyFillGradient(RageSvgPathElement path, RageSvgStyle style) {
		var gradient = style.RageSvgGradient;
		var start = new Vector2(gradient.X1, gradient.Y1);
		var end = new Vector2(gradient.X2, gradient.Y2);
		if (DebugStyleCreation)
			Debug.Log("\tGradient Start Color:" + gradient.StartColor);
		path.Spline.FillGradient.StartColor = gradient.StartColor;
		path.Spline.FillGradient.EndColor = gradient.EndColor;
		path.Spline.FillGradient.StyleLocalPositioning = true;
		path.Spline.FillGradient.Offset = (start + end) / 2;
		path.Spline.FillGradient.Scale = 1 / ((end - start) / 2).magnitude;
		//Debug.Log("current: "+current.Spline.name+" | start-end x/y : "+ start.x +" "+ start.y+" "+end.x+" "+end.y);
		var gradientAngle = ColorExtension.CalcTheta(new Vector2(gradient.X1, gradient.Y1),
											new Vector2(gradient.X2, gradient.Y2));

		if (DebugStyleCreation)
			Debug.Log("\tGradient Angle: " + gradientAngle);
		path.Spline.FillGradient.Angle = gradientAngle;
	}

	// style="fill:none;stroke:#FF0000;stroke-width:1px;stroke-linecap:butt;stroke-linejoin:miter;stroke-opacity:1"
	private static IEnumerable<RageSvgStyleEntry> GetSvgStyleCommands(string styleString) {
		if (styleString.Length == 0)
			yield break;

		var styleStringLength = styleString.Length;

		for (int i = 0; i < styleStringLength; i++) {
			var style = RageSvgStyleEntry.NewInstance();

			while (styleString[i] != ':') {
				style.Command += styleString[i];
				i++;
				if (i == styleStringLength)         //safe-check for malformed SVGs
					yield break;
			}
			i++; //skip the colon

			while (styleString[i] != ';') {
				style.Value += styleString[i];
				i++;
				if (i == styleStringLength) {
					yield return style;
					yield break;
				}
			}
			// Remove empty spaces from commands and values
			style.Command = style.Command.Trim();
			style.Value = style.Value.Trim();

			// new style entry found, iterate
			yield return style;
		}
	}

	/// <summary> ApplyTransform variation used for Gradient Transforms</summary>
	private Vector2 ApplyTransform(Vector2 target, string newTransformString) {
		if (string.IsNullOrEmpty(newTransformString)) return target;

		if (DebugMeshCreation)
			Debug.Log("Transformation String: " + newTransformString);
		var transformCommand = newTransformString.Split(new[] { ' ', ',', '(', ')', '\r', '\n' });
		transformCommand.RemoveEmptyEntries();

		for (var i = 0; i < transformCommand.Length; i++) {

			if (transformCommand[i] == "matrix") {
				var a = transformCommand[1].SvgToFloat(); // x Scale
				var b = transformCommand[2].SvgToFloat(); // x Skew
				var c = transformCommand[3].SvgToFloat(); // y Skew
				var d = transformCommand[4].SvgToFloat(); // y Scale
				var tx = transformCommand[5].SvgToFloat();
				var ty = transformCommand[6].SvgToFloat();
				target = new Vector2((target.x * a) + (target.y * c) + tx,
										(target.x * b) + (target.y * d) + ty);
			}
			if (transformCommand[i] == "translate") {
				if (DebugMeshCreation)
					Debug.Log("\tTransform translate: " + transformCommand[i + 1].SvgToFloat() +
							  "," + transformCommand[i + 2].SvgToFloat());
				var offset = new Vector2(transformCommand[i + 1].SvgToFloat(),
										 transformCommand[i + 2].SvgToFloat());

				target = new Vector2(target.x + offset.x,
									 target.y + offset.y);					// reflects on Y
				i = i + 2;
			}
		}
		return target;
	}
}
