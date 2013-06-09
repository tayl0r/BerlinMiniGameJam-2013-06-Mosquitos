using System;
using System.Collections.Generic;
using Rage;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(RageEdgetune))]
[AddComponentMenu("RageTools/Rage Text")]
[ExecuteInEditMode]
public class RageText : MonoBehaviour {

	private Dictionary<string, GameObject> _indexedChars = new Dictionary<string, GameObject>();
	public bool QuickMode;
	public enum AlignmentTypes { Left = 0, Center = 1, Right = 2 };
	public bool CharNotFound;
	public bool RageFontWasChanged;
	public bool RageFontIsPrefab;

	#region Properties
	[SerializeField]
	private bool _on;
	public bool On {
		get { return _on; }
		set {
			if (_on == false  && value ) {
				_on = true;
				UpdateText();
			} else
				_on = value;
		}
	}

	[SerializeField] private RageCanvasAlign _container;
	public RageCanvasAlign Container {
		get { return _container; }
		set {	if (value == _container) return;
				_container = value;
				UpdateText();
			}
	}

	[SerializeField] private int _displayBufferSize = 4;
	public int DisplayBufferSize {
		get { return _displayBufferSize; }
		set {	if (_displayBufferSize == value) return;
				_displayBufferSize = value <= 1 ? 1 : value;
				UpdateText();
			}
	}

	[SerializeField] private AlignmentTypes _alignment = AlignmentTypes.Left;
	public AlignmentTypes Alignment {
		get { return _alignment; }
		set {
			if(_alignment == value) return;
			_alignment = value;
			UpdateText();
		}
	}

	[SerializeField] private string _text;
	public string Text {
		get { return _text; }
		set {
			if(_text == value) return;
			_text = value;
			UpdateText();
		}
	}

	[SerializeField] private GameObject _rageFont;
	public GameObject RageFont {
		get { return _rageFont; }
		set {
			if(_rageFont == value) return;
			_rageFont = value;
			if (_rageFont == null) return;
			RageFontWasChanged = true;
			IndexFont();
			UpdateText();
		}
	}

	[SerializeField] private float _tracking = 0.1f;
	public float Tracking {
		get { return _tracking; }
		set {
			if(_tracking == value) return;
			_tracking = value;
			ApplyTracking();
		}
	}

	public int DisplaySize {
		set {
			int currentSize = transform.childCount;
			if (currentSize == value)
				return;
			if (value > currentSize) {
				IncreaseDisplayLength(currentSize, value);
				return;
			}
			DecrementDisplayLength(currentSize, value);
		}
		get { return transform.childCount; }
	}

	#endregion

	private string _paddedText;

	public void UpdateText() {
#if UNITY_4_0
		if (!gameObject.activeSelf) return;
#else
		if(!gameObject.active) return;
#endif
		if(!_on) return;
		if(RageFont == null) return;

		CharNotFound = false;
		FontIndexCheck();
		if (Text == "") Text = " ";
		_paddedText = Text.Pad((int)Alignment, DisplaySize);
		PrepareDisplayCheck();
		UpdateDisplayChars(_paddedText);
		ApplyAlignment();

		UpdateConnectedComponents();
	}

	public void Awake() {
		if (_container == null)
			_container = GetComponent<RageCanvasAlign>();
	}

	private void PrepareDisplayCheck() {
		if (Text.Length > DisplaySize) {
			DisplaySize += _displayBufferSize;
			return;
		}

		if (DisplaySize <= Text.Length + _displayBufferSize) return;
		DisplaySize = _displayBufferSize + Text.Length % _displayBufferSize;
	}

	/// <summary> Update RageGroup and EdgeTune if 'text' changed </summary>
	private void UpdateConnectedComponents() {

		var group = GetComponent<RageGroup>();
		if (group != null) {
			group.UpdatePathList();
			group.ApplyStyle();
		}
		var edgetune = GetComponent<RageEdgetune>();
		if(edgetune == null) return;
		if (edgetune.On) edgetune.DoEdgeTune();
		//Resources.UnloadUnusedAssets();
	}

	public void UpdateDisplayChars(string text) {

		if (string.IsNullOrEmpty(text)) text = " ";

		GameObject previousChar = null;

		for(int i = 0; i < text.Length; i++) {
			string sChar = text.Substring(i, 1);
			if(string.IsNullOrEmpty(sChar)) continue;
			sChar = ReplaceSpecialCharCheck(sChar);

			GameObject displaySlot = GetDisplayChar(i);
			if(displaySlot == null) continue;
			
			if(!_indexedChars.ContainsKey(sChar)) {
				CharNotFound = true;
				continue;
			}

			GameObject rageFontChar = _indexedChars[sChar];

			if(QuickMode) QuickCopy(displaySlot, rageFontChar);
			else displaySlot = UpdateDisplayChar(displaySlot, rageFontChar);

			if(previousChar != null) ApplyTracking(displaySlot, previousChar);

			previousChar = displaySlot;
		}
	}

	private void ApplyTracking() {
		if (!_on) return;
		GameObject previousChar = null;
		for (int i = 0; i < _paddedText.Length; i++) {
			GameObject displaySlot = GetDisplayChar(i);
			if (previousChar != null) ApplyTracking(displaySlot, previousChar);
			previousChar = displaySlot;
		}
	}

	private string ReplaceSpecialCharCheck (string sChar) {
		if (sChar == ".") sChar = "/.";
		if (sChar == "/") sChar = "//";
		return sChar;
	}

	private void ApplyTracking(GameObject currentChar, GameObject previousChar) {
		var previousCharBoxCollider = previousChar.GetComponent<BoxCollider>();
		var displayCharBoxCollider = currentChar.GetComponent<BoxCollider>();

		currentChar.transform.localPosition = new Vector3(
				(previousChar.transform.localPosition.x
				 + previousCharBoxCollider.center.x
				 + previousCharBoxCollider.extents.x
				 + Tracking
				 + displayCharBoxCollider.extents.x
				 - displayCharBoxCollider.center.x),
				currentChar.transform.localPosition.y,
				currentChar.transform.localPosition.z
			);
	}

	/// <summary> Offsets the characters positions on X according to the alignment.
	/// Stores the rotation, sets it to default and restores it after the operation, to avoid distortions.
	/// </summary>
	private void ApplyAlignment() {
		try{
			if (_container == null) _container = GetComponent<RageCanvasAlign>();

			float offset;
			var currentRotation = gameObject.transform.localRotation;
			switch (Alignment){
				case AlignmentTypes.Left:
					gameObject.transform.localRotation = Quaternion.identity;
					offset = gameObject.collider.bounds.min.x - GetDisplayChar(0).collider.bounds.min.x;
					OffsetHorizontal(offset);
					gameObject.transform.localRotation = currentRotation;
					ContainerApplyAlignment(RageCanvasAlign.HorizontalAlignType.Left);
					break;

				case AlignmentTypes.Center:
					gameObject.transform.localRotation = Quaternion.identity;
					offset = gameObject.collider.bounds.center.x -
							 (GetDisplayChar(0).collider.bounds.min.x +
							  GetDisplayChar(Text.Length - 1).collider.bounds.max.x)/2;
					OffsetHorizontal(offset);
					gameObject.transform.localRotation = currentRotation;
					ContainerApplyAlignment(RageCanvasAlign.HorizontalAlignType.Center);
					break;

				case AlignmentTypes.Right:
					offset = gameObject.collider.bounds.max.x - GetDisplayChar(DisplaySize - 1).collider.bounds.max.x;
					OffsetHorizontal(offset);
					gameObject.transform.localRotation = currentRotation;
					ContainerApplyAlignment(RageCanvasAlign.HorizontalAlignType.Right);
					break;
			}
		}catch (NullReferenceException){/*ignore*/}
	}

	public void ContainerApplyAlignment(RageCanvasAlign.HorizontalAlignType alignType) {
		if (_container == null) return;
		_container.HorizontalAlign = alignType;
		_container.DoAlignToCanvas();
	}

	private GameObject GetDisplayChar(int index){ return gameObject.GetChildByName(index.ToString()); }

	private void OffsetHorizontal(float offset) {
		for (int i = 0; i < DisplaySize; i++) {
			GameObject go = GetDisplayChar(i);
			if (go == null) continue;
			Vector3 current = go.transform.position;
			float x = current.x + offset;
			GetDisplayChar(i).transform.position = new Vector3(x, current.y, current.z);
		}
	}

	public static GameObject UpdateDisplayChar (GameObject toReplaceChar, GameObject prototypeChar) {
		if(toReplaceChar == null) return null;
		if(prototypeChar == null) return toReplaceChar;

		GameObject newDisplayChar = (GameObject)Instantiate(prototypeChar, toReplaceChar.transform.position, toReplaceChar.transform.rotation);
		newDisplayChar.name = toReplaceChar.name;
		newDisplayChar.transform.position = toReplaceChar.transform.position;
		newDisplayChar.transform.rotation = toReplaceChar.transform.rotation;
		newDisplayChar.transform.parent = toReplaceChar.transform.parent;
		newDisplayChar.transform.localScale = toReplaceChar.transform.localScale;

		toReplaceChar.SmartDestroy();

		return newDisplayChar;
	}

	private void DecrementDisplayLength(int currentLength, int newLength) {
		for(int i = currentLength; i > newLength; i--) {
			GameObject go = gameObject.GetChildByName((i-1).ToString());
			go.SmartDestroy();
		}
		UpdateText();
	}

	private void IncreaseDisplayLength(int currentLength, int newLength) {
		if (RageFont == null) return;
		for(int i = currentLength; i < newLength; i++) {

			GameObject newGO = (GameObject)Instantiate(_indexedChars[" "]);
			newGO.name = ""+transform.childCount;
			newGO.transform.parent = transform;

			if (i>1) {
				newGO.transform.rotation = GetDisplayChar(i - 1).transform.rotation;
				newGO.transform.localScale = GetDisplayChar(i - 1).transform.localScale;
				newGO.transform.localPosition = GetDisplayChar(i - 1).transform.localPosition;
			} else {
				newGO.transform.rotation = Quaternion.identity;
				newGO.transform.localScale = Vector3.one;
				var charOffset = Vector3.zero;
				var space = _indexedChars[" "];
				if (space!=null) charOffset = space.transform.position - RageFont.transform.position;
				newGO.transform.localPosition = new Vector3(0f, charOffset.y, 0f); //displayColliderBounds.center.y
			}
		}
		UpdateText();
	}

	private void FontIndexCheck() {
		if(_indexedChars.Count == 0) IndexFont();
	}

	private void IndexFont() {
		if(RageFont == null) return;
		_indexedChars = new Dictionary<string, GameObject>();
		foreach(Transform child in RageFont.transform)
			_indexedChars.Add(child.name, child.gameObject);
	}

	/// <summary>
	/// The QuickCopy function simply copies the mesh and collider to the display character.
	/// The RageSpline is not updated. Faster method and suitable if you don't need EdgeTune. </summary>
	private static void QuickCopy(GameObject displayChar, GameObject rageFontChar) {

		CopyColliderData(rageFontChar, displayChar);

		if(rageFontChar.name == " ") {
			RemoveAllMeshes(displayChar);
			return;
		}
		CopyGraphicElements(rageFontChar, displayChar);
	}

	private static void CopyGraphicElements(GameObject rageFontChar, GameObject displayChar) {

		MeshRenderer[] renderers = displayChar.GetComponentsInChildren<MeshRenderer>();
		foreach(MeshRenderer meshRenderer in renderers){

			meshRenderer.enabled = true;
			GameObject displayCharElement = meshRenderer.gameObject;
			if(displayCharElement == displayChar) continue;

			GameObject rageFontElement = rageFontChar.GetChildByName(displayCharElement.name);
			if (rageFontElement == null) {
				meshRenderer.enabled = false;
				continue;
			}

			MeshFilter srcMeshFilter = rageFontElement.GetComponent<MeshFilter>();
			MeshFilter targetMeshFilter = displayCharElement.GetComponent<MeshFilter>();
			if(srcMeshFilter == null || targetMeshFilter == null) return;

			targetMeshFilter.sharedMesh = srcMeshFilter.sharedMesh;
		}
	}

	private static void RemoveAllMeshes(GameObject displayChar) {
		MeshFilter[] filters = displayChar.GetComponentsInChildren<MeshFilter>();
		foreach (MeshFilter thisMeshFilter in filters) {
			thisMeshFilter.sharedMesh = null;
		}
	}

	private static void CopyColliderData(GameObject src, GameObject target) {
		BoxCollider targetCollider = target.GetComponent<BoxCollider>();
		if(targetCollider == null) return;

		BoxCollider srcCollider = src.GetComponent<BoxCollider>();
		if(srcCollider == null) return;

		targetCollider.extents = new Vector3 (srcCollider.extents.x, srcCollider.extents.y, srcCollider.extents.z);
		targetCollider.center = new Vector3 (srcCollider.center.x, srcCollider.center.y, srcCollider.center.z);
	}
}
