/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.2
	
	1.0.2: Added support for multi-object editing.
	1.0.0: Initial version.
*/

using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(CCTextMarkedColorer))]
public class CCTextMarkedColorerInspector : Editor {
	
	[MenuItem("Assets/Create/Text Box/Marked Colorer", false, 2000)]
	static void CreateNewAsset () {
		CCEditorUtility.CreateAsset<CCTextMarkedColorer>("New Marked Colorer");
	}
	
	private SerializedObject modifier;
	private SerializedProperty
		beginSymbol,
		endSymbol,
		color;
	
	void OnEnable () {
		foreach(Object o in targets){
			if(o as CCTextMarkedColorer == null){
				return;
			}
		}
		modifier = new SerializedObject(targets);
		beginSymbol = modifier.FindProperty("beginSymbol");
		endSymbol = modifier.FindProperty("endSymbol");
		color = modifier.FindProperty("color");
	}
	
	public override void OnInspectorGUI () {
		if(modifier == null){
			GUILayout.Label("Cannot edit multiple modifiers with different types.");
			return;
		}
		
		modifier.Update();
		
		EditorGUILayout.PropertyField(beginSymbol);
		EditorGUILayout.PropertyField(endSymbol);
		EditorGUILayout.PropertyField(color);
		
		if(modifier.ApplyModifiedProperties() || CCEditorUtility.UndoRedoEventHappened){
			foreach(CCTextMarkedColorer m in targets){
				if(m.beginSymbol.Length == 0){
					m.beginSymbol = "[";
				}
				else if(m.beginSymbol.Length > 1){
					m.beginSymbol = m.beginSymbol[0].ToString();
				}
				if(m.endSymbol.Length == 0){
					m.endSymbol = "]";
				}
				else if(m.endSymbol.Length > 1){
					m.endSymbol = m.endSymbol[0].ToString();
				}
				m.UpdateAllCCText();
			}
		}
	}
}
