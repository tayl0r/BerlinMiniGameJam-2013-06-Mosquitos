/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.2
	
	1.0.2: Added support for multi-object editing.
	1.0.0: Initial version.
*/

using System;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(CCGradient))]
public sealed class CCGradientInspector : Editor {
	
	[MenuItem("Assets/Create/Gradient")]
	static void CreateColorGradient () {
		CCEditorUtility.CreateAsset<CCGradient>("New Gradient");
	}
	
	private static GUIContent
		stopContent = GUIContent.none,
		teleportStopContent = new GUIContent("T"),
		addStopContent = new GUIContent("+", "add stop"),
		removeStopContent = new GUIContent("-", "remove stop"),
		interpolateContent = new GUIContent("Interpolation", "how to interpolate between stops"),
		wrapContent = new GUIContent("Wrap", "how to deal with input values outside the [0,1] range");
	
	private static GUILayoutOption colorWidth = GUILayout.MaxWidth(50f);
	
	private SerializedObject gradient;
	private SerializedProperty
		colors,
		positions,
		interpolation,
		wrap;
	
	private int teleportingElement;
	
	void OnEnable () {
		gradient = new SerializedObject(targets);
		colors = gradient.FindProperty("colors");
		positions = gradient.FindProperty("positions");
		interpolation = gradient.FindProperty("interpolation");
		wrap = gradient.FindProperty("wrap");
		teleportingElement = -1;
		teleportStopContent.tooltip = "start teleporting this point";
		InitPreview();
	}
	
	public override void OnInspectorGUI () {
		gradient.Update();
		
		GUILayout.Label("Stops");
		for(int i = 0; i < positions.arraySize; i++){
			SerializedProperty position = positions.GetArrayElementAtIndex(i);
			if(position.propertyType != SerializedPropertyType.Float){
				break;
			}
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Slider(position, 0f, 1f, stopContent);
			EditorGUILayout.PropertyField(colors.GetArrayElementAtIndex(i), stopContent, colorWidth);
			
			if(!gradient.isEditingMultipleObjects){
				if(GUILayout.Button(teleportStopContent, EditorStyles.miniButtonLeft)){
					if(teleportingElement >= 0){
						positions.MoveArrayElement(teleportingElement, i);
						colors.MoveArrayElement(teleportingElement, i);
						teleportingElement = -1;
						teleportStopContent.tooltip = "start teleporting this stop";
					}
					else{
						teleportingElement = i;
						teleportStopContent.tooltip = "teleport here";
					}
				}
				if(GUILayout.Button(addStopContent, EditorStyles.miniButtonMid)){
					positions.InsertArrayElementAtIndex(i);
					colors.InsertArrayElementAtIndex(i);
				}
				if(positions.arraySize > 1 && GUILayout.Button(removeStopContent, EditorStyles.miniButtonRight)){
					positions.DeleteArrayElementAtIndex(i);
					colors.DeleteArrayElementAtIndex(i);
				}
			}
			
			EditorGUILayout.EndHorizontal();
		}
		if(teleportingElement >= 0){
			GUILayout.Label("teleporting stop " + teleportingElement);
		}
		
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(interpolation, interpolateContent);
		EditorGUILayout.PropertyField(wrap, wrapContent);
		EditorGUILayout.Space();
		
		gradient.ApplyModifiedProperties();
		
		if(!gradient.isEditingMultipleObjects && GUILayout.Button("Export PNG file")) {
			CCGradientExporterWindow.OpenWindow();
		}
	}
	
	#region Preview
	
	private CCEditorUtility.TextureDrawMode previewMode;
	private Texture2D
		previewTexture,
		thumbTexture;
	private float
		previewFrom,
		previewTo = 1f;
	
	void OnDisable () {
		DestroyImmediate(previewTexture);
		DestroyImmediate(thumbTexture);
	}
	
	private void InitPreview () {
		previewMode = (CCEditorUtility.TextureDrawMode)EditorPrefs.GetInt("CCGradientEditor.previewMode");
		previewFrom = EditorPrefs.GetFloat("CCGradientEditor.previewFrom");
		previewTo = EditorPrefs.GetFloat("CCGradientEditor.previewTo", 1f);
		previewTexture = new Texture2D(1, 1);
		previewTexture.hideFlags = HideFlags.HideAndDontSave;
		thumbTexture = new Texture2D(40, 1);
		thumbTexture.hideFlags = HideFlags.HideAndDontSave;
	}
	
	public override void OnPreviewSettings () {
		GUILayout.Label("from");
		float oldValue = previewFrom;
		previewFrom = EditorGUILayout.FloatField(previewFrom, GUILayout.MaxWidth(30f));
		if(previewFrom != oldValue){
			EditorPrefs.SetFloat("CCGradientEditor.previewFrom", previewFrom);
		}
		GUILayout.Label("to");
		oldValue = previewTo;
		previewTo = EditorGUILayout.FloatField(previewTo, GUILayout.MaxWidth(30f));
		if(previewTo != oldValue){
			EditorPrefs.SetFloat("CCGradientEditor.previewTo", previewTo);
		}
		CCEditorUtility.TextureDrawMode oldMode = previewMode;
		previewMode = (CCEditorUtility.TextureDrawMode)EditorGUILayout.EnumPopup(previewMode, GUILayout.MaxWidth(50f));
		if(previewMode != oldMode){
			EditorPrefs.SetInt("CCGradientEditor.previewMode", (int)previewMode);
		}
	}
	
	public override bool HasPreviewGUI () {
		return true;
	}

	public override void OnPreviewGUI (Rect r, GUIStyle background) {
		CCGradient gradient = (CCGradient)target;
		if(r.width <= 1f){
			// events, no drawing
			return;
		}
		if(r.width == 40f){
			// draw the thumbnail
			gradient.WriteToTexture(0f, 1f, thumbTexture);
			thumbTexture.Apply();
			CCEditorUtility.DrawTexture(r, thumbTexture, previewMode);
			return;
		}
		
		// draw the preview
		if((int)r.width != previewTexture.width){
			previewTexture.Resize((int)r.width, 1);
		}
		gradient.WriteToTexture(previewFrom, previewTo, previewTexture);
		previewTexture.Apply();
		CCEditorUtility.DrawTexture(r, previewTexture, previewMode);
	}
	
	#endregion
}
