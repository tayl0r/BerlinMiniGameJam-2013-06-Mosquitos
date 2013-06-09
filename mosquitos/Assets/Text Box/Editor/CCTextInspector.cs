/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.2
	
	1.0.2: Added support for multi-object editing. OnSceneGUI now draws a cube if the bounds are 3D.
	1.0.1: Fixed prefab change not triggering update of instances.
	1.0.0: Initial version.
*/

using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(CCText))]
public sealed class CCTextInspector : Editor {
	
	[MenuItem("GameObject/Create Other/Text Box")]
	static void CreateTextBox () {
		CCFont font = Selection.activeObject as CCFont;
		CCText newBox = CCEditorUtility.CreateGameObjectWithComponent<CCText>("New Text Box");
		if(font != null){
			// set its font to the currently selected font
			newBox.Font = font;
		}
	}
	
	private static GUIContent anchorContent = GUIContent.none;
	private static GUILayoutOption anchorWidth = GUILayout.Width(60f);
	
	private GUIStyle textFieldStyle;
	
	private SerializedObject serializedBox;
	private SerializedProperty
		alignment,
		horizontalAnchor,
		verticalAnchor,
		bounding,
		chunkSize,
		color,
		font,
		lineHeight,
		modifier,
		offset,
		tabSize,
		text,
		width;
	
	void OnEnable () {
		serializedBox = new SerializedObject(targets);
		alignment = serializedBox.FindProperty("alignment");
		horizontalAnchor = serializedBox.FindProperty("horizontalAnchor");
		verticalAnchor = serializedBox.FindProperty("verticalAnchor");
		bounding = serializedBox.FindProperty("bounding");
		chunkSize = serializedBox.FindProperty("chunkSize");
		color = serializedBox.FindProperty("color");
		font = serializedBox.FindProperty("font");
		lineHeight = serializedBox.FindProperty("lineHeight");
		modifier = serializedBox.FindProperty("modifier");
		offset = serializedBox.FindProperty("offset");
		tabSize = serializedBox.FindProperty("tabSize");
		text = serializedBox.FindProperty("text");
		width = serializedBox.FindProperty("width");
	}
	
	public override void OnInspectorGUI () {
		serializedBox.Update();
		
		EditorGUILayout.PropertyField(color);
		
		Object oldFont = font.objectReferenceValue;
		EditorGUILayout.PropertyField(font);
		if(font.objectReferenceValue != null && oldFont != font.objectReferenceValue && !((CCFont)font.objectReferenceValue).IsValid){
			font.objectReferenceValue = oldFont;
			Debug.LogWarning("CCText refused to accept an invalid CCFont. Please import font data first.");
		}
		
		EditorGUILayout.PropertyField(modifier);
		EditorGUILayout.PropertyField(alignment);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Anchor");
		EditorGUILayout.PropertyField(horizontalAnchor, anchorContent, anchorWidth);
		EditorGUILayout.PropertyField(verticalAnchor, anchorContent, anchorWidth);
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.PropertyField(bounding);
		EditorGUILayout.PropertyField(width);
		EditorGUILayout.PropertyField(chunkSize);
		EditorGUILayout.PropertyField(lineHeight);
		EditorGUILayout.PropertyField(tabSize);
		EditorGUILayout.PropertyField(offset);
		
		if(!serializedBox.isEditingMultipleObjects){
			if(textFieldStyle == null){
				EditorGUIUtility.LookLikeControls();
				textFieldStyle = new GUIStyle(EditorStyles.textField);
				textFieldStyle.wordWrap = true;
			}
			CCText box = (CCText)target;
			GUILayout.Label("Text", text.prefabOverride ? EditorStyles.boldLabel : EditorStyles.label);
			string newText = EditorGUILayout.TextArea(text.stringValue, textFieldStyle, GUILayout.Height(81f));
			if(!newText.Equals(text.stringValue)){
				text.stringValue = newText;
			}
			if(box.enabled){
				GUILayout.Label(" using " + box.UsedSpriteCount + " of " + box.SpriteCount + " sprites", EditorStyles.miniLabel);
			}
		}
		
		if(serializedBox.ApplyModifiedProperties() || CCEditorUtility.UndoRedoEventHappened){
			foreach(CCText t in targets){
				if(PrefabUtility.GetPrefabType(t) != PrefabType.Prefab){
					if(t.LineHeight < 0){
						t.LineHeight = 0;
					}
					if(t.TabSize < 0.001f){
						t.TabSize = 0.001f;
					}
					if(t.Width < 0){
						t.Width = 0;
					}
					t.ResetColors();
					t.UpdateText();
				}
			}
		}
	}
	
	public void OnSceneGUI () {
		CCText box = (CCText)target;
		if(!box.enabled){
			return;
		}
		
		Vector3
			min = box.minBounds,
			max = box.maxBounds;
		Transform t = box.transform;
		
		if(min.z == max.z){
			// draw a white box to show mesh bounds and a yellow box to show caret bounds
			CCEditorUtility.DrawWireRectangle(min, max, t);
			Handles.color = Color.yellow;
			CCEditorUtility.DrawWireRectangle(box.CaretMinBounds, box.CaretMaxBounds, t);
		}
		else{
			CCEditorUtility.DrawWireCube(min, max, t);
		}
	}
}
