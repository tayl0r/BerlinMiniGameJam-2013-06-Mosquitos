// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0

using System;
using System.Collections;
using System.Text;
using UnityEngine;

// controller for the demo's GUI
public class GUIController : MonoBehaviour {
	
	#region Types

	// convenience class for storing font data
	[Serializable]
	public class FontPackage {
		public CCFont font;
		public Texture2D atlas, distanceMap;
	}
	
	#endregion
	
	#region Constants and Variables
	
	private const float rotationLimit = 60f;
	
	// convenience consts for identifying selections
	private const int
		FoxDog = 0,
		LoremIpsum = 1,
		Timer = 2,
		Table = 3,
		
		AlphaBlend = 0,
		AlphaTest = 1,
		Smooth = 2,
		SmoothFade = 3,
		SmoothOutline = 4,
		SmoothShadow = 5,
		Gradient = 6;
	
	// option labels
	public static string[]
		contentOptions = {
			"Fox",
			"Lorem",
			"Timer",
			"Table"
		},
	
		alignmentOptions = {
			"L", "C", "R", "J"
		},
	
		modifierOptions = {
			"None",
			"[marked]",
			"Vertical",
			"a-z"
		},
	
		shaderOptions = {
			"Alpha Blend",
			"Alpha Test",
			"Smooth",
			"Smooth Fade",
			"Smooth Outline",
			"Smooth Shadow",
			"Gradient"
		};
	
	public CCText text;
	public FontPackage[] fontPackages;
	public CCTextModifier[] modifiers;
	public Shader[] shaders;
	public CCGradient[] gradients;
	
	private int
		selectedContent,
		selectedAlignment,
		selectedFont,
		selectedShader,
		selectedGradient,
		selectedModifier;
	private bool useDistanceMap;
	private float
		scale,
		width;
	private string[] fontOptions;
	private Texture2D[] gradientMaps;
	private Material textMaterial;
	private Transform textTransform;
	private Vector3 rotation;
	private StringBuilder stringBuilder;
	
	private float
		alphaBoundary = 0.5f,
		edgeMin = 0.45f,
		edgeMax = 0.5f,
		outlineMin = 0.35f,
		outlineMax = 0.4f,
		shadowMin = 0.3f,
		shadowMax = 0.5f,
		shadowOffsetU = -0.005f,
		shadowOffsetV = 0.005f,
		fadeDistance = 10f,
		fadeStrength = 1f;
	
	#endregion
	
	#region Unity Events
	
	void Start () {
		textMaterial = text.renderer.material;
		textTransform = text.transform;
		stringBuilder = new StringBuilder();
		selectedContent = FoxDog;
		selectedShader = Smooth;
		scale = 1f;
		useDistanceMap = true;
		
		// generate gradient maps
		gradientMaps = new Texture2D[gradients.Length];
		for(int i = 0; i < gradients.Length; i++){
			Texture2D t = new Texture2D(32, 8);
			t.filterMode = FilterMode.Trilinear;
			t.wrapMode = TextureWrapMode.Clamp;
			gradients[i].WriteToTexture(0f, 1f, t);
			
			// add lines to make gradient clearly visible in GUI
			for(int p = 0; p < 32; p++){
				Color c = t.GetPixel(p, 0);
				t.SetPixel(p, 1, c);
				t.SetPixel(p, 2, c);
				t.SetPixel(p, 3, c);
				t.SetPixel(p, 4, c);
				t.SetPixel(p, 5, c);
				t.SetPixel(p, 6, c);
				t.SetPixel(p, 7, c);
			}
			t.Apply(true, true);
			gradientMaps[i] = t;
		}
		
		// fit text between GUI
		text.Width = width = -2f * Camera.main.ScreenToWorldPoint(new Vector3(145f, 0f, -Camera.main.transform.localPosition.z)).x;
		
		// setup font options
		fontOptions = new string[fontPackages.Length];
		for(int i = 0; i < fontOptions.Length; i++){
			fontOptions[i] = fontPackages[i].font.name;
		}
		if(fontOptions.Length > 0){
			UpdateFont();
		}
		else{
			Debug.Log("Please add your own fonts to the demo.");
		}
		
		UpdateContent();
	}
	
	void OnGUI () {
		// left GUI, content area
		GUILayout.BeginArea(new Rect(2f, 2f, 135f, Screen.height - 4f), GUI.skin.box);
		
		GUILayout.Label("Content");
		int oldSelection = selectedContent;
		selectedContent = GUILayout.SelectionGrid(selectedContent, contentOptions, 2);
		if(selectedContent != oldSelection){
			UpdateContent();
		}
		GUILayout.Label("Font");
		oldSelection = selectedFont;
		selectedFont = GUILayout.SelectionGrid(selectedFont, fontOptions, 1);
		if(selectedFont != oldSelection){
			UpdateFont();
		}
		GUILayout.Label("Alignment");
		oldSelection = selectedAlignment;
		selectedAlignment = GUILayout.SelectionGrid(selectedAlignment, alignmentOptions, 4);
		if(selectedAlignment != oldSelection){
			text.Alignment = (CCText.AlignmentMode)selectedAlignment;
		}
		GUILayout.Label("Modifier");
		oldSelection = selectedModifier;
		selectedModifier = GUILayout.SelectionGrid(selectedModifier, modifierOptions, 2);
		if(selectedModifier != oldSelection){
			text.Modifier = modifiers[selectedModifier];
		}
		
		GUILayout.Label("Scale");
		float oldScale = scale;
		scale = GUILayout.HorizontalSlider(scale, 0.2f, 4f);
		if(scale != oldScale){
			textTransform.localScale = Vector3.one * scale;
			text.Width = width / scale;
		}
		
		GUILayout.Label("Rotation X/Y/Z");
		Vector3 oldRotation = rotation;
		GUILayout.BeginHorizontal();
		rotation.x = GUILayout.HorizontalSlider(rotation.x, -rotationLimit, rotationLimit);
		rotation.y = GUILayout.HorizontalSlider(rotation.y, -rotationLimit, rotationLimit);
		rotation.z = GUILayout.HorizontalSlider(rotation.z, -rotationLimit, rotationLimit);
		GUILayout.EndHorizontal();
		if(rotation != oldRotation){
			textTransform.eulerAngles = rotation;
		}
		
		GUILayout.EndArea();
		
		// right GUI, shader area
		GUILayout.BeginArea(new Rect(Screen.width - 137f, 2f, 135f, Screen.height - 4f), GUI.skin.box);
		
		GUILayout.Label("Shader");
		oldSelection = selectedShader;
		selectedShader = GUILayout.SelectionGrid(selectedShader, shaderOptions, 1);
		if(selectedShader != oldSelection){
			UpdateShader();
		}
		bool oldToggle = useDistanceMap;
		useDistanceMap = GUILayout.Toggle(useDistanceMap, "Use Distance Map");
		if(useDistanceMap != oldToggle){
			UpdateFont();
		}
		
		switch(selectedShader){
		case AlphaTest:
			GUILayout.Label("Alpha Boundary");
			alphaBoundary = ShaderFloat("_AlphaBoundary", alphaBoundary);
			break;
		case Smooth:
			GUILayout.Label("Edge Min/Max");
			edgeMin = ShaderFloat("_EdgeMin", edgeMin);
			edgeMax = ShaderFloat("_EdgeMax", edgeMax);
			break;
		case SmoothFade:
			GUILayout.Label("Edge Min/Max");
			edgeMin = ShaderFloat("_EdgeMin", edgeMin);
			edgeMax = ShaderFloat("_EdgeMax", edgeMax);
			GUILayout.Label("Fade Distance/Factor");
			fadeDistance = ShaderFloat("_FadeDistance", fadeDistance, 0f, 20f);
			fadeStrength = ShaderFloat("_FadeStrength", fadeStrength, 0f, 1f);
			break;
		case SmoothOutline:
			GUILayout.Label("Edge Min/Max");
			edgeMin = ShaderFloat("_EdgeMin", edgeMin);
			edgeMax = ShaderFloat("_EdgeMax", edgeMax);
			GUILayout.Label("Outline Min/Max");
			outlineMin = ShaderFloat("_OutlineMin", outlineMin);
			outlineMax = ShaderFloat("_OutlineMax", outlineMax);
			break;
		case SmoothShadow:
			GUILayout.Label("Edge Min/Max");
			edgeMin = ShaderFloat("_EdgeMin", edgeMin);
			edgeMax = ShaderFloat("_EdgeMax", edgeMax);
			GUILayout.Label("Shadow Min/Max");
			shadowMin = ShaderFloat("_ShadowMin", shadowMin);
			shadowMax = ShaderFloat("_ShadowMax", shadowMax);
			GUILayout.Label("Shadow Offset U/V");
			shadowOffsetU = ShaderFloat("_ShadowOffsetU", shadowOffsetU, -0.01f, 0.01f);
			shadowOffsetV = ShaderFloat("_ShadowOffsetV", shadowOffsetV, -0.01f, 0.01f);
			break;
		case Gradient:
			GUILayout.Label("Gradients");
			oldSelection = selectedGradient;
			selectedGradient = GUILayout.SelectionGrid(selectedGradient, gradientMaps, 2);
			if(selectedGradient != oldSelection){
				textMaterial.SetTexture("_Gradient", gradientMaps[selectedGradient]);
			}
			break;
		}
		
		GUILayout.EndArea();
	}
	
	void Update () {
		if(selectedContent == Timer){
			stringBuilder.Length = 25;
			CCStringBuilderUtility.AppendFloatGrouped(stringBuilder, Time.realtimeSinceStartup * 1000f, 2, 7);
			stringBuilder.Append("\nTimes a wastin!");
			text.UpdateText();
		}
	}
	
	#endregion
	
	#region Private Helper Methods
	
	private float ShaderFloat (string name, float value) {
		float newValue = GUILayout.HorizontalSlider(value, 0f, 1f);
		if(newValue != value){
			textMaterial.SetFloat(name, newValue);
		}
		return newValue;
	}
	
	private float ShaderFloat (string name, float value, float min, float max) {
		float newValue = GUILayout.HorizontalSlider(value, min, max);
		if(newValue != value){
			textMaterial.SetFloat(name, newValue);
		}
		return newValue;
	}
	
	private void UpdateContent () {
		switch(selectedContent){
		case FoxDog:
			text.Text = "The quick brown fox jumped over a lazy dog.\n\nThe dog [did not] fuss about it.\n\nThe fox thought it was [pretty cool].";
			text.StringBuilder = null;
			break;
		case LoremIpsum:
			text.Text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
			text.StringBuilder = null;
			break;
		case Timer:
			stringBuilder.Length = 0;
			text.StringBuilder = stringBuilder;
			stringBuilder.Append("Milliseconds since load:\n");
			break;
		case Table:
			text.Text = "\tA\tB\tC\nA\t1\t22\t333\nB\t22\t333\t4444\nC\t333\t4444\t55555";
			text.StringBuilder = null;
			break;
		}
	}
	
	private void UpdateFont () {
		FontPackage p = fontPackages[selectedFont];
		textMaterial.mainTexture = useDistanceMap ? p.distanceMap : p.atlas;
		text.Font = p.font;
	}
	
	private void UpdateShader () {
		textMaterial.shader = shaders[selectedShader];
		if(selectedShader == Gradient){
			textMaterial.SetTexture("_Gradient", gradientMaps[selectedGradient]);
		}
	}
	
	#endregion
}
