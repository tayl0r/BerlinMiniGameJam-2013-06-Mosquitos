/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.3
	
	1.0.3: Now works with pre-release Flash publishing.
	1.0.2: Added MeshCollider support. Improved WYSIWYG support. LineHeight and TabSize no longer allow negative values.
	1.0.0: Initial version.
*/

using System;
using System.Text;
using UnityEngine;

/// <summary>
/// Component that uses a mesh to display a box of text.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), ExecuteInEditMode]
public sealed class CCText : MonoBehaviour {
	
	private static Vector3 zeroVector3 = Vector3.zero;
	
	#region Types
	
	/// <summary>
	/// How to align text.
	/// </summary>
	public enum AlignmentMode {
		/// <summary>
		/// Align text to the left.
		/// </summary>
		Left,
		/// <summary>
		/// Align text in the center.
		/// </summary>
		Center,
		/// <summary>
		/// Align text to the right.
		/// </summary>
		Right,
		/// <summary>
		/// Justify text, stretching word-wrapped lines to fill the entire width.
		/// </summary>
		Justify
	}
	
	/// <summary>
	/// How to bound text.
	/// </summary>
	public enum BoundingMode {
		/// <summary>
		/// Constrain the caret, allow text to go beyond the bounds.
		/// </summary>
		Caret,
		/// <summary>
		/// Constrain text so it never goes beyond the bounds.
		/// </summary>
		Margin
	}
	
	/// <summary>
	/// How to anchor text horizontally.
	/// </summary>
	public enum HorizontalAnchorMode {
		/// <summary>
		/// Anchor at the left.
		/// </summary>
		Left,
		/// <summary>
		/// Anchor at the center.
		/// </summary>
		Center,
		/// <summary>
		/// Anchor at the right.
		/// </summary>
		Right
	}
	
	/// <summary>
	/// How to anchor text vertically.
	/// </summary>
	public enum VerticalAnchorMode {
		/// <summary>
		/// Anchor at the top.
		/// </summary>
		Top,
		/// <summary>
		/// Anchor at the middle.
		/// </summary>
		Middle,
		/// <summary>
		/// Anchor at the bottom.
		/// </summary>
		Bottom,
		/// <summary>
		/// Anchor at the baseline of the first line.
		/// </summary>
		Baseline
	}
	
	#endregion
	
	#region Properties
	
	/// <summary>
	/// Get a character from the currently displayed text at some position. Uses StringBuilder if assigned, otherwise Text.
	/// </summary>
	/// <param name="index">
	/// Character index.
	/// </param>
	public char this[int index] {
		get {
			return stringBuilder != null ? StringBuilder[index] : text[index];
		}
	}
	
	[SerializeField]
	private AlignmentMode alignment;
	/// <summary>
	/// Get or set the text alignment.
	/// Setting a different value triggers an update.
	/// </summary>
	public AlignmentMode Alignment {
		get {
			return alignment;
		}
		set {
			if(alignment != value){
				alignment = value;
				UpdateText();
			}
		}
	}
	
	[SerializeField]
	private BoundingMode bounding;
	/// <summary>
	/// Get or set the bounding mode.
	/// Setting a different value triggers an update.
	/// </summary>
	public BoundingMode Bounding {
		get {
			return bounding;
		}
		set {
			if(bounding != value){
				bounding = value;
				UpdateText();
			}
		}
	}
	
	[SerializeField]
	private HorizontalAnchorMode horizontalAnchor;
	/// <summary>
	/// Get or set the horizontal anchor mode.
	/// Setting to a different value triggers an update.
	/// </summary>
	public HorizontalAnchorMode HorizontalAnchor {
		get {
			return horizontalAnchor;
		}
		set {
			if(horizontalAnchor != value){
				horizontalAnchor = value;
				UpdateText();
			}
		}
	}
	
	[SerializeField]
	private VerticalAnchorMode verticalAnchor;
	/// <summary>
	/// Get or set the vertical anchor mode.
	/// Setting a different value triggers an update.
	/// </summary>
	public VerticalAnchorMode VerticalAnchor {
		get {
			return verticalAnchor;
		}
		set {
			if(verticalAnchor != value){
				verticalAnchor = value;
				UpdateText();
			}
		}
	}
	
	/// <summary>
	/// Get the minimum bounds of the caret based on minBounds and font borders, in local space units.
	/// Will be incorrect if a modifier changed minBounds.
	/// </summary>
	public Vector3 CaretMinBounds {
		get {
			if(font == null){
				return minBounds;
			}
			Vector3 b = minBounds;
			b.x += font.leftMargin;
			b.y += font.bottomMargin;
			return b;
		}
	}

	/// <summary>
	/// Get the maximum bounds of the caret based on minBounds and font borders, in local space units.
	/// Will be incorrect if a modifier changed minBounds.
	/// </summary>
	public Vector3 CaretMaxBounds {
		get {
			if(font == null){
				return maxBounds;
			}
			Vector3 b = maxBounds;
			b.x -= font.rightMargin;
			b.y -= font.topMargin;
			return b;
		}
	}
		
	[SerializeField]
	private int chunkSize = 1;
	/// <summary>
	/// Get or set the chunk sized used to increase SpriteCount, with a minimum of 1.
	/// </summary>
	public int ChunkSize {
		get {
			return chunkSize;
		}
		set {
			chunkSize = value < 1 ? 1 : value;
		}
	}
	
	[SerializeField]
	private Color color = Color.black;
	/// <summary>
	/// Get or set the color of the text.
	/// Setting a different value triggers a color reset and an update.
	/// </summary>
	public Color Color {
		get {
			return color;
		}
		set {
			if(color != value){
				color = value;
				ResetColors();
				UpdateText();
			}
		}
	}
	
	[SerializeField]
	private CCFont font;
	/// <summary>
	/// Get or set the font used to place text.
	/// No text is displayed if set to null or an invalid font.
	/// Setting a different value triggers an update.
	/// </summary>
	public CCFont Font {
		get {
			return font;
		}
		set {
			if(font != value && (value == null || value.IsValid)){
				font = value;
				UpdateText();
			}
		}
	}
		
	/// <summary>
	/// Get the length of the text currently being displayed. Uses StringBuilder if assigned, otherwise Text.
	/// </summary>
	public int Length {
		get {
			return stringBuilder != null ? stringBuilder.Length : text.Length;
		}
	}
	
	private int lineCount;
	/// <summary>
	/// Get the number of text lines currently being displayed.
	/// </summary>
	public int LineCount {
		get {
			return lineCount;
		}
	}

	[SerializeField]
	private float lineHeight = 1f;
	/// <summary>
	/// Get or set the line height, in local space units, with a minimum of 0.
	/// A height of 1 corresponds to the font's default line height.
	/// Setting a different value triggers an update.
	/// </summary>
	public float LineHeight {
		get {
			return lineHeight;
		}
		set {
			if(lineHeight != value){
				lineHeight = value < 0f ? 0f : value;
				UpdateText();
			}
		}
	}
	
	private float lineWidth;
	/// <summary>
	/// Get the line width to which the caret is constrained.
	/// Might be the same as Width, depending on Width, Bounding, and Font.
	/// </summary>
	public float LineWidth {
		get {
			return lineWidth;
		}
	}
	
	[SerializeField]
	private CCTextModifier modifier;
	/// <summary>
	/// Get or set the modifier used to alter the text sprites. Can be null.
	/// Setting a different value triggers an update.
	/// </summary>
	public CCTextModifier Modifier {
		get {
			return modifier;
		}
		set {
			if(modifier != value){
				modifier = value;
				// reset colors, previous modifier most likely tweaked them
				ResetColors();
				UpdateText();
			}
		}
	}

	[SerializeField]
	private Vector3 offset;
	/// <summary>
	/// Get or set the text offset relative to the anchor.
	/// Setting a different value triggers an update.
	/// </summary>
	public Vector3 Offset {
		get {
			return offset;
		}
		set {
			if(offset != value){
				offset = value;
				UpdateText();
			}
		}
	}
	
	private int spriteCount;
	/// <summary>
	/// Get how many sprites are currently allocated.
	/// </summary>
	public int SpriteCount {
		get {
			return spriteCount;
		}
	}
	
	private int usedSpriteCount;
	/// <summary>
	/// Get how many sprites are currently used to display characters.
	/// </summary>
	public int UsedSpriteCount {
		get {
			return usedSpriteCount;
		}
	}

	private StringBuilder stringBuilder;
	/// <summary>
	/// Get or set the StringBuilder used to display text. Setting a different value triggers an update.
	/// Modifying the StringBuilder object itself does not trigger an update. If not null, will be used
	/// instead of Text.
	/// </summary>
	public StringBuilder StringBuilder {
		get {
			return stringBuilder;
		}
		set {
			if(stringBuilder != value){
				stringBuilder = value;
				UpdateText();
			}
		}
	}
	
	[SerializeField]
	private string text = string.Empty;
	/// <summary>
	/// Get or set the string used to display text. Setting always triggers an update. Setting to null will result in the empty string.
	/// Will be ignored if StringBuilder is not null.
	/// </summary>
	public string Text {
		get {
			return text;
		}
		set {
			text = value == null ? string.Empty : value;
			UpdateText();
		}
	}
	
	[SerializeField]
	private float tabSize = 2f;
	/// <summary>
	/// Get or set the tab size, in local space units, with a minimum of 0.001.
	/// Setting a different value triggers an update.
	/// </summary>
	public float TabSize {
		get {
			return tabSize;
		}
		set {
			if(tabSize != value){
				tabSize = value < 0.001f ? 0.001f : value;
				UpdateText();
			}
		}
	}
	
	[SerializeField]
	private float width = 10f;
	/// <summary>
	/// Get or set the width of the box containing the text, with a minimum of 0.
	/// Setting a different value triggers an update.
	/// </summary>
	public float Width {
		get {
			return width;
		}
		set {
			if(width != value){
				width = value < 0f ? 0f : value;
				UpdateText();
			}
		}
	}
	
	#endregion
	
	#region Public Variables
	
	/// <summary>
	/// Minimum bounds coordinates used for bounding the mesh, in local space units.
	/// Modifiers might want change this if they update vertices.
	/// </summary>
	[NonSerialized]
	public Vector3 minBounds;
	
	/// <summary>
	/// Maximum bounds coordinates used for bounding the mesh, in local space units.
	/// Modifiers might want change this if they update vertices.
	/// </summary>
	[NonSerialized]
	public Vector3 maxBounds;
	
	/// <summary>
	/// Mesh used to display the text.
	/// Modifiers might want to access it.
	/// </summary>
	[NonSerialized]
	public Mesh mesh;
	
	/// <summary>
	/// Vertices used to position character sprites.
	/// Modifiers might want to access these.
	/// Will be pushed to the mesh after the modifier has been called.
	/// </summary>
	[NonSerialized]
	public Vector3[] vertices;
	
	/// <summary>
	/// Colors used to color character sprites.
	/// Modifiers might want to access these.
	/// Will not be pushed to the mesh by default, that's up to the modifier.
	/// </summary>
	[NonSerialized]
	public Color[] colors;
	
	/// <summary>
	/// UV coordinates used to texture the character sprites.
	/// Modifiers might want to access these.
	/// Will be pushed to the mesh after the modifier has been called.
	/// </summary>
	[NonSerialized]
	public Vector2[] uv;
	
	/// <summary>
	/// Triangles used to define character sprites.
	/// Modifiers might want to access these, but probably not.
	/// Will not be pushed to the mesh by default.
	/// </summary>
	[NonSerialized]
	public int[] triangles;
	
	#endregion
	
	#region Unity Events
	
	void Awake () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "CCText Mesh";
		mesh.hideFlags = HideFlags.HideAndDontSave;
		meshCollider = GetComponent<MeshCollider>();
	}
	
	void Start () {
		// update unless something already triggered an update
		if(vertices == null){
			UpdateText();
		}
	}
	
#if UNITY_EDITOR
	
	void OnEnable () {
		spriteCount = 0;
		usedSpriteCount = 0;
		UpdateText();
	}
	
	void OnDisable () {
		GetComponent<MeshFilter>().mesh = null;
		DestroyImmediate(mesh);
	}
	
	void Reset () {
		UpdateText();
	}
	
#endif
	
	#endregion
	
	#region Public Methods
	
	/// <summary>
	/// Return the index of a character based on a triangle index. You can use this to detect which character was hit.
	/// </summary>
	/// <returns>
	/// A character index.
	/// </returns>
	/// <param name='triangleIndex'>
	/// A triangle index.
	/// </param>
	public int TriangleToCharacterIndex (int triangleIndex) {
		for(int i = 0, t = 0, l = Length; i < l; i++){
			if(Text[i] <= ' '){
				continue;
			}
			if(t == triangleIndex || t + 1 == triangleIndex){
				return i;
			}
			t += 2;
		}
		return -1;
	}
	
	/// <summary>
	/// Return the index of a hit character, or -1 if nothing was hit.
	/// </summary>
	/// <returns>
	/// A character index.
	/// </returns>
	/// <param name='hit'>
	/// A hit.
	/// </param>
	public int HitCharacterIndex (RaycastHit hit) {
		if(hit.collider == meshCollider){
			return TriangleToCharacterIndex(hit.triangleIndex);
		}
		return -1;
	}
	
	/// <summary>
	/// Reset the colors of all character sprites.
	/// By default colors aren't updated.
	/// </summary>
	public void ResetColors () {
		if(colors == null){
			return;
		}
		for(int i = 0, l = colors.Length; i < l; i += 4){
			colors[i] = color;
			colors[i + 1] = color;
			colors[i + 2] = color;
			colors[i + 3] = color;
		}
		mesh.colors = colors;
	}
	
	/// <summary>
	/// Update the text mesh. Is called automatically when setting most properties.
	/// Typically, you only call this yourself after modifying the contents of a StringBuilder.
	/// </summary>
	public void UpdateText () {
#if UNITY_EDITOR
		if(mesh == null){
			GetComponent<MeshFilter>().mesh = mesh = new Mesh();
			mesh.name = "CCText Mesh";
			mesh.hideFlags = HideFlags.HideAndDontSave;
			meshCollider = GetComponent<MeshCollider>();
		}
#endif
		
		if(font == null){
			if(vertices != null){
				HideSprites(0);
				mesh.vertices = vertices;
			}
			return;
		}
		
		Vector3 anchor = offset;
		
		// set line width based on bounding mode
		if(bounding == BoundingMode.Caret){
			lineWidth = width;
		}
		else{
			lineWidth = width - font.leftMargin - font.rightMargin;
		}
		
		// update text
		if(stringBuilder != null){
			UpdateFromStringBuilder();
		}
		else{
			UpdateFromString();
		}
		
		if(vertices == null){
			return;
		}
		
		// align if needed
		if(alignment != AlignmentMode.Left){
			if(alignment == AlignmentMode.Justify){
				JustifyChars();
			}
			else{
				AlignCharsCenterOrRight();
			}
		}
		
		// define bounds and place anchor based on bounding and anchor modes
		if(bounding == BoundingMode.Caret){
			switch(horizontalAnchor){
			case HorizontalAnchorMode.Left:
				break;
			case HorizontalAnchorMode.Center:
				anchor.x -= width * 0.5f;
				break;
			case HorizontalAnchorMode.Right:
				anchor.x -= width;
				break;
			}
			switch(verticalAnchor){
			case VerticalAnchorMode.Top:
				break;
			case VerticalAnchorMode.Middle:
				anchor.y += lineCount * lineHeight * 0.5f;
				break;
			case VerticalAnchorMode.Bottom:
				anchor.y += lineCount * lineHeight;
				break;
			case VerticalAnchorMode.Baseline:
				anchor.y += font.baseline;
				break;
			}
			minBounds.x = anchor.x - font.leftMargin;
			minBounds.y = anchor.y - font.bottomMargin - lineHeight * (lineCount - 1) - 1f;
			maxBounds.x = anchor.x + font.rightMargin + width;
			maxBounds.y = anchor.y + font.topMargin;
		}
		else{
			switch(horizontalAnchor){
			case HorizontalAnchorMode.Left:
				anchor.x += font.leftMargin;
				break;
			case HorizontalAnchorMode.Center:
				anchor.x += font.leftMargin - width * 0.5f;
				break;
			case HorizontalAnchorMode.Right:
				anchor.x += font.leftMargin - width;
				break;
			}
			switch(verticalAnchor){
			case VerticalAnchorMode.Top:
				anchor.y -= font.topMargin;
				break;
			case VerticalAnchorMode.Middle:
				anchor.y += (font.bottomMargin - font.topMargin + lineCount * lineHeight) * 0.5f;
				break;
			case VerticalAnchorMode.Bottom:
				anchor.y += font.bottomMargin + lineCount * lineHeight;
				break;
			case VerticalAnchorMode.Baseline:
				anchor.y += font.baseline;
				break;
			}
			minBounds.x = anchor.x - font.leftMargin;
			minBounds.y = anchor.y - font.bottomMargin - lineHeight * (lineCount - 1) - 1f;
			maxBounds.x = minBounds.x + width;
			maxBounds.y = anchor.y + font.topMargin;
		}
		minBounds.z = anchor.z;
		maxBounds.z = anchor.z;
		
		// position sprites
		if(stringBuilder != null){
			for(int i = 0, t = 0, l = stringBuilder.Length; t < l; t++){
				char c = stringBuilder[t];
				if(c > ' '){
					Vector3 p = vertices[i];
					p.x += anchor.x;
					p.y += anchor.y;
					p.z = anchor.z;
					UpdateSprite(font[c], i, p);
					i += 4;
				}
			}
		}
		else{
			for(int i = 0, t = 0, l = text.Length; t < l; t++){
				char c = text[t];
				if(c > ' '){
					Vector3 p = vertices[i];
					p.x += anchor.x;
					p.y += anchor.y;
					p.z = anchor.z;
					UpdateSprite(font[c], i, p);
					i += 4;
				}
			}
		}
		
		// allow modifier to do its job
		if(modifier != null){
			modifier.Modify(this);
		}
		
		// update the mesh
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.bounds = new Bounds((minBounds + maxBounds) * 0.5f, maxBounds - minBounds);
		if(meshCollider != null){
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = mesh;
		}
	}
	
	#endregion
	
	#region Private Stuff
	
	private MeshCollider meshCollider;
	
	private void UpdateFromStringBuilder () {
		lineCount = 1;
		int
			nonWhiteSpaceCount = 0,
			textLength = stringBuilder.Length;
		
		if(textLength == 0){
			HideSprites(0);
			usedSpriteCount = 0;
			return;
		}
		
		// walk through the text
		Vector3
			caret = zeroVector3,
			metaData;
		CCFont.Char fontChar;
		int
			textIndex = 0,
			vertexIndex = 0;
		char c = stringBuilder[0];
		while(true){
			if(c <= ' '){
				if(++textIndex == textLength){
					break;
				}
				if(c == ' '){
					caret.x += font.spaceAdvance;
				}
				else if(c == '\n'){
					lineCount += 1;
					caret.x = 0f;
					caret.y -= lineHeight;
				}
				else if(c == '\t'){
					caret.x = (1f + Mathf.Floor(caret.x / tabSize)) * tabSize;
					
				}
				c = stringBuilder[textIndex];
			}
			else{
				if(++nonWhiteSpaceCount > spriteCount){
					AddSpritesFromStringBuilder(textIndex + 1);
				}
				
				// add a sprite
				fontChar = font[c];
				
				if(caret.x + fontChar.width > lineWidth){
					if(WordWrapFromStringBuilder(textIndex, vertexIndex, ref caret)){
						lineCount += 1;
					}
				}
				
				// store position in 1st vertex
				vertices[vertexIndex] = caret;
				
				// store advance in metaData.x
				metaData.x = fontChar.advance;
				
				if(++textIndex == textLength){
					metaData.y = 0f;
					metaData.z = 0f;
					vertices[vertexIndex + 1] = metaData;
					break;
				}
				c = stringBuilder[textIndex];
				if(c == '\n'){
					caret.x += fontChar.xOffset + fontChar.width;
					// store indication of last character before newline in metaData.y
					metaData.y = 1f;
					metaData.z = 0f;
					vertices[vertexIndex + 1].y = 1f;
				}
				else if(c <= ' '){
					caret.x += fontChar.advance;
					// store indication of last character before gap in metaData.z
					metaData.y = 0f;
					metaData.z = 1f;
					vertices[vertexIndex + 1].z = 1f;
				}
				else{
					metaData.y = 0f;
					metaData.z = 0f;
					caret.x += fontChar.AdvanceWithKerning(c);
				}
				// store metaData in 2nd vertex
				vertices[vertexIndex + 1] = metaData;
				vertexIndex += 4;
			}
		}
		
		if(nonWhiteSpaceCount < usedSpriteCount){
			HideSprites(nonWhiteSpaceCount);
		}
		usedSpriteCount = nonWhiteSpaceCount;
	}
	
	private void UpdateFromString () {
		lineCount = 1;
		int
			nonWhiteSpaceCount = 0,
			textLength = text.Length;
		
		if(textLength == 0){
			HideSprites(0);
			usedSpriteCount = 0;
			return;
		}
		
		Vector3
			caret = zeroVector3,
			metaData;
		CCFont.Char fontChar;
		int
			textIndex = 0,
			vertexIndex = 0;
		char c = text[0];
		while(true){
			if(c <= ' '){
				if(++textIndex == textLength){
					break;
				}
				if(c == ' '){
					caret.x += font.spaceAdvance;
				}
				else if(c == '\n'){
					lineCount += 1;
					caret.x = 0f;
					caret.y -= lineHeight;
				}
				else if(c == '\t'){
					caret.x = (1f + Mathf.Floor(caret.x / tabSize)) * tabSize;
				}
				c = text[textIndex];
			}
			else{
				if(++nonWhiteSpaceCount > spriteCount){
					AddSpritesFromString(textIndex + 1);
				}
				
				// add a sprite
				fontChar = font[c];
				
				if(caret.x + fontChar.width > lineWidth){
					if(WordWrapFromString(textIndex, vertexIndex, ref caret)){
						lineCount += 1;
					}
				}
				
				// store position in 1st vertex
				vertices[vertexIndex] = caret;
				
				// store advance in metaData.x
				metaData.x = fontChar.advance;
				
				if(++textIndex == textLength){
					metaData.y = 0f;
					metaData.z = 0f;
					vertices[vertexIndex + 1] = metaData;
					break;
				}
				c = text[textIndex];
				if(c == '\n'){
					caret.x += fontChar.xOffset + fontChar.width;
					// store indication of last character before newline in metaData.y
					metaData.y = 1f;
					metaData.z = 0f;
					vertices[vertexIndex + 1].y = 1f;
				}
				else if(c <= ' '){
					caret.x += fontChar.advance;
					// store indication of last character before gap in metaData.z
					metaData.y = 0f;
					metaData.z = 1f;
					vertices[vertexIndex + 1].z = 1f;
				}
				else{
					metaData.y = 0f;
					metaData.z = 0f;
					caret.x += fontChar.AdvanceWithKerning(c);
				}
				// store metaData in 2nd vertex
				vertices[vertexIndex + 1] = metaData;
				vertexIndex += 4;
			}
		}
		
		if(nonWhiteSpaceCount < usedSpriteCount){
			HideSprites(nonWhiteSpaceCount);
		}
		usedSpriteCount = nonWhiteSpaceCount;
	}

	private void UpdateSprite (CCFont.Char c, int index, Vector3 position) {
		// vertex order is
		// 01
		// 32
		Vector2 corner;
		corner.x = c.uMin;
		corner.y = c.vMax;
		uv[index] = corner;
		corner.x = c.uMax;
		uv[index + 1] = corner;
		corner.y = c.vMin;
		uv[index + 2] = corner;
		corner.x = c.uMin;
		uv[index + 3] = corner;
		
		position.x += c.xOffset;
		position.y += c.yOffset;
		
		vertices[index] = position;
		position.x += c.width;
		vertices[index + 1] = position;
		position.y -= c.height;
		vertices[index + 2] = position;
		position.x -= c.width;
		vertices[index + 3] = position;
	}
	
	private void AlignCharsCenterOrRight () {
		// start aligning at 2nd sprite because a single sprite needs no aligning
		float
			rightShift;
		int
			lineStart = 0,
			i = 4;
		for(int l = usedSpriteCount << 2; i < l; i += 4){
			if(vertices[i].x == 0f){
				// encountered a new line
				rightShift = lineWidth - vertices[i - 4].x - vertices[i - 3].x;
				if(rightShift > 0f){
					// shift previous line
					if(alignment == AlignmentMode.Center){
						rightShift *= 0.5f;
					}
					for(int li = lineStart; li < i; li += 4){
						vertices[li].x += rightShift;
					}
				}
				lineStart = i;
			}
		}
		// check last line
		rightShift = lineWidth - vertices[i - 4].x - vertices[i - 3].x;
		if(rightShift > 0f){
			// shift last line
			if(alignment == AlignmentMode.Center){
				rightShift *= 0.5f;
			}
			for(int li = lineStart; li < i; li += 4){
				vertices[li].x += rightShift;
			}
		}
	}
	
	private void JustifyChars () {
		// start justifying at 2nd sprite because a single sprite needs no aligning
		int
			lineStart = 0,
			i = 4;
		for(int l = usedSpriteCount << 2; i < l; i += 4){
			if(vertices[i].x == 0f){
				// encountered a new line
				if(vertices[i - 3].y == 0f){
					// newline was inserted by word wrapping, so try to stretch it
					JustifyLine(lineStart, i - 4);
				}
				lineStart = i;
			}
		}
	}
	
	private void JustifyLine (int first, int last) {
		// compute line width and count gaps
		float currentLineWidth = vertices[last].x + vertices[last + 1].x;
		int gapCount = -1;
		for(int i = first; i <= last; i += 4){
			if(vertices[i + 1].z == 1f){
				gapCount += 1;
			}
		}
		
		// don't touch lines without gaps
		if(gapCount <= 0){
			return;
		}
		
		// stretch all gaps to make the line fit
		float
			extraGapSize = (lineWidth - currentLineWidth) / gapCount,
			rightShift = 0f;
		for(int i = first; i <= last; i += 4){
			vertices[i].x += rightShift;
			if(vertices[i + 1].z == 1f){
				rightShift += extraGapSize;
			}
		}
	}
	
	private void HideSprites (int i) {
		// collapse visible sprites, beginning at some start index
		i <<= 2;
		for(int l = usedSpriteCount << 2; i < l; i += 4){
			vertices[i] = zeroVector3;
			vertices[i + 1] = zeroVector3;
			vertices[i + 2] = zeroVector3;
			vertices[i + 3] = zeroVector3;
		}
	}
	
	private void AddSpritesFromString (int i) {
		int newSpriteCount = 1;
		for(int l = text.Length; i < l; i++){
			if(text[i] > ' '){
				newSpriteCount += 1;
			}
		}
		AddSprites(newSpriteCount);
	}
	
	private void AddSpritesFromStringBuilder (int i) {
		int newSpriteCount = 1;
		for(int l = stringBuilder.Length; i < l; i++){
			if(stringBuilder[i] > ' '){
				newSpriteCount += 1;
			}
		}
		AddSprites(newSpriteCount);
	}
	
	private void AddSprites (int newSpriteCount) {
		// grow mesh data to accomodate more sprites
		newSpriteCount = spriteCount + ((newSpriteCount - 1) / chunkSize + 1) * chunkSize;
		mesh.Clear();
		int newLength = newSpriteCount << 2;
		
		Vector3[] oldVertices = vertices;
		vertices = new Vector3[newLength];
		colors = new Color[newLength];
		uv = new Vector2[newLength];
		triangles = new int[newSpriteCount * 6];
		
		// copy and regenerate relevant data
		int v = 0, t = 0, l = spriteCount << 2;
		for(; v < l; v += 4, t += 6){
			vertices[v] = oldVertices[v];
			vertices[v + 1] = oldVertices[v + 1];
			colors[v] = color;
			colors[v + 1] = color;
			colors[v + 2] = color;
			colors[v + 3] = color;
			triangles[t] = v;
			triangles[t + 1] = v + 1;
			triangles[t + 2] = v + 2;
			triangles[t + 3] = v;
			triangles[t + 4] = v + 2;
			triangles[t + 5] = v + 3;
		}
		
		// initialize new colors and triangles now, as they don't depend on text
		// so they needn't be updated again, barring modifiers
		for(; v < newLength; v += 4, t += 6){
			colors[v] = color;
			colors[v + 1] = color;
			colors[v + 2] = color;
			colors[v + 3] = color;
			// triangles are defined as clockwise |\| first \| and then |\
			triangles[t] = v;
			triangles[t + 1] = v + 1;
			triangles[t + 2] = v + 2;
			triangles[t + 3] = v;
			triangles[t + 4] = v + 2;
			triangles[t + 5] = v + 3;
		}
		mesh.vertices = vertices;
		mesh.colors = colors;
		mesh.triangles = triangles;
		spriteCount = newSpriteCount;
	}
	
	private bool WordWrapFromString(int textIndex, int vertexIndex, ref Vector3 caret){
		if(vertexIndex == 0){
			// don't wrap first character of entire text
			return false;
		}
		if(text[textIndex - 1] <= ' '){
			// no backtracking needed for first character of word
			caret.x = 0f;
			caret.y -= lineHeight;
			return true;
		}
		
		// backtrack to beginning of last word
		int i = textIndex;
		while(true){
			if(text[i] <= ' '){
				break;
			}
			if(--i < 0){
				// don't wrap first word
				return false;
			}
		}
		
		// set i to the last spriteIndex of the previous word
		i = vertexIndex + ((i - textIndex) << 2);
		
		// move last word to beginning of next line
		i += 4;
		Vector3 delta = new Vector3(-vertices[i].x, -lineHeight);
		if(delta.x == 0f){
			// don't wrap the only word of a line
			return false;
		}
		for(; i < vertexIndex; i++){
			vertices[i] += delta;
		}
		
		// update caret
		caret += delta;
		return true;
	}
	
	private bool WordWrapFromStringBuilder(int textIndex, int vertexIndex, ref Vector3 caret){
		if(vertexIndex == 0){
			// don't wrap first character of entire text
			return false;
		}
		if(stringBuilder[textIndex - 1] <= ' '){
			// no backtracking needed for first character of word
			caret.x = 0f;
			caret.y -= lineHeight;
			return true;
		}
		
		// backtrack to beginning of last word
		int i = textIndex;
		while(true){
			if(stringBuilder[i] <= ' '){
				break;
			}
			if(--i < 0){
				// don't wrap first word
				return false;
			}
		}
		
		// set i to the last spriteIndex of the previous word
		i = vertexIndex + ((i - textIndex) << 2);
		
		// move last word to beginning of next line
		i += 4;
		Vector3 delta = new Vector3(-vertices[i].x, -lineHeight);
		if(delta.x == 0f){
			// don't wrap the only word of a line
			return false;
		}
		for(; i < vertexIndex; i++){
			vertices[i] += delta;
		}
		
		// update caret
		caret += delta;
		return true;
	}
	
	#endregion
}
