// Copyright 2012, Catlike Coding
// http://catlikecoding.com
// Version 1.0

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(CCFont))]
public sealed class CCFontInspector : Editor {
	
	private CCFont font;
	
	void OnEnable () {
		font = (CCFont)target;
	}
	
	[MenuItem("Assets/Create/Text Box/Font", false, 1000)]
	static void CreateSpriteFont () {
		CCEditorUtility.CreateAsset<CCFont>("New Font");
	}
	
	public override void OnInspectorGUI () {
		if(CCEditorUtility.UndoRedoEventHappened){
			// update any text boxes currently using the font
			font.UpdateAllCCText();
		}
		
		if(font.IsValid){
			EditorGUILayout.LabelField("Non-ASCII Chars", font.otherChars.Length.ToString());
			EditorGUILayout.LabelField("Pixel Line Height", font.pixelLineHeight.ToString());
			EditorGUILayout.LabelField("Pixel Scale", font.pixelScale.ToString());
			EditorGUILayout.LabelField("Supports Kerning", font.supportsKerning.ToString());
		}
		else{
			GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
			labelStyle.wordWrap = true;
			GUILayout.Label("Please import a BMFont compatible font specification. Text and XML formats are supported.", labelStyle);
		}
		
		if(GUILayout.Button("Import FNT file")){
			string filePath = CCEditorUtility.OpenFilePanelInSameFolder("Open FNT file", "fnt", font);
			if(filePath.Length == 0){
				return;
			}
			Undo.SetSnapshotTarget(font, "Import FNT file");
			Undo.CreateSnapshot();
			try{
				Import(filePath);
			}
			catch(Exception e){
				Undo.RestoreSnapshot();
				Debug.LogError("Failed to import FNT file. See next error for details.");
				throw e;
			}
			Undo.RegisterSnapshot();
			EditorUtility.SetDirty(font);
			// update any text boxes currently using the font
			font.UpdateAllCCText();
		}
	}
	
	#region Import
	
	private static char[] separators = {' ', '=', '"', '<', '>', '/', '\t'};
	private static string[] lines;
	private static Dictionary<string, int> currentLine;
	private static string currentLineType;
	private static int currentLineIndex;
	
	private void Import(string filePath) {
		lines = File.ReadAllLines(filePath);
		currentLine = new Dictionary<string, int>();
		currentLineIndex = 0;
		do{
			ReadNextLine();
		}
		while(currentLineType != null && currentLineType != "common");
		
		int
			pixelLineHeight = currentLine["lineHeight"],
			width = currentLine["scaleW"],
			height = currentLine["scaleH"];
		
		float
			baseline = (float)currentLine["base"] / pixelLineHeight,
			pixelScale = 1f / pixelLineHeight,
			leftMargin = 0f,
			rightMargin = 0f,
			topMargin = 0f,
			bottomMargin = 0f;
		
		CCFont.Char[] asciiGlyphs = new CCFont.Char['~' - ' '];
		List<CCFont.Char> otherGlyphs = new List<CCFont.Char>();
		
		// read character data
		do{
			ReadNextLine();
		}
		while(currentLineType != null && currentLineType != "chars");
		for(int i = 0, l = currentLine["count"]; i < l; i++){
			CCFont.Char c = ParseGlyph(width, height, pixelScale);
			if(c.xOffset < leftMargin){
				leftMargin = c.xOffset;
			}
			if(c.xOffset + c.width - c.advance > rightMargin){
				rightMargin = c.xOffset + c.width - c.advance;
			}
			if(c.yOffset > topMargin){
				topMargin = c.yOffset;
			}
			if(c.height - c.yOffset > bottomMargin){
				bottomMargin = c.height - c.yOffset;
			}
			
			if(c.id == ' '){
				// set space advance
				font.spaceAdvance = c.advance;
			}
			else if('!' <= c.id && c.id <= '~') {
				asciiGlyphs[c.id - '!'] = c;
			}
			else if(c.id > 127){
				// add everything else that isn't a control character
				otherGlyphs.Add(c);
			}
		}
		
		// set missing ASCII characters
		CCFont.Char missing = new CCFont.Char();
		missing.kerningIds = new int[0];
		for(int i = 0; i < '~' - ' '; i++){
			if(asciiGlyphs[i] == null){
				asciiGlyphs[i] = missing;
			}
		}
		
		// set font data
		font.pixelLineHeight = pixelLineHeight;
		font.baseline = baseline;
		font.pixelScale = pixelScale;
		font.leftMargin = -leftMargin;
		font.rightMargin = rightMargin;
		font.topMargin = topMargin;
		font.bottomMargin = bottomMargin - 1f;
		font.asciiChars = asciiGlyphs;
		font.otherChars = otherGlyphs.ToArray();
		Array.Sort(font.otherChars, new CharComparer());
		
		// add kernings
		do{
			ReadNextLine();
		}
		while(currentLineType != null && currentLineType != "kernings");
		if(currentLineType != null && currentLine["count"] > 0){
			font.supportsKerning = true;
			int l = currentLine["count"];
			ReadNextLine();
			int currentCharId = currentLine["first"];
			List<int> kerningIds = new List<int>();
			List<float> kernings = new List<float>();
			for(int i = 0; i < l; i++){
				if(currentCharId != currentLine["first"]){
					CCFont.Char c = font[(char)currentCharId];
					c.kerningIds = kerningIds.ToArray();
					c.kernings = kernings.ToArray();
					Array.Sort(c.kerningIds, c.kernings);
					kernings.Clear();
					kerningIds.Clear();
					currentCharId = currentLine["first"];
				}
				kerningIds.Add(currentLine["second"]);
				kernings.Add(currentLine["amount"] * pixelScale);
				ReadNextLine();
			}
		}
		else{
			font.supportsKerning = false;
		}
		
		currentLine = null;
	}
	
	private static void ReadNextLine () {
		string[] data;
		do{
			if(currentLineIndex >= lines.Length){
				currentLineType = null;
				return;
			}
			// extract array of words from text or XML line
			data = lines[currentLineIndex++].Split(separators, StringSplitOptions.RemoveEmptyEntries);
		}
		while(data.Length == 0);
		currentLineType = data[0];
		int value = 0;
		// store line data in dictionary
		for(int i = 2; i < data.Length; i += 2){
			if(int.TryParse(data[i], out value)){
				currentLine[data[i - 1]] = value;
			}
		}
	}
	
	private static CCFont.Char ParseGlyph (int width, int height, float scale) {
		ReadNextLine();
		CCFont.Char c = new CCFont.Char();
		c.kerningIds = new int[0];
		c.id = currentLine["id"];
		float
			w = currentLine["width"],
			h = currentLine["height"];
		
		c.uMin = (currentLine["x"] + 0.5f) / width;
		c.uMax = c.uMin + w / width;
		c.vMin = (height - h - currentLine["y"] - 0.5f) / height;
		c.vMax = c.vMin + h / height;
		
		c.xOffset = currentLine["xoffset"] * scale;
		c.width = w * scale;
		c.yOffset = -currentLine["yoffset"] * scale;
		c.height = h * scale;
		
		c.advance = currentLine["xadvance"] * scale;
		
		return c;
	}
	
	// comparer used to sort non-ASCII character data
	private class CharComparer : IComparer<CCFont.Char> {
		public int Compare (CCFont.Char x, CCFont.Char y) {
			if(x.id < y.id){
				return -1;
			}
			if(x.id > y.id){
				return 1;
			}
			return 0;
		}
	}
	
	#endregion
	
}
