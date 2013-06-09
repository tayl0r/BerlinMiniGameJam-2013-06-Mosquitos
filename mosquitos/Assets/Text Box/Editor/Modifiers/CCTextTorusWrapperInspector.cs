/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.3
	
	1.0.3: Initial version.
*/

using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(CCTextTorusWrapper))]
public class CCTextTorusWrapperInspector : Editor {
	
	[MenuItem("Assets/Create/Text Box/Torus Wrapper", false, 3001)]
	static void CreateNewAsset () {
		CCEditorUtility.CreateAsset<CCTextTorusWrapper>("New Torus Wrapper");
	}
	
	private SerializedObject modifier;
	private SerializedProperty
		minorRadius,
		majorRadius,
		revolveMode;
	
	void OnEnable () {
		foreach(Object o in targets){
			if(o as CCTextTorusWrapper == null){
				return;
			}
		}
		modifier = new SerializedObject(targets);
		minorRadius = modifier.FindProperty("minorRadius");
		majorRadius = modifier.FindProperty("majorRadius");
		revolveMode = modifier.FindProperty("revolveMode");
	}
	
	public override void OnInspectorGUI () {
		if(modifier == null){
			GUILayout.Label("Cannot edit multiple modifiers with different types.");
			return;
		}
		
		modifier.Update();
		
		EditorGUILayout.PropertyField(revolveMode);
		EditorGUILayout.PropertyField(majorRadius);
		EditorGUILayout.PropertyField(minorRadius);
		
		if(modifier.ApplyModifiedProperties() || CCEditorUtility.UndoRedoEventHappened){
			foreach(CCTextTorusWrapper m in targets){
				m.UpdateAllCCText();
			}
		}
	}
}
