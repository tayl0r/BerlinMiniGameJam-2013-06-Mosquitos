/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com
	Version 1.0.2
	
	1.0.2: Added DrawWireCube and DrawWireRectangle. UndoRedoEventHappened now also checks for ExecuteCommand.
	1.0.0: Initial version.
*/

using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Utility class for some common functionality used by Catlike Coding editor classes.
/// </summary>
public static class CCEditorUtility {
	
	private const int checkerCount = 20;
	private const float checkerGrayFactor = 0.8f;
	private static Texture2D checkerPattern;
	
	#region Types
	
	/// <summary>
	/// How to draw a texture.
	/// </summary>
	public enum TextureDrawMode {
		/// <summary>
		/// Draw colors with alpha on top of a checker patter.
		/// </summary>
		RGBA,
		/// <summary>
		/// Draw colors, ignoring alpha.
		/// </summary>
		Colors,
		/// <summary>
		/// Draw alpha as grayscale.
		/// </summary>
		Alpha
	}

	#endregion
	
	#region Static Properties
	
	/// <summary>
	/// Get a texture containing a checker pattern.
	/// Useful as a background for previewing semitransparent effects.
	/// </summary>
	public static Texture2D CheckerPattern {
		get {
			if(checkerPattern == null){
				checkerPattern = new Texture2D(checkerCount, checkerCount, TextureFormat.RGB24, false);
				checkerPattern.hideFlags = HideFlags.HideAndDontSave;
				checkerPattern.filterMode = FilterMode.Point;
				for(int x = 0; x < checkerCount; x++){
					for(int y = 0; y < checkerCount; y++){
						checkerPattern.SetPixel(x, y, ((x + y) & 1) == 0 ? Color.white : Color.white * checkerGrayFactor);
					}
				}
				checkerPattern.Apply(false, true);
			}
			return checkerPattern;
		}
	}
	
	/// <summary>
	/// Get whether an undo or redo event happened.
	/// Only works if inspector or window has focus.
	/// </summary>
	public static bool UndoRedoEventHappened {
		get {
			return (Event.current.type == EventType.ValidateCommand || Event.current.type == EventType.ExecuteCommand) &&
				Event.current.commandName == "UndoRedoPerformed";
		}
	}

	#endregion
	
	#region Static Methods
	
	
	/// <summary>
	/// Create and return a new asset in a smart location based on the current selection and then select it.
	/// </summary>
	/// <param name="name">
	/// Name of the new asset. Do not include the .asset extension.
	/// </param>
	/// <returns>
	/// The new asset.
	/// </returns>
	public static T CreateAsset<T> (string name) where T : ScriptableObject {
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if(path.Length == 0){
			// no asset selected, place in asset root
			path = "Assets/" + name + ".asset";
		}
		else if(Directory.Exists(path)){
			// place in currently selected directory
			path += "/" + name + ".asset";
		}
		else{
			// place in current selection's containing directory
			path = Path.GetDirectoryName(path) + "/" + name + ".asset";
		}
		T asset = ScriptableObject.CreateInstance <T> ();
		AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path));
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
		return asset;
	}
	
	/// <summary>
	/// Create and return a new GameObject, add a component to it, and select it.
	/// Make it a child of the previously selected object, if any.
	/// </summary>
	/// <param name="name">
	/// Name of the new game object.
	/// </param>
	public static T CreateGameObjectWithComponent <T> (string name) where T : MonoBehaviour {
		GameObject o = new GameObject(name);
		T component = o.AddComponent<T>();
		if(Selection.activeTransform != null){
			// make it a child of the currently selected object
			o.transform.parent = Selection.activeTransform;
			o.transform.localPosition = Vector3.zero;
		}
		Selection.activeObject = o;
		return component;
	}
	
	/// <summary>
	/// Draw a wire cube using handles.
	/// </summary>
	/// <param name='min'>
	/// Minimum cube corner.
	/// </param>
	/// <param name='max'>
	/// Maximum cube corner.
	/// </param>
	/// <param name='transform'>
	/// Transform to apply to the cube.
	/// </param>
	public static void DrawWireCube (Vector3 min, Vector3 max, Transform transform) {
		Vector3
			right = transform.right * transform.localScale.x * (max.x - min.x),
			up = transform.up * transform.localScale.y * (max.y - min.y),
			forward = transform.forward * transform.localScale.z * (max.z - min.z),
			a = transform.TransformPoint(min),
			b = a + right,
			c = b + up,
			d = a + up,
			e = a + forward;
		
		Handles.DrawLine(a, b);
		Handles.DrawLine(a, d);
		Handles.DrawLine(a, e);
		
		a = c + forward;
		Handles.DrawLine(c, a);
		Handles.DrawLine(c, b);
		Handles.DrawLine(c, d);
		
		c = b + forward;
		Handles.DrawLine(c, b);
		Handles.DrawLine(c, a);
		Handles.DrawLine(c, e);
		
		c = d + forward;
		Handles.DrawLine(c, d);
		Handles.DrawLine(c, a);
		Handles.DrawLine(c, e);
	}
	
	/// <summary>
	/// Draw a wire rectangle using handles.
	/// </summary>
	/// <param name='min'>
	/// Minimum rectangle corner. Z is ignored.
	/// </param>
	/// <param name='max'>
	/// Maximum rectangle corner. Z is ignored.
	/// </param>
	/// <param name='transform'>
	/// Transform to apply to the rectangle.
	/// </param>
	public static void DrawWireRectangle (Vector3 min, Vector3 max, Transform transform){
		Vector3
			a = transform.TransformPoint(min.x, min.y, min.z),
			b = transform.TransformPoint(min.x, max.y, min.z),
			c = transform.TransformPoint(max.x, max.y, min.z);
		
		Handles.DrawLine(a, b);
		Handles.DrawLine(b, c);
		
		b = transform.TransformPoint(max.x, min.y, min.z);
		Handles.DrawLine(c, b);
		Handles.DrawLine(b, a);
	}
	
	/// <summary>
	/// Draw a texture.
	/// </summary>
	/// <param name="rect">
	/// Where to draw in the view.
	/// </param>
	/// <param name="texture">
	/// What to draw.
	/// </param>
	/// <param name="mode">
	/// How to draw.
	/// </param>
	public static void DrawTexture(Rect rect, Texture2D texture, TextureDrawMode mode) {
		switch(mode){
		case TextureDrawMode.RGBA:
			GUI.DrawTexture(rect, CCEditorUtility.CheckerPattern, ScaleMode.ScaleAndCrop);
			GUI.DrawTexture(rect, texture);
			break;
		case TextureDrawMode.Colors:
			EditorGUI.DrawPreviewTexture(rect, texture);
			break;
		case TextureDrawMode.Alpha:
			EditorGUI.DrawTextureAlpha(rect, texture);
			break;
		}
	}
	
	/// <summary>
	/// Draw a texture using RGBA mode.
	/// </summary>
	/// <param name="rect">
	/// Where to draw in the view.
	/// </param>
	/// <param name="texture">
	/// What to draw.
	/// </param>
	public static void DrawTexture(Rect rect, Texture2D texture) {
		DrawTexture(rect, texture, TextureDrawMode.RGBA);
	}
	
	/// <summary>
	/// Open the online CC Documentation in a browser.
	/// </summary>
	[MenuItem("Help/Catlike Coding Documentation")]
	public static void OpenCCDocumentation () {
		Application.OpenURL("http://catlikecoding.com/unity/documentation/");
	}
	
	/// <summary>
	/// Open a file panel in the same folder as some asset.
	/// Return the selected path or the empty string if canceled.
	/// </summary>
	/// <param name="title">
	/// Title of the file panel.
	/// </param>
	/// <param name="extension">
	/// Extension to look for.
	/// </param>
	/// <param name="asset">
	/// Asset used to set the initial folder.
	/// </param>
	/// <returns>
	/// Selected path, or an empty string if canceled.
	/// </returns>
	public static string OpenFilePanelInSameFolder(string title, string extension, Object asset){
		return EditorUtility.OpenFilePanel(title, new FileInfo(AssetDatabase.GetAssetPath(asset)).DirectoryName, extension);
	}
	
	/// <summary>
	/// Open a safe file panel in the same folder as some asset.
	/// Return the selected path or the empty string if canceled.
	/// </summary>
	/// <param name="title">
	/// Title of the file panel.
	/// </param>
	/// <param name="name">
	/// Default name of the file to save.
	/// <param name="extension">
	/// Extension of the file to save.
	/// </param>
	/// <param name="asset">
	/// Asset used to set the initial folder.
	/// </param>
	/// <returns>
	/// Selected path, or an empty string if canceled.
	/// </returns>
	public static string SaveFilePanelInSameFolder(string title, string name, string extension, Object asset){
		return EditorUtility.SaveFilePanel(title, new FileInfo(AssetDatabase.GetAssetPath(asset)).DirectoryName, name, extension);
	}
	
	#endregion
}
