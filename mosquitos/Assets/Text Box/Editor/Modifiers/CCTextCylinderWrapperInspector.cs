/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.2
	
	1.0.2: Initial version.
*/

using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(CCTextCylinderWrapper))]
public class CCTextCylinderWrapperInspector : Editor {
	
	[MenuItem("Assets/Create/Text Box/Cylinder Wrapper", false, 3000)]
	static void CreateNewAsset () {
		CCEditorUtility.CreateAsset<CCTextCylinderWrapper>("New Cylinder Wrapper");
	}
	
	private SerializedObject modifier;
	private SerializedProperty
		radius,
		wrapMode;
	
	void OnEnable () {
		foreach(Object o in targets){
			if(o as CCTextCylinderWrapper == null){
				return;
			}
		}
		modifier = new SerializedObject(targets);
		radius = modifier.FindProperty("radius");
		wrapMode = modifier.FindProperty("wrapMode");
	}
	
	public override void OnInspectorGUI () {
		if(modifier == null){
			GUILayout.Label("Cannot edit multiple modifiers with different types.");
			return;
		}
		
		modifier.Update();
		
		EditorGUILayout.PropertyField(wrapMode);
		EditorGUILayout.PropertyField(radius);
		
		if(modifier.ApplyModifiedProperties() || CCEditorUtility.UndoRedoEventHappened){
			foreach(CCTextCylinderWrapper m in targets){
				m.UpdateAllCCText();
			}
		}
	}
}
