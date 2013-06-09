using UnityEngine;
using UnityEditor;
// using System;
// using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(RageSpline))]
public class RageSplineEditor : Editor {
	// Since Unity 3.4 this is deprecated.
	public bool showMeshColliderWarning = false;
	public bool showLocalStyleOptions = false;
	public List<int> selectedControlPoints = new List<int>();
	private SerializedObject sRageSpline;
	//private SerializedProperty sStyle;
	private static bool displayOptions;
	[SerializeField]
	private static bool physicsOptions = false;
	[SerializeField]
	private int _currentVertexCount;
	[SerializeField]
	private int _currentVertexDensity;
	private bool _dragSegmentShortcutPressed;
	private int _dragSegmentNearestIdx;

	public void OnEnable() {
		sRageSpline = new SerializedObject(target);
		//sStyle = sRageSpline.FindProperty ("style");
		//sVertexDensity = sRageSpline.FindProperty("_vertexDensity");
	}

	public override void OnInspectorGUI() {
		sRageSpline.Update();
		RageSpline rageSpline = target as RageSpline;

		if (rageSpline.DestroyFarseerNow) {
			foreach (Transform child in rageSpline.transform)
				if (child.gameObject.name.StartsWith("convexShape")) DestroyImmediate(child.gameObject);
			bool found = false;
			foreach (Transform child in rageSpline.transform)
				if (child.gameObject.name.StartsWith("convexShape")) found = true;
			if (!found) rageSpline.DestroyFarseerNow = false;
		}

		int labelWidth = 160;

		EditorGUIUtility.LookLikeControls();

		EditorGUILayout.Separator();

		GuiX.Horizontal(() => {
			rageSpline.Visible = GUILayout.Toggle(rageSpline.Visible, "Visible", GUILayout.MinWidth(75f), GUILayout.ExpandWidth(false));
			float minwidth = 120;
			if (rageSpline.GetOptimize()) minwidth = 65;
			rageSpline.SetOptimize(GUILayout.Toggle(rageSpline.GetOptimize(), "Optimize", GUILayout.MinWidth(minwidth)));
			EditorGUIUtility.LookLikeControls(30f, 1f);
			if (rageSpline.GetOptimize()) {
				float optimizeAngle;
				optimizeAngle = EditorGUILayout.FloatField(new GUIContent("Ang", "Max deviation angle for optimization"), rageSpline.GetOptimizeAngle(), GUILayout.MaxWidth(50f), GUILayout.ExpandWidth(true));
				if (optimizeAngle < 0) optimizeAngle = 0;
				rageSpline.SetOptimizeAngle(optimizeAngle);
			}
			rageSpline.PerspectiveMode = GUILayout.Toggle(rageSpline.PerspectiveMode, new GUIContent("3D", "3D-compatible Mode"), GUILayout.ExpandWidth(false));
			if (rageSpline.CurrentPerspective != rageSpline.PerspectiveMode) {
				rageSpline.SwitchPerspectiveMode();
				EditorUtility.SetDirty(target);
				rageSpline.CurrentPerspective = rageSpline.PerspectiveMode;
				rageSpline.RefreshMeshInEditor(true, true, true);
			}
		});

		GuiX.Horizontal(() => {
			EditorGUIUtility.LookLikeControls(80f, 90f);
			rageSpline.SetOutline((RageSpline.Outline) EditorGUILayout.EnumPopup(" Outline:", rageSpline.GetOutline(), GUILayout.Width(160f)));
			if (rageSpline.GetOutline() != RageSpline.Outline.None) {
				rageSpline.SetOutlineColor1(EditorGUILayout.ColorField(rageSpline.GetOutlineColor1(), GUILayout.Width(40)));
				if (rageSpline.GetOutlineGradient() != RageSpline.OutlineGradient.None)
					rageSpline.SetOutlineColor2(EditorGUILayout.ColorField(rageSpline.GetOutlineColor2(), GUILayout.Width(40)));
			}
		});
		if (rageSpline.GetOutline() != RageSpline.Outline.None) {
			EditorGUIUtility.LookLikeInspector();
			rageSpline.SetOutlineWidth(EditorGUILayout.FloatField("      Outline width", rageSpline.GetOutlineWidth()));
			rageSpline.SetOutlineNormalOffset(EditorGUILayout.FloatField("      Outline offset", rageSpline.GetOutlineNormalOffset()));
			rageSpline.SetCorners((RageSpline.Corner) EditorGUILayout.EnumPopup("      Outline corners:", rageSpline.GetCorners()));
			if (rageSpline.GetTexturing1() == RageSpline.UVMapping.Outline || rageSpline.GetTexturing2() == RageSpline.UVMapping.Outline)
				rageSpline.SetOutlineTexturingScaleInv(EditorGUILayout.FloatField("      Texturing scale", rageSpline.GetOutlineTexturingScaleInv()));
			rageSpline.SetOutlineGradient((RageSpline.OutlineGradient) EditorGUILayout.EnumPopup("      Outline gradient:", rageSpline.GetOutlineGradient()));
			EditorGUILayout.Separator();
		}

		EditorGUIUtility.LookLikeControls();

		GuiX.Horizontal(() => {
			EditorGUIUtility.LookLikeControls(80f, 90f);
			rageSpline.SetFill((RageSpline.Fill) EditorGUILayout.EnumPopup(" Fill:", rageSpline.GetFill(), GUILayout.Width(labelWidth)));
			switch (rageSpline.fill) {
				case RageSpline.Fill.None:
					break;
				case RageSpline.Fill.Solid:
					rageSpline.SetFillColor1(EditorGUILayout.ColorField(rageSpline.GetFillColor1(), GUILayout.Width(40)));
					break;
				case RageSpline.Fill.Gradient:
					rageSpline.SetFillColor1(EditorGUILayout.ColorField(rageSpline.GetFillColor1(), GUILayout.Width(40)));
					rageSpline.SetFillColor2(EditorGUILayout.ColorField(rageSpline.GetFillColor2(), GUILayout.Width(40)));
					break;
				case RageSpline.Fill.Landscape:
					rageSpline.SetFillColor1(EditorGUILayout.ColorField(rageSpline.GetFillColor1(), GUILayout.Width(40)));
					rageSpline.SetFillColor2(EditorGUILayout.ColorField(rageSpline.GetFillColor2(), GUILayout.Width(40)));
					break;
			}
		});

		if (rageSpline.GetFill() == RageSpline.Fill.Landscape) {
			EditorGUIUtility.LookLikeInspector();
			rageSpline.SetLandscapeBottomDepth(EditorGUILayout.FloatField("      Landscape bottom depth", rageSpline.GetLandscapeBottomDepth()));
			rageSpline.SetLandscapeOutlineAlign(EditorGUILayout.FloatField("      Landscape Outline align", rageSpline.GetLandscapeOutlineAlign()));
			EditorGUILayout.Separator();
		}

		GuiX.Horizontal(() => {
			EditorGUIUtility.LookLikeControls(80f, 90f);
			rageSpline.SetEmboss((RageSpline.Emboss) EditorGUILayout.EnumPopup(" Emboss:", rageSpline.GetEmboss(), GUILayout.Width(labelWidth)));
			switch (rageSpline.GetEmboss()) {
				case RageSpline.Emboss.None:
					break;
				case RageSpline.Emboss.Sharp:
				case RageSpline.Emboss.Blurry:
					rageSpline.SetEmbossColor1(EditorGUILayout.ColorField(rageSpline.GetEmbossColor1(), GUILayout.Width(40)));
					rageSpline.SetEmbossColor2(EditorGUILayout.ColorField(rageSpline.GetEmbossColor2(), GUILayout.Width(40)));
					break;
			}
		});

		if (rageSpline.GetEmboss() != RageSpline.Emboss.None) {
			EditorGUIUtility.LookLikeInspector();
			rageSpline.SetEmbossOffset(EditorGUILayout.FloatField("      Emboss offset", rageSpline.GetEmbossOffset()));
			rageSpline.SetEmbossSmoothness(EditorGUILayout.FloatField("      Emboss smoothness", rageSpline.GetEmbossSmoothness()));
			EditorGUILayout.Separator();
		}

		EditorGUIUtility.LookLikeControls(80f, 90f);
		GuiX.Horizontal(() => rageSpline.SetTexturing1((RageSpline.UVMapping) EditorGUILayout.EnumPopup(" Texturing:", rageSpline.GetTexturing1(), GUILayout.Width(labelWidth))));
		GuiX.Horizontal(() => rageSpline.SetTexturing2((RageSpline.UVMapping) EditorGUILayout.EnumPopup(" Texturing2:", rageSpline.GetTexturing2(), GUILayout.Width(labelWidth))));

		GeneralSettings(rageSpline);

		PhysicsPopup(labelWidth, rageSpline, showMeshColliderWarning);

		GuiX.Horizontal(() => {
			EditorGUIUtility.LookLikeControls(80f, 90f);
			rageSpline.showCoordinates = ((RageSpline.ShowCoordinates) EditorGUILayout.EnumPopup(" Snapping:", rageSpline.showCoordinates, GUILayout.Width(labelWidth)));
		});

		EditorGUIUtility.LookLikeControls(170f, 1f);
		if ((rageSpline.showCoordinates != RageSpline.ShowCoordinates.None)) {
			rageSpline.showGrid = EditorGUILayout.Toggle("      Show grid", rageSpline.showGrid);
			if (rageSpline.showGrid) {
				rageSpline.gridColor = EditorGUILayout.ColorField("      Grid color", rageSpline.gridColor, GUILayout.MaxWidth(210f));
				rageSpline.gridExpansion = EditorGUILayout.IntField("      Grid expand", rageSpline.gridExpansion);
			}
			rageSpline.gridSize = Mathf.Clamp(EditorGUILayout.FloatField("      Grid size", rageSpline.gridSize), 0.01f, 100f);
			rageSpline.snapToGrid = EditorGUILayout.Toggle("      Snap points to grid", rageSpline.snapToGrid);
			rageSpline.snapHandlesToGrid = EditorGUILayout.Toggle("      Snap handles to grid", rageSpline.snapHandlesToGrid);
			rageSpline.showNumberInputs = EditorGUILayout.Toggle("      Coordinate inputs", rageSpline.showNumberInputs);
		}

		if (rageSpline.showCoordinates != RageSpline.ShowCoordinates.None && rageSpline.showNumberInputs)
			ShowCoordinatesData(rageSpline);

		EditorGUILayout.Separator();

		EditorGUIUtility.LookLikeControls(80f, 90f);
		GuiX.Horizontal(() => {
#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3
			rageSpline.style = EditorGUILayout.ObjectField(" Style", rageSpline.style, typeof(RageSplineStyle)) as RageSplineStyle;
#else
			rageSpline.style = EditorGUILayout.ObjectField(" Style", rageSpline.style, typeof(RageSplineStyle), false) as RageSplineStyle;
#endif
		});

		if (rageSpline.style == null) {
			GuiX.Horizontal(() => {
				rageSpline.defaultStyleName = EditorGUILayout.TextField("      Name:", rageSpline.defaultStyleName);
				if (GUILayout.Button("Create", GUILayout.Height(15f), GUILayout.Width(60f))) {
					var style = CreateInstance(typeof(RageSplineStyle)) as RageSplineStyle;
					style.GetStyleFromRageSpline(rageSpline);
					AssetDatabase.CreateAsset(style, "Assets/" + rageSpline.defaultStyleName + ".asset");
					//EditorUtility.SetDirty(style);
					//AssetDatabase.SaveAssets();
					rageSpline.SetStyle(style);
				}
			});
		} else {
			EditorGUIUtility.LookLikeInspector();
			showLocalStyleOptions = EditorGUILayout.Foldout(showLocalStyleOptions, "      Style options");
			if (showLocalStyleOptions) {
				rageSpline.styleLocalGradientPositioning = EditorGUILayout.Toggle("      Local gradient gizmos", rageSpline.styleLocalGradientPositioning);
				rageSpline.styleLocalTexturePositioning = EditorGUILayout.Toggle("      Local texture gizmos", rageSpline.styleLocalTexturePositioning);
				rageSpline.styleLocalEmbossPositioning = EditorGUILayout.Toggle("      Local emboss gizmos", rageSpline.styleLocalEmbossPositioning);
				rageSpline.styleLocalAntialiasing = EditorGUILayout.Toggle("      Local anti-aliasing", rageSpline.styleLocalAntialiasing);
				rageSpline.styleLocalVertexCount = EditorGUILayout.Toggle("      Local vertex count", rageSpline.styleLocalVertexCount);
				rageSpline.styleLocalPhysicsColliderCount = EditorGUILayout.Toggle("      Local physics collider count", rageSpline.styleLocalPhysicsColliderCount);
			}
		}

		//v1.5: Auto-Refresh
		EditorGUILayout.Separator();
		GuiX.Vertical(GUI.skin.box, () => {
			GuiX.Horizontal(() => {
				GUILayout.Label(" ", GUILayout.Width(7f));
				if (GUILayout.Button(new GUIContent("Refresh", "Re-builds polygon mesh from the vector data"), GUILayout.Width(70f)))
					rageSpline.RefreshMeshInEditor(true, true, true);
				EditorGUIUtility.LookLikeControls(110f, 20f);
				rageSpline.AutoRefresh =
					EditorGUILayout.Toggle(new GUIContent("       Auto Refresh", "Auto re-builds polygon mesh in a given time interval"), rageSpline.AutoRefresh);
			});
			if (rageSpline.AutoRefresh) {
				GuiX.Horizontal(() => {
					EditorGUIUtility.LookLikeControls(70f, 30f);
					rageSpline.AutoRefreshDelay = EditorGUILayout.FloatField("     Delay", rageSpline.AutoRefreshDelay);
					rageSpline.AutoRefreshDelayRandomize = EditorGUILayout.Toggle("Randomize", rageSpline.AutoRefreshDelayRandomize);
				});
			}
		});

		EditorGUILayout.Separator();

		sRageSpline.ApplyModifiedProperties();

		if (Event.current.type == EventType.ExecuteCommand) {
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
		}

		if (GUI.changed) EditorUtility.SetDirty(rageSpline);

		if (Event.current.type == EventType.ExecuteCommand || Event.current.type == EventType.KeyUp || Event.current.type == EventType.used) {
			var spMesh = target as RageSpline;
			if (rageSpline.style != null) {
				EditorUtility.SetDirty(rageSpline.style);
			}
			spMesh.RefreshMeshInEditor(true, true, true);
		}
	}

	private void GeneralSettings(RageSpline rageSpline) {
		EditorGUIUtility.LookLikeControls(170f, 10f);
		GuiX.Vertical(GUI.skin.box, () => {
			GuiX.Horizontal(() => EditorGUILayout.LabelField("General:", ""));
			int vCount = 0, vDensity = 0;

			GuiX.Horizontal(() => {
				EditorGUIUtility.LookLikeControls(120f, 0f);
				vDensity = EditorGUILayout.IntField("  Vertex:  Density", rageSpline.VertexDensity, GUILayout.Width(152f), GUILayout.ExpandWidth(false));
				if (vDensity < 1) vDensity = 1;
				if (_currentVertexDensity != vDensity) {
					_currentVertexDensity = vDensity;
					rageSpline.VertexDensity = vDensity;
				}
#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#else
				GUILayout.Space(-5f);
#endif
				EditorGUIUtility.LookLikeControls(52f, 0f);
				if (!rageSpline.optimize)
					vCount = EditorGUILayout.IntField("Count", !rageSpline.lowQualityRender
															? rageSpline.GetVertexCount()
															: rageSpline.vertexCount);
				else {
					vCount = !rageSpline.lowQualityRender ? rageSpline.GetVertexCount() : rageSpline.vertexCount;
					EditorGUILayout.LabelField(new GUIContent("Count   " + rageSpline.optimizedVertexCount, "Can't modify Vertex Count with Optimize On"));
				}
			});
			if (vCount != _currentVertexCount) {
				_currentVertexCount = vCount;
				rageSpline.SetVertexCount(vCount);
			}

			GuiX.Horizontal(() => {
				EditorGUIUtility.LookLikeControls(120f);
				rageSpline.SetAntialiasingWidth(EditorGUILayout.FloatField("  AntiAlias:  Width", rageSpline.GetAntialiasingWidth(), GUILayout.Width(152f), GUILayout.ExpandWidth(false)));
#if UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#else
				GUILayout.Space(-5f);
#endif
				EditorGUIUtility.LookLikeControls(52f, 0f);
				rageSpline.LocalAaWidth = EditorGUILayout.Toggle(" Local", rageSpline.LocalAaWidth);
			});

			EditorGUIUtility.LookLikeControls(170f, 10f);
			displayOptions = EditorGUILayout.Foldout(displayOptions, "Display Options");
			if (displayOptions) {
				RageSpline.showSplineGizmos = EditorGUILayout.Toggle("          Show spline gizmos", RageSpline.showSplineGizmos);
				RageSpline.showOtherGizmos = EditorGUILayout.Toggle("          Show other gizmos", RageSpline.showOtherGizmos);
				RageSpline.showWireFrameInEditor = EditorGUILayout.Toggle("          Show wireframe", RageSpline.showWireFrameInEditor);
				rageSpline.hideHandles = !(EditorGUILayout.Toggle("          Show handles", !rageSpline.hideHandles));
				rageSpline.ShowTriangleCount = EditorGUILayout.Toggle("          Show tri count", rageSpline.ShowTriangleCount);
				if (rageSpline.ShowTriangleCount)
					EditorGUILayout.FloatField("      Triangle Count", rageSpline.GetTriangleCount());
			}
		});
		EditorGUILayout.Separator();
	}

	private static void PhysicsPopup(int labelWidth, RageSpline rageSpline, bool showMeshColliderWarning) {
		EditorGUIUtility.LookLikeControls(80f, 90f);
		RageSpline.Physics oldPhysics = rageSpline.GetPhysics();
		bool oldCreatePhysicsInEditor = rageSpline.GetCreatePhysicsInEditor();

		rageSpline.SetPhysics((RageSpline.Physics) EditorGUILayout.EnumPopup(" Physics:", rageSpline.GetPhysics(), GUILayout.Width(labelWidth)));
		EditorGUIUtility.LookLikeControls(170f, 10f);
		switch (rageSpline.GetPhysics()) {
			case RageSpline.Physics.None:
				break;
			case RageSpline.Physics.MeshCollider:
			case RageSpline.Physics.OutlineMeshCollider:
				physicsOptions = EditorGUILayout.Foldout(physicsOptions, "    Physics Options");
				if (physicsOptions) {
					rageSpline.LockPhysicsToAppearence = EditorGUILayout.Toggle("      Lock to Appearance",
																		rageSpline.LockPhysicsToAppearence);
					if (!rageSpline.LockPhysicsToAppearence) {
						rageSpline.SetPhysicsColliderCount(EditorGUILayout.IntField("      Physics split count", rageSpline.GetPhysicsColliderCount()));
					}
					rageSpline.SetPhysicsZDepth(EditorGUILayout.FloatField("      Physics z-depth", rageSpline.GetPhysicsZDepth()));
					if (rageSpline.GetPhysics() != RageSpline.Physics.OutlineMeshCollider)
						rageSpline.SetPhysicsNormalOffset(EditorGUILayout.FloatField("      Physics normal offset", rageSpline.GetPhysicsNormalOffset()));

					rageSpline.SetPhysicsMaterial(EditorGUILayout.ObjectField("      Physics material", rageSpline.GetPhysicsMaterial(), typeof(PhysicMaterial), false) as PhysicMaterial);

					if (rageSpline.GetPhysics() != RageSpline.Physics.OutlineMeshCollider)
						rageSpline.SetCreateConvexMeshCollider(EditorGUILayout.Toggle("      Physics convex", rageSpline.GetCreateConvexMeshCollider()));
					rageSpline.PhysicsIsTrigger = EditorGUILayout.Toggle("      Physics Set Trigger", rageSpline.PhysicsIsTrigger);
					rageSpline.SetCreatePhysicsInEditor(EditorGUILayout.Toggle("      Physics editor creation", rageSpline.GetCreatePhysicsInEditor()));
				}
				break;
			case RageSpline.Physics.Boxed:
				physicsOptions = EditorGUILayout.Foldout(physicsOptions, "    Physics Options");
				if (physicsOptions) {
					rageSpline.LockPhysicsToAppearence = EditorGUILayout.Toggle("      Lock to Appearance", rageSpline.LockPhysicsToAppearence);
					if (!rageSpline.LockPhysicsToAppearence) {
						rageSpline.SetPhysicsColliderCount(EditorGUILayout.IntField("      Physics split count",
																					rageSpline.GetPhysicsColliderCount()));
						rageSpline.SetPhysicsZDepth(EditorGUILayout.FloatField("      Physics z-depth", rageSpline.GetPhysicsZDepth()));
						rageSpline.SetPhysicsNormalOffset(EditorGUILayout.FloatField("      Physics offset",
																					 rageSpline.GetPhysicsNormalOffset()));
						rageSpline.SetBoxColliderDepth(EditorGUILayout.FloatField("      Physics normal depth",
																				  rageSpline.GetBoxColliderDepth()));
					}
					rageSpline.SetPhysicsMaterial(
						EditorGUILayout.ObjectField("      Physics material", rageSpline.GetPhysicsMaterial(), typeof(PhysicMaterial),
													false) as PhysicMaterial);
					rageSpline.PhysicsIsTrigger = EditorGUILayout.Toggle("      Physics Set Trigger", rageSpline.PhysicsIsTrigger);
					rageSpline.SetCreatePhysicsInEditor(EditorGUILayout.Toggle("      Physics editor creation",
																			   rageSpline.GetCreatePhysicsInEditor()));
				}
				break;
			case RageSpline.Physics.Farseer:
				rageSpline.SetCreatePhysicsInEditor(EditorGUILayout.Toggle("      Physics editor creation", rageSpline.GetCreatePhysicsInEditor()));
				physicsOptions = EditorGUILayout.Foldout(physicsOptions, "    Physics Options");
				if (physicsOptions) {
					rageSpline.FarseerPhysicsPointsOnly = EditorGUILayout.Toggle("      Use points only", rageSpline.FarseerPhysicsPointsOnly);
					if (!rageSpline.FarseerPhysicsPointsOnly) {
						rageSpline.LockPhysicsToAppearence = EditorGUILayout.Toggle("      Lock to Appearance",
																			rageSpline.LockPhysicsToAppearence);
						if (!rageSpline.LockPhysicsToAppearence) {
							rageSpline.SetPhysicsColliderCount(EditorGUILayout.IntField("      Physics split count",
																						rageSpline.GetPhysicsColliderCount()));
							rageSpline.SetPhysicsNormalOffset(EditorGUILayout.FloatField("      Physics offset",
																						 rageSpline.GetPhysicsNormalOffset()));
						}
					}
				}
				break;
		}
		EditorGUILayout.Separator();

		if (showMeshColliderWarning) {
			if (rageSpline.GetPhysics() != RageSpline.Physics.MeshCollider && oldPhysics == RageSpline.Physics.MeshCollider && rageSpline.GetCreatePhysicsInEditor() ||
				rageSpline.GetPhysics() == RageSpline.Physics.MeshCollider && oldCreatePhysicsInEditor && !rageSpline.GetCreatePhysicsInEditor()) {
				var meshCollider = rageSpline.gameObject.GetComponent<MeshCollider>();
				if (meshCollider != null)
					Debug.Log("RageSpline warning: Please remove the Mesh Collider component from the GameObject '" + rageSpline.gameObject.name + "' manually. This warning can be disabled from the RageSpline/Code/RageSplineEditor.cs line 13.");
			}
		}
	}

	private void ShowCoordinatesData(RageSpline rageSpline) {
		EditorGUILayout.Separator();
		GuiX.Horizontal(() => {
			EditorGUILayout.LabelField("Point  X|Y", "", GUILayout.MinWidth(40f));
			EditorGUILayout.LabelField("Handle In  X|Y", "", GUILayout.MinWidth(40f));
			EditorGUILayout.LabelField("Handle Out  X|Y", "", GUILayout.MinWidth(40f));
		});
		EditorGUIUtility.LookLikeControls();

		for (int p = 0; p < selectedControlPoints.Count; p++) {
			int i = selectedControlPoints[p];
			switch (rageSpline.showCoordinates) {
				case RageSpline.ShowCoordinates.Local:
					EditorGUILayout.BeginHorizontal();
					Vector2 newPos = new Vector2();

					newPos.x = EditorGUILayout.FloatField(rageSpline.GetPosition(i).x, GUILayout.MinWidth(30f));
					newPos.y = EditorGUILayout.FloatField(rageSpline.GetPosition(i).y, GUILayout.MinWidth(30f));

					rageSpline.SetPoint(i, newPos);

					Vector2 newIn = new Vector2();
					Vector2 newOut = new Vector2();

					newIn.x = EditorGUILayout.FloatField(rageSpline.GetInControlPositionPointSpace(i).x, GUILayout.MinWidth(30f));
					newIn.y = EditorGUILayout.FloatField(rageSpline.GetInControlPositionPointSpace(i).y, GUILayout.MinWidth(30f));
					newOut.x = EditorGUILayout.FloatField(rageSpline.GetOutControlPositionPointSpace(i).x, GUILayout.MinWidth(30f));
					newOut.y = EditorGUILayout.FloatField(rageSpline.GetOutControlPositionPointSpace(i).y, GUILayout.MinWidth(30f));

					rageSpline.SetInControlPositionPointSpace(i, newIn);
					rageSpline.SetOutControlPositionPointSpace(i, newOut);

					EditorGUILayout.EndHorizontal();
					break;
				case RageSpline.ShowCoordinates.World:
					EditorGUILayout.BeginHorizontal();
					Vector2 newPos2 = new Vector2();

					newPos2.x = EditorGUILayout.FloatField(rageSpline.GetPositionWorldSpace(i).x, GUILayout.MinWidth(30f));
					newPos2.y = EditorGUILayout.FloatField(rageSpline.GetPositionWorldSpace(i).y, GUILayout.MinWidth(30f));

					rageSpline.SetPointWorldSpace(i, newPos2);

					Vector2 newIn2 = new Vector2();
					Vector2 newOut2 = new Vector2();

					newIn2.x = EditorGUILayout.FloatField(rageSpline.GetInControlPositionWorldSpace(i).x, GUILayout.MinWidth(30f));
					newIn2.y = EditorGUILayout.FloatField(rageSpline.GetInControlPositionWorldSpace(i).y, GUILayout.MinWidth(30f));
					newOut2.x = EditorGUILayout.FloatField(rageSpline.GetOutControlPositionWorldSpace(i).x, GUILayout.MinWidth(30f));
					newOut2.y = EditorGUILayout.FloatField(rageSpline.GetOutControlPositionWorldSpace(i).y, GUILayout.MinWidth(30f));

					rageSpline.SetInControlPositionWorldSpace(i, newIn2);
					rageSpline.SetOutControlPositionWorldSpace(i, newOut2);

					EditorGUILayout.EndHorizontal();
					break;
			}
		}
	}

	public void OnSceneGUI() {
		DisconnectFromRageSplineBasicPrefabs();
		bool mouseDrag = false;
		bool mouseUp = false;
		bool keyUp = false;

		KeyCode keyCode = KeyCode.None;
		if (Event.current.type == EventType.keyUp) {
			keyUp = true;
			keyCode = Event.current.keyCode;
		}

		if (Event.current.type == EventType.mouseUp)
			mouseUp = true;

		if (Event.current.type == EventType.mouseDrag)
			mouseDrag = true;

		Undo.SetSnapshotTarget(target, "Modified object");

		RageSpline rageSpline = target as RageSpline;

		if (rageSpline.renderer != null)
			EditorUtility.SetSelectedWireframeHidden(rageSpline.renderer, !RageSpline.showWireFrameInEditor);

		if (Mathf.Approximately(rageSpline.transform.localScale.x, 0f) ||
			Mathf.Approximately(rageSpline.transform.localScale.y, 0f) ||
			Mathf.Approximately(rageSpline.transform.localScale.z, 0f))
			return;

		// Gradient Handles
		if (RageSpline.showOtherGizmos && rageSpline.GetFill() == RageSpline.Fill.Gradient) {
			Handles.color = Color.green;
			rageSpline.SetGradientOffset(
				rageSpline.transform.InverseTransformPoint(
				Handles.FreeMoveHandle(
					rageSpline.transform.TransformPoint(rageSpline.GetGradientOffset()),
					Quaternion.identity,
					HandleUtility.GetHandleSize(rageSpline.transform.TransformPoint(rageSpline.GetGradientOffset())) * 0.2f,
					Vector3.zero,
					Handles.CircleCap
				))
			);

			Handles.color = Color.green;
			var up = new Vector3(0f, 1f, 0f);
			Vector3 point = rageSpline.RotatePoint2D_CCW(up, rageSpline.GetGradientAngleDeg() * Mathf.Deg2Rad);
			Vector3 middle = rageSpline.GetGradientOffset();

			Vector3 pos =
				rageSpline.transform.InverseTransformPoint(
					Handles.FreeMoveHandle(
						rageSpline.transform.TransformPoint(middle + point * (1f / rageSpline.GetGradientScaleInv())),
						Quaternion.identity,
						HandleUtility.GetHandleSize(rageSpline.transform.TransformPoint(point)) * 0.1f,
						Vector3.zero,
						Handles.CircleCap
					)
				);

			Vector2 pos2D = pos;
			rageSpline.SetGradientScaleInv(1f / (pos2D - rageSpline.GetGradientOffset()).magnitude);

			Vector3 dir = (pos - middle).normalized;
			rageSpline.SetGradientAngleDeg(rageSpline.Vector2Angle_CCW(dir));
		}

		// Texturing Handles
		if (RageSpline.showOtherGizmos && rageSpline.GetFill() != RageSpline.Fill.None && rageSpline.GetTexturing1() == RageSpline.UVMapping.Fill) {
			Handles.color = Color.magenta;
			rageSpline.SetTextureOffset(
				rageSpline.transform.InverseTransformPoint(
				Handles.FreeMoveHandle(
					rageSpline.transform.TransformPoint(rageSpline.GetTextureOffset()),
					Quaternion.identity,
					HandleUtility.GetHandleSize(rageSpline.transform.TransformPoint(rageSpline.GetTextureOffset())) * 0.2f,
					Vector3.zero,
					Handles.CircleCap
				))
			);

			Vector3 point = rageSpline.RotatePoint2D_CCW(Vector3.up, rageSpline.GetTextureAngleDeg() * Mathf.Deg2Rad);
			Vector3 middle = rageSpline.GetTextureOffset();

			Vector3 pos =
				rageSpline.transform.InverseTransformPoint(
					Handles.FreeMoveHandle(
						rageSpline.transform.TransformPoint(middle + point * (1f / rageSpline.GetTextureScaleInv())),
						Quaternion.identity,
						HandleUtility.GetHandleSize(rageSpline.transform.TransformPoint(point)) * 0.1f,
						Vector3.zero,
						Handles.CircleCap
					)
				);

			Vector2 pos2D = pos;
			rageSpline.SetTextureScaleInv(1f / (pos2D - rageSpline.GetTextureOffset()).magnitude);
			Vector3 dir = (pos - middle).normalized;
			rageSpline.SetTextureAngleDeg(rageSpline.Vector2Angle_CCW(dir));
		}

		// Texture 2 Handles
		if (RageSpline.showOtherGizmos && rageSpline.GetFill() != RageSpline.Fill.None && rageSpline.GetTexturing2() == RageSpline.UVMapping.Fill) {

			Handles.color = Color.magenta;
			rageSpline.SetTextureOffset2(
				rageSpline.transform.InverseTransformPoint(
				Handles.FreeMoveHandle(
					rageSpline.transform.TransformPoint(rageSpline.GetTextureOffset2()),
					Quaternion.identity,
					HandleUtility.GetHandleSize(rageSpline.transform.TransformPoint(rageSpline.GetTextureOffset2())) * 0.2f,
					Vector3.zero,
					Handles.CircleCap
					)
				)
			);

			Vector3 point = rageSpline.RotatePoint2D_CCW(Vector3.up, rageSpline.GetTextureAngle2Deg() * Mathf.Deg2Rad);
			Vector3 middle = rageSpline.GetTextureOffset2();

			Vector3 pos = rageSpline.transform.InverseTransformPoint(
					Handles.FreeMoveHandle(
						rageSpline.transform.TransformPoint(middle + point * (1f / rageSpline.GetTextureScale2Inv())),
						Quaternion.identity,
						HandleUtility.GetHandleSize(rageSpline.transform.TransformPoint(point)) * 0.1f,
						Vector3.zero,
						Handles.CircleCap
					)
				);

			Vector2 pos2d = pos;
			rageSpline.SetTextureScale2Inv(1f / (pos2d - rageSpline.GetTextureOffset2()).magnitude);
			Vector3 dir = (pos - middle).normalized;
			rageSpline.SetTextureAngle2Deg(rageSpline.Vector2Angle_CCW(dir));
		}

		// Emboss Handles
		if (RageSpline.showOtherGizmos && rageSpline.GetEmboss() != RageSpline.Emboss.None) {

			Handles.color = Color.blue;

			Vector3 up = new Vector3(0f, 1f, 0f);
			Vector3 point = rageSpline.RotatePoint2D_CCW(up, rageSpline.GetEmbossAngleDeg() * Mathf.Deg2Rad);
			Vector3 middle = rageSpline.GetMiddle();

			Vector3 pos =
				rageSpline.transform.InverseTransformPoint(
					Handles.FreeMoveHandle(
						rageSpline.transform.TransformPoint(middle + point * (rageSpline.GetEmbossSize() * 4f)),
						Quaternion.identity,
						HandleUtility.GetHandleSize(rageSpline.transform.TransformPoint(point)) * 0.2f,
						Vector3.zero,
						Handles.ConeCap
					)
				);

			Vector2 pos2d = pos - middle;
			rageSpline.SetEmbossSize(pos2d.magnitude * 0.25f);
			Vector2 dir = pos2d.normalized;
			rageSpline.SetEmbossAngleDeg(rageSpline.Vector2Angle_CCW(dir));
		}

		// Shortcut Processing
		if (Event.current.type == EventType.KeyDown) {

			if (Event.current.keyCode == KeyCode.I) {
				Color oldOutlineColor1 = rageSpline.GetOutlineColor1();
				Color oldOutlineColor2 = rageSpline.GetOutlineColor2();
				rageSpline.SetOutlineColor1(rageSpline.GetFillColor1());
				if (rageSpline.GetFill() == RageSpline.Fill.Gradient)
					rageSpline.SetOutlineColor2(rageSpline.GetFillColor2());

				rageSpline.SetFillColor1(oldOutlineColor1);
				if (rageSpline.GetOutlineGradient() != RageSpline.OutlineGradient.None)
					rageSpline.SetFillColor2(oldOutlineColor2);

				GUI.changed = true;
				EditorUtility.SetDirty(target);
				Event.current.Use();
			}

			if (Event.current.keyCode == KeyCode.O) {
				rageSpline.SetOutlineColor1(Color.black);
				rageSpline.SetOutlineColor2(Color.black);
				rageSpline.SetFillColor1(Color.white);
				rageSpline.SetFillColor2(Color.white);
				rageSpline.SetOutlineGradient(RageSpline.OutlineGradient.None);
				rageSpline.SetFill(RageSpline.Fill.Solid);
				GUI.changed = true;
				EditorUtility.SetDirty(target);
				Event.current.Use();
			}
		}

		if (RageSpline.showSplineGizmos) {

			if (Event.current.type == EventType.mouseDown) {
				Vector3 clickPosition = rageSpline.transform.InverseTransformPoint(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).GetPoint((rageSpline.transform.position - Camera.current.transform.position).magnitude));
				if (Event.current.control || Event.current.clickCount > 1) {
					clickPosition.z = rageSpline.transform.position.z;
					float nearestWeight = rageSpline.GetNearestSplinePosition(clickPosition, 1000);
					rageSpline.AddPoint(nearestWeight);
					rageSpline.RefreshMeshInEditor(true, true, true);
					Event.current.Use();
					GUI.changed = true;
				} else {
					int nearestIdx = rageSpline.GetNearestPointIndex(clickPosition);
					if (Event.current.shift)
						SelectPoint(nearestIdx);
					else {
						selectedControlPoints.Clear();
						SelectPoint(nearestIdx);
					}
				}
				_dragSegmentShortcutPressed = false;
			}

			if (Event.current.type == EventType.KeyUp)
				_dragSegmentShortcutPressed = false;

			if (Event.current.type == EventType.KeyDown) {

				if (Event.current.keyCode == KeyCode.J) { // && selectedControlPoints.Count == 0) {
					Vector3 clickPosition = rageSpline.transform.InverseTransformPoint(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).GetPoint((rageSpline.transform.position - Camera.current.transform.position).magnitude));
					if (!_dragSegmentShortcutPressed) {
						_dragSegmentNearestIdx = rageSpline.GetNearestPointIndex(clickPosition);
						_dragSegmentShortcutPressed = true;
					}
					int prevIdx = (_dragSegmentNearestIdx > 0) ? _dragSegmentNearestIdx - 1 : rageSpline.GetLastIndex();
					int nextIdx = (_dragSegmentNearestIdx == rageSpline.GetLastIndex()) ? 0 : _dragSegmentNearestIdx + 1;
					Vector3 nearestOutCtrl = rageSpline.GetOutControlPositionWorldSpace(nextIdx);
					Vector3 nearestInCtrl = rageSpline.GetInControlPositionWorldSpace(prevIdx);
					float inCtrlDistance = (nearestInCtrl - clickPosition).sqrMagnitude;
					float outCtrlDistance = (nearestOutCtrl - clickPosition).sqrMagnitude;
					//Debug.Log("Nearest Idx: " + nearestIdx);
					//Debug.Log("Out Ctrl: " + nearestOutCtrl + "  In Ctrl: " + nearestInCtrl);
					if (inCtrlDistance < outCtrlDistance)
						rageSpline.SetInControlPosition(_dragSegmentNearestIdx, clickPosition);
					else
						rageSpline.SetOutControlPosition(_dragSegmentNearestIdx, clickPosition);
					rageSpline.RefreshMeshInEditor(true, true, true);
					ShortcutApplied();
				}
				if (Event.current.keyCode == KeyCode.N && selectedControlPoints.Count > 0) {
					foreach (int thisControlPoint in selectedControlPoints)
						rageSpline.SetNatural(thisControlPoint, !rageSpline.GetNatural(thisControlPoint));
					ShortcutApplied();
				} else if (Event.current.keyCode == KeyCode.L && selectedControlPoints.Count > 0) {
					foreach (int thisControlPoint in selectedControlPoints)
						rageSpline.SetOutlineWidthMultiplier(thisControlPoint, Mathf.Max(rageSpline.GetOutlineWidthMultiplier(thisControlPoint) * 1.15f, 0.01f));
					ShortcutApplied();
				} else if (Event.current.keyCode == KeyCode.K && selectedControlPoints.Count > 0) {
					foreach (int thisControlPoint in selectedControlPoints)
						rageSpline.SetOutlineWidthMultiplier(thisControlPoint, Mathf.Max(rageSpline.GetOutlineWidthMultiplier(thisControlPoint) * 0.85f, 0.01f));
					ShortcutApplied();
				} else if (Event.current.keyCode == KeyCode.L && selectedControlPoints.Count > 0 && Event.current.shift) {
					rageSpline.scaleHandles(1.1f);
					ShortcutApplied();
				} else if (Event.current.keyCode == KeyCode.K && selectedControlPoints.Count > 0 && Event.current.shift) {
					rageSpline.scaleHandles(0.9f);
					ShortcutApplied();
				} else if (Event.current.keyCode == KeyCode.L && selectedControlPoints.Count > 0 && (Event.current.control || Event.current.command)) {
					rageSpline.scalePoints(rageSpline.GetMiddle(), 1.1f);
					ShortcutApplied();
				} else if (Event.current.keyCode == KeyCode.K && selectedControlPoints.Count > 0 && (Event.current.control || Event.current.command)) {
					rageSpline.scalePoints(rageSpline.GetMiddle(), 0.9f);
					ShortcutApplied();
				} else if (Event.current.keyCode == KeyCode.L && selectedControlPoints.Count > 0) {
					rageSpline.scalePoints(rageSpline.GetMiddle(), 1.1f);
					rageSpline.scaleHandles(1.1f);
					ShortcutApplied();
				} else if (Event.current.keyCode == KeyCode.K && selectedControlPoints.Count > 0) {
					rageSpline.scalePoints(rageSpline.GetMiddle(), 0.9f);
					rageSpline.scaleHandles(0.9f);
					ShortcutApplied();
				} else if ((Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace) && selectedControlPoints.Count > 0) {
					selectedControlPoints.Sort();
					selectedControlPoints.Reverse();
					foreach (int thisControlPoint in selectedControlPoints) {
						if (Event.current.shift)
							rageSpline.RemovePoint(thisControlPoint);
						else
							rageSpline.SmartRemovePoint(thisControlPoint);
					}
					selectedControlPoints.Clear();
					rageSpline.RefreshMeshInEditor(true, true, true);
					GUI.changed = true;
					Event.current.Use();
				} else if (Event.current.keyCode == KeyCode.H) {
					rageSpline.hideHandles = !rageSpline.hideHandles;
					ShortcutApplied();
				} else if ((Event.current.keyCode == KeyCode.C))
					rageSpline.setPivotCenter();
			}

			Handles.color = Color.white;

			// Loops through all points
			for (int s = 0; s < rageSpline.GetPointCount(); s++) {

				Handles.DrawCapFunction handleCap;
				float handleSizeMultiplier;
				if (rageSpline.GetNatural(s)) {
					handleCap = Handles.SphereCap;
					handleSizeMultiplier = 2.5f;
				} else {
					handleCap = Handles.RectangleCap;
					handleSizeMultiplier = 1f;
				}

				bool guiChanged = GUI.changed;
				Vector3 oldPosition = rageSpline.GetPosition(s);

				Handles.color = Color.white;
				if (selectedControlPoints.Count > 1)
					foreach (int selectedControlPoint in selectedControlPoints)
						if (s == selectedControlPoint)
							Handles.color = new Color(1f, 0.6f, 0);	//orange

				// Drag points
				rageSpline.SetPointWorldSpace(s,
					Handles.FreeMoveHandle(
						rageSpline.GetPositionWorldSpace(s),
						Quaternion.identity,
						HandleUtility.GetHandleSize(rageSpline.GetPositionWorldSpace(s)) * 0.075f * handleSizeMultiplier,
						Vector3.zero,
						handleCap
					)
				);

				if (!guiChanged && GUI.changed) {
					if (selectedControlPoints.Count > 1) {
						Vector3 offset = oldPosition - rageSpline.GetPosition(s);
						//selectedControlPoints = s;
						foreach (int selectedControlPoint in selectedControlPoints) {
							if (selectedControlPoint != s)
								rageSpline.SetPoint(selectedControlPoint, rageSpline.GetPosition(selectedControlPoint) - offset);

							if (rageSpline.snapToGrid && rageSpline.showCoordinates != RageSpline.ShowCoordinates.None)
								if (rageSpline.showCoordinates == RageSpline.ShowCoordinates.Local)
									rageSpline.SetPoint(s, SnapToGrid(rageSpline.GetPosition(s), rageSpline.gridSize));
								else
									rageSpline.SetPointWorldSpace(s, SnapToGrid(rageSpline.GetPositionWorldSpace(s), rageSpline.gridSize));
						}
					}
				}

				// Draw Control Handles
				Handles.color = Color.white;
				if (!rageSpline.hideHandles) {
					guiChanged = GUI.changed;
					rageSpline.SetInControlPositionWorldSpace(s,
						Handles.FreeMoveHandle(
							rageSpline.GetInControlPositionWorldSpace(s),
							Quaternion.identity,
							HandleUtility.GetHandleSize(rageSpline.GetPositionWorldSpace(s)) * 0.1f,
							Vector3.zero,
							Handles.SphereCap
						)
					);
					if (!guiChanged && GUI.changed) {
						if (rageSpline.snapHandlesToGrid && rageSpline.showCoordinates != RageSpline.ShowCoordinates.None) {
							if (rageSpline.showCoordinates == RageSpline.ShowCoordinates.Local)
								rageSpline.SetInControlPosition(s, SnapToGrid(rageSpline.GetInControlPosition(s), rageSpline.gridSize));
							else
								rageSpline.SetInControlPositionWorldSpace(s, SnapToGrid(rageSpline.GetInControlPositionWorldSpace(s), rageSpline.gridSize));
						}
					}

					guiChanged = GUI.changed;
					rageSpline.SetOutControlPositionWorldSpace(s,
						Handles.FreeMoveHandle(
							rageSpline.GetOutControlPositionWorldSpace(s),
							Quaternion.identity,
							HandleUtility.GetHandleSize(rageSpline.GetPositionWorldSpace(s)) * 0.1f,
							Vector3.zero,
							Handles.SphereCap
						)
					);
				}

				if (!guiChanged && GUI.changed) {											//selectedControlPoints = s;
					if (rageSpline.snapHandlesToGrid && rageSpline.showCoordinates != RageSpline.ShowCoordinates.None) {
						if (rageSpline.showCoordinates == RageSpline.ShowCoordinates.Local)
							rageSpline.SetOutControlPosition(s, SnapToGrid(rageSpline.GetOutControlPosition(s), rageSpline.gridSize));
						else
							rageSpline.SetOutControlPositionWorldSpace(s, SnapToGrid(rageSpline.GetOutControlPositionWorldSpace(s), rageSpline.gridSize));
					}
				}
			}

		}

		if (Event.current.type == EventType.mouseDown) {
			Undo.CreateSnapshot();
			Undo.RegisterSnapshot();
		}

		if (mouseUp || (keyUp && keyCode != KeyCode.Delete) || mouseDrag) {
			if (mouseUp || keyUp)
				rageSpline.RefreshMeshInEditor(true, true, true);
			else
				rageSpline.RefreshMeshInEditor(false, true, true);
			EditorUtility.SetDirty(target);
		}
	}

	private void ShortcutApplied() {
		GUI.changed = true;
		EditorUtility.SetDirty(target);
		Event.current.Use();
	}

	private void SelectPoint(int index) {
		if (selectedControlPoints.Contains(index)) return;
		selectedControlPoints.Add(index);
	}

	public float SnapToGrid(float val, float gridsize) {
		if (mod(val, gridsize) < gridsize * 0.5f)
			return val - mod(val, gridsize);

		return val + (gridsize - mod(val, gridsize));
	}

	public Vector3 SnapToGrid(Vector3 val, float gridsize) {
		if (selectedControlPoints.Count == 1)
			return new Vector3(SnapToGrid(val.x, gridsize), SnapToGrid(val.y, gridsize));

		return new Vector3(val.x, val.y);
	}

	public void DisconnectFromRageSplineBasicPrefabs() {
		GameObject go = ((RageSpline) target).gameObject;
		if (go == null) return;

		switch (go.name) {
			case ("Blob"):
			case ("Circle"):
			case ("Line"):
			case ("MultitextureLandscape"):
			case ("Rectangle"):
			case ("Triangle"):
				PrefabUtility.DisconnectPrefabInstance(go);
				break;
		}
	}

	// 	private int mod(int x, int m) {
	// 		return (x % m + m) % m;
	// 	}

	private float mod(float x, float m) {
		return (x % m + m) % m;
	}
}
