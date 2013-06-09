using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RageCanvasAlign))]
public class RageCanvasAlignEditor : Editor
{
	public override void OnInspectorGUI(){

		var canvasAlign = target as RageCanvasAlign;
		if (canvasAlign == null) return;

		EditorGUILayout.Separator();
		//---------

		EditorGUILayout.BeginHorizontal();
		{
// 			var icon = (Texture2D)Resources.Load("ragetoolsicon");
// 			if (icon != null)
// 				GUILayout.Box(icon, GUILayout.MinHeight(22f), GUILayout.MinWidth(22f),
// 								GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(false));

			canvasAlign.StartOnly = GUILayout.Toggle(canvasAlign.StartOnly, "Start Only", GUILayout.MinWidth(75f), GUILayout.MaxHeight(18f));

			EditorGUILayout.BeginVertical();
			{
				GUILayout.Space(4f);
				GUILayout.Label("H:");
			} EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			{
				GUILayout.Space(4f); 
				EditorGUIUtility.LookLikeControls(50f, 30f);

				canvasAlign.HorizontalAlign = (RageCanvasAlign.HorizontalAlignType)
											  EditorGUILayout.EnumPopup(canvasAlign.HorizontalAlign, GUILayout.MaxWidth(60f));
				EditorGUILayout.Space();
			} EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			{
				GUILayout.Space(4f);
				GUILayout.Label("V:");
			} EditorGUILayout.EndVertical();

			EditorGUILayout.BeginVertical();
			{
				GUILayout.Space(4f);
				EditorGUIUtility.LookLikeControls(50f, 30f);
				canvasAlign.VerticalAlign = (RageCanvasAlign.VerticalAlignType)
											EditorGUILayout.EnumPopup(canvasAlign.VerticalAlign, GUILayout.MaxWidth(60f));
				EditorGUILayout.Space();
			} EditorGUILayout.EndVertical();

		} EditorGUILayout.EndHorizontal();

//         EditorGUIUtility.LookLikeControls(90f, 10f);
// #if UNITY_3_4 || UNITY_3_5
//         canvasAlign.ReferenceGO = (GameObject)EditorGUILayout.ObjectField("Debug gO: ", canvasAlign.ReferenceGO, typeof(GameObject), true);
// #else
//         canvasAlign.ReferenceGO = (GameObject)EditorGUILayout.ObjectField("Debug gO: ", canvasAlign.ReferenceGO, typeof(GameObject));
// #endif

		// For the script to be updated every frame (and re-check the canvas size), setdirty must be unconditional
		// And the function must be iterated on OnGUI, not OnUpdate
		EditorUtility.SetDirty(target);
	}
}
