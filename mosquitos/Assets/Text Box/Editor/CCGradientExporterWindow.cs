// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0

using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class CCGradientExporterWindow : EditorWindow {
	
	public static void OpenWindow () {
		EditorWindow.GetWindow<CCGradientExporterWindow>(true, "Gradient Exporter");
	}
	
	private static string
		minimumKey = "CCGradientExporterWindow.minimum",
		maximumKey = "CCGradientExporterWindow.maximum",
		pixelsKey = "CCGradientExporterWindow.pixels",
		bilinearPreviewKey = "CCGradientExporterWindow.bilinearPreview";
	
	private float minimum = 0f, maximum = 1f;
	private int pixels = 16;
	private CCGradient gradient;
	private Texture2D texture;
	private bool bilinearPreview;
	
	void OnEnable () {
		gradient = Selection.activeObject as CCGradient;
		
		minimum = EditorPrefs.GetFloat(minimumKey);
		maximum = EditorPrefs.GetFloat(maximumKey, 1f);
		pixels = EditorPrefs.GetInt(pixelsKey, 16);
		bilinearPreview = EditorPrefs.GetBool(bilinearPreviewKey, true);
		
		texture = new Texture2D(pixels, 1, TextureFormat.ARGB32, false);
		texture.hideFlags = HideFlags.HideAndDontSave;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = bilinearPreview ? FilterMode.Bilinear : FilterMode.Point;
		
		if(gradient != null){
			gradient.WriteToTexture(minimum, maximum, texture);
			texture.Apply();
		}
	}
	
	void OnDisable () {
		DestroyImmediate(texture);
	}
	
	void OnGUI () {
		CCGradient oldGradient = gradient;
		gradient = (CCGradient)EditorGUILayout.ObjectField("Gradient", gradient, typeof(CCGradient), false);
		bool updateTexture = gradient != oldGradient;
		
		if(gradient == null){
			return;
		}
		
		float oldValue = minimum;
		minimum = EditorGUILayout.FloatField("Minimum", minimum);
		if(minimum != oldValue){
			EditorPrefs.SetFloat(minimumKey, minimum);
			updateTexture = true;
		}
		oldValue = maximum;
		maximum = EditorGUILayout.FloatField("Maximum", maximum);
		if(maximum != oldValue){
			EditorPrefs.SetFloat(maximumKey, maximum);
			updateTexture = true;
		}
		
		int oldPixels = pixels;
		pixels = Mathf.Max(1, EditorGUILayout.IntField("Pixels", pixels));
		if(pixels != oldPixels){
			texture.Resize(pixels, 1);
			EditorPrefs.SetInt(pixelsKey, pixels);
			updateTexture = true;
		}
		
		bool oldToggle = bilinearPreview;
		bilinearPreview = EditorGUILayout.Toggle("Bilinear Preview", bilinearPreview);
		if(bilinearPreview != oldToggle){
			EditorPrefs.SetBool(bilinearPreviewKey, bilinearPreview);
			texture.filterMode = bilinearPreview ? FilterMode.Bilinear : FilterMode.Point;
		}
		
		if(updateTexture){
			gradient.WriteToTexture(minimum, maximum, texture);
			texture.Apply();
		}
		
		if(GUILayout.Button("Export PNG file")){
			string filePath = CCEditorUtility.SaveFilePanelInSameFolder("Save Color Gradient", gradient.name + " map", "png", gradient);
			if(filePath.Length > 0){
				File.WriteAllBytes(filePath, texture.EncodeToPNG());
				AssetDatabase.Refresh();
				Close();
			}
		}
		CCEditorUtility.DrawTexture(new Rect(2f, 130f, position.width - 4f, position.height - 132f), texture);
	}
}
