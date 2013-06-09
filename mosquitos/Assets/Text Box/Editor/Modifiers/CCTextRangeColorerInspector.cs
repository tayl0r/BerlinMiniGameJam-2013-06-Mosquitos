/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.2
	
	1.0.2: Added support for multi-object editing.
	1.0.0: Initial version.
*/

using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(CCTextRangeColorer))]
public class CCTextRangeColorerInspector : Editor {
	
	[MenuItem("Assets/Create/Text Box/Range Colorer", false, 2001)]
	static void CreateNewAsset () {
		CCEditorUtility.CreateAsset<CCTextRangeColorer>("New Range Colorer");
	}
	
	private SerializedObject modifier;
	private SerializedProperty
		rangeBeginSymbol,
		rangeEndSymbol,
		color;
	
	void OnEnable () {
		foreach(Object o in targets){
			if(o as CCTextRangeColorer == null){
				return;
			}
		}
		modifier = new SerializedObject(targets);
		rangeBeginSymbol = modifier.FindProperty("rangeBeginSymbol");
		rangeEndSymbol = modifier.FindProperty("rangeEndSymbol");
		color = modifier.FindProperty("color");
	}
	
	public override void OnInspectorGUI () {
		if(modifier == null){
			GUILayout.Label("Cannot edit multiple modifiers with different types.");
			return;
		}
		
		modifier.Update();
		
		EditorGUILayout.PropertyField(rangeBeginSymbol);
		EditorGUILayout.PropertyField(rangeEndSymbol);
		EditorGUILayout.PropertyField(color);
		
		if(modifier.ApplyModifiedProperties() || CCEditorUtility.UndoRedoEventHappened){
			foreach(CCTextRangeColorer m in targets){
				if(m.rangeBeginSymbol.Length == 0){
					m.rangeBeginSymbol = "A";
				}
				else if(m.rangeBeginSymbol.Length > 1){
					m.rangeBeginSymbol = m.rangeBeginSymbol[0].ToString();
				}
				if(m.rangeEndSymbol.Length == 0){
					m.rangeEndSymbol = "Z";
				}
				else if(m.rangeEndSymbol.Length > 1){
					m.rangeEndSymbol = m.rangeEndSymbol[0].ToString();
				}
				m.UpdateAllCCText();
			}
		}
	}
}
