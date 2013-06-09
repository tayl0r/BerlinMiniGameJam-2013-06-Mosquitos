using UnityEngine;
using System;
using System.Collections;

public static class GuiX {

	public static Action<Action> Horizontal = horizontalBlockActions => {
		GUILayout.BeginHorizontal();
		horizontalBlockActions();
		GUILayout.EndHorizontal();
	};

// 	private static Action<Action> Vertical = verticalBlockActions => {
// 		GUILayout.BeginVertical();
// 		verticalBlockActions();
// 		GUILayout.EndVertical();
// 	};

	public static Action<GUIStyle, Action> Vertical = (blockStyle, verticalBlockActions) => {
		if (blockStyle==null) GUILayout.BeginVertical();
		else 
			GUILayout.BeginVertical(blockStyle);
		verticalBlockActions();
		GUILayout.EndVertical();
	};

}
