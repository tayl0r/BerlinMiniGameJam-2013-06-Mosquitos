/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.2
	
	1.0.2: Added support for multi-object editing.
	1.0.0: Initial version.
*/

using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(CCTextVerticalColorer))]
public class CCTextVerticalColorerInspector : Editor {
	
	[MenuItem("Assets/Create/Text Box/Vertical Colorer", false, 2002)]
	static void CreateNewAsset () {
		CCEditorUtility.CreateAsset<CCTextVerticalColorer>("New Vertical Colorer");
	}
	
	private SerializedObject modifier;
	private SerializedProperty
		topColor,
		bottomColor;
	
	void OnEnable () {
		foreach(Object o in targets){
			if(o as CCTextVerticalColorer == null){
				return;
			}
		}
		modifier = new SerializedObject(targets);
		topColor = modifier.FindProperty("topColor");
		bottomColor = modifier.FindProperty("bottomColor");
	}
	
	public override void OnInspectorGUI () {
		if(modifier == null){
			GUILayout.Label("Cannot edit multiple modifiers with different types.");
			return;
		}
		
		modifier.Update();
		
		EditorGUILayout.PropertyField(topColor);
		EditorGUILayout.PropertyField(bottomColor);
		
		if(modifier.ApplyModifiedProperties() || CCEditorUtility.UndoRedoEventHappened){
			foreach(CCTextVerticalColorer m in targets){
				m.UpdateAllCCText();
			}
		}
	}
}
