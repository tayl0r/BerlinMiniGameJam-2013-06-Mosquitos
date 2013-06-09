// Copyright 2012, Catlike Coding
// http://catlikecoding.com/
// Version 1.0

using UnityEngine;

/// <summary>
/// Data for a bitmap sprite font.
/// Supports all non-control characters in the Basic Multilingual Plane.
/// </summary>
public sealed class CCFont : ScriptableObject {
	
	#region Types
	
	/// <summary>
	/// Data for a single character of a bitmap sprite font.
	/// </summary>
	[System.Serializable]
	public class Char {
		
		/// <summary>
		/// Identifier, which is equal to the UTF-16 code of the represented character.
		/// </summary>
		public int id;
		
		/// <summary>
		/// Minimum U texture coordinate value.
		/// </summary>
		public float uMin;
		
		/// <summary>
		/// Maximum U texture coordinate value.
		/// </summary>
		public float uMax;
		
		/// <summary>
		/// Minimum V texture coordinate value.
		/// </summary>
		public float vMin;
		
		/// <summary>
		/// Maximum V texture coordinate value.
		/// </summary>
		public float vMax;
		
		/// <summary>
		/// Sprite X offset relative to caret.
		/// </summary>
		public float xOffset;
		
		/// <summary>
		/// Sprite Y offset relative to caret.
		/// </summary>
		public float yOffset;
		
		/// <summary>
		/// Sprite width.
		/// </summary>
		public float width;
		
		/// <summary>
		/// Sprite height.
		/// </summary>
		public float height;
		
		/// <summary>
		/// Amount to advance the caret.
		/// </summary>
		public float advance;
		
		/// <summary>
		/// Identifiers of characters this character has kernings for.
		/// </summary>
		public int[] kerningIds;
		
		/// <summary>
		/// Kerning amounts, corresponding to kerningIds.
		/// </summary>
		public float[] kernings;
		
		/// <summary>
		/// Compute how much to advance the caret, taking kerning into account.
		/// </summary>
		/// <param name="nextChar">
		/// The character to compute kerning for.
		/// </param>
		/// <returns>
		/// How much to advance the caret.
		/// </returns>
		public float AdvanceWithKerning(char nextChar){
			if(kerningIds.Length == 0){
				return advance;
			}
			// binary search the kernings
			int
				min = 0,
				max = kerningIds.Length - 1;
			while(max >= min){
				int midpoint = (min + max) >> 1;
				int id = kerningIds[midpoint];
				if(id > nextChar){
					max = midpoint - 1;
				}
				else if(id < nextChar){
					min = midpoint + 1;
				}
				else{
					return advance + kernings[midpoint];
				}
			}
			return advance;
		}
	}
	
	#endregion
	
	#region Public Variables
	
	/// <summary>
	/// ASCII characters, from '!' to '~'.
	/// Characters missing from the font have zeros for all their values.
	/// </summary>
	public Char[] asciiChars;
	
	/// <summary>
	/// All non-ASCII characters included in the font.
	/// </summary>
	public Char[] otherChars;
	
	/// <summary>
	/// Original line height in pixels.
	/// </summary>
	public int pixelLineHeight;
	
	/// <summary>
	/// How large a pixel is, in local space.
	/// Equal to 1f / pixelLineHeight.
	/// </summary>
	public float pixelScale;
	
	/// <summary>
	/// Offset of the baseline from the top of the line.
	/// </summary>
	public float baseline;
	
	/// <summary>
	/// Amount to advance the caret for a space.
	/// </summary>
	public float spaceAdvance;
	
	/// <summary>
	/// Whether kerning is supported by the font.
	/// </summary>
	public bool supportsKerning;

	/// <summary>
	/// Maximum distance that spites extend to the left of the caret.
	/// </summary>
	public float leftMargin;
	
	/// <summary>
	/// Maximum distance that spites extend to the right of the caret.
	/// </summary>
	public float rightMargin;
	
	/// <summary>
	/// Maximum distance that spites extend above the line.
	/// </summary>
	public float topMargin;
	
	/// <summary>
	/// Maximum distance that spites extend below the line.
	/// </summary>
	public float bottomMargin;
	
	#endregion
	
	#region Properties
	
	/// <summary>
	/// Get the data for a character in this font. Missing characters have 0s for all values.
	/// </summary>
	/// <param name="c">
	/// A character.
	/// </param>
	public Char this[char c] {
		get {
			if('!' <= c && c <= '~'){
				return asciiChars[c - '!'];
			}
			// binary search the other glyphs
			int
				min = 0,
				max = otherChars.Length - 1;
			while(max >= min){
				int midpoint = (min + max) >> 1;
				int id = otherChars[midpoint].id;
				if(id > c){
					max = midpoint - 1;
				}
				else if(id < c){
					min = midpoint + 1;
				}
				else{
					return otherChars[midpoint];
				}
			}
			if(missing == null){
				missing = new Char();
				missing.kerningIds = new int[0];
			}
			return missing;
		}
	}
	
	/// <summary>
	/// Get whether this is a valid imported font.
	/// </summary>
	public bool IsValid {
		get {
			return pixelLineHeight != 0;
		}
	}

	#endregion
	
	#region Public Methods
	
	/// <summary>
	/// Update all CCText components in the scene that use this font.
	/// This method is expensive and creates a temporary array.
	/// </summary>
	public void UpdateAllCCText () {
		CCText[] boxes = (CCText[])FindObjectsOfType(typeof(CCText));
		for(int i = 0; i < boxes.Length; i++){
			boxes[i].UpdateText();
		}
	}
	
	#endregion
	
	#region Private Stuff
	
	// dummy for missing non-ASCII characters, created on demand
	private Char missing;
	
	// prevent direct creation via constructor
	private CCFont () {}
	
	#endregion
	
}
