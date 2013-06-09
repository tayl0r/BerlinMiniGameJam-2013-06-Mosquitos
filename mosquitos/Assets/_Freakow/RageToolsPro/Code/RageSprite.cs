using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rage;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animation))]
[AddComponentMenu("RageTools/Rage Sprite")]
//[ExecuteInEditMode]
public class RageSprite : MonoBehaviour {

	public List<RageSpriteAnimation> Animations;
	public float SetFrame;
	public bool DraftPreview = true;
	public List<RageSpriteFrame> AllFrames = new List<RageSpriteFrame>();
	private float _currentAnimSpeed;
	private float _currentFrame;

	public void GenerateClip(RageSpriteAnimation anim) {
		var curve = new AnimationCurve();
		var keyframeList = new List<Keyframe>();
		float time = 0f; // = anim.Frames[0].globalIdx;
		if(anim.FrameDuration < 0.01f)
			anim.FrameDuration = 0.01f;

		for(int i = 0; i < (anim.Frames.Count); i++) {
			keyframeList.Add(new Keyframe(time, anim.Frames[i].globalIdx, 0, 0));
			time += anim.FrameDuration + anim.Frames[i].Delay;
			keyframeList.Add(new Keyframe(time-0.0001f, anim.Frames[i].globalIdx, 0, 0));
		}
		keyframeList.Add(new Keyframe(time, anim.Frames[anim.Frames.Count-1].globalIdx, 0, 0)); // Adds Last key hold frame
		curve.keys = keyframeList.ToArray();
		var clip = new AnimationClip { wrapMode = WrapMode.Loop, name = anim.Name };
		clip.SetCurve("", typeof(RageSprite), "SetFrame", curve);
		animation.AddClip(clip, clip.name);
		#if UNITY_EDITOR
			AssetDatabase.CreateAsset(clip, "Assets/" + anim.Name + ".anim");
			AssetDatabase.SaveAssets();
		#endif
		animation.clip = clip;
	}

	/// <summary> In the first (start) frame, RageSprite makes sure only the current-set frame is enabled </summary>
	public void Start() {
		for (int i = 0; i < (AllFrames.Count); i++)
			AllFrames[i].startCheck = true;
	}
	
	public void Update () {
		UpdateFrameCheck();
	}

	private void UpdateFrameCheck() {
		if (Mathf.Approximately(SetFrame, _currentFrame)) return;
		_currentFrame = SetFrame;
		DisableAllButCurrent();
	}

	void DisableAllButCurrent() {
		int globalIdx = Mathf.FloorToInt (SetFrame);
		for (int i = 0; i < (AllFrames.Count); i++) {
			if (i != globalIdx) {
				AllFrames[i].Enabled = false;
				continue;
			}
			AllFrames[i].Enabled = true;
		}
	}

	public void SetActiveFrameObject(RageSpriteFrame rsFrame) {
		foreach (RageSpriteFrame thisFrame in AllFrames)
			thisFrame.Enabled = (thisFrame.gO == rsFrame.gO);
	}

	[ContextMenu("Clear Preview Cache")]
	public void ClearPreviewCache() {
		AllFrames = new List<RageSpriteFrame>();
	}

}

[System.Serializable]
public class RageSpriteAnimation {
	public bool ExpandAnimation;
	public bool ExpandFrames = true;
	public RageSprite HostRageSprite;

	private System.DateTime _lastFrameTime;

	#region IsPreviewing Property

	private bool _isPreviewing;

	public bool IsPreviewing {
		get { return _isPreviewing; }
		set {
			_isPreviewing = value;
			if (!_isPreviewing) return;
			_lastFrameTime = DateTime.Now;
		}
	}

	#endregion

	#region CurrentFrameIdx Property

	private int _currentFrameIdx;
	public int CurrentFrameIdx {
		get {
			if (_currentFrameIdx >= Frames.Count) _currentFrameIdx = 0; //Resets the animation to start
			return _currentFrameIdx;
		}
		set {
			_currentFrameIdx = value;
			HostRageSprite.SetActiveFrameObject (Frames[_currentFrameIdx]);
		}
	}

	#endregion

	public string Name = "NewAnimation...";
	public float FrameDuration = 0.1f;
	public List<RageSpriteFrame> Frames = new List<RageSpriteFrame>();

	public void RemoveCheck(int frameIndex) {
		if (frameIndex < 0 || frameIndex >= Frames.Count) return;
		var frame = Frames[frameIndex];
		Frames.RemoveAt (frameIndex);
		ClearFromCacheCheck(frame);
	}

	private void ClearFromCacheCheck (RageSpriteFrame frame) {
		bool foundAnotherReference = false;
		foreach (RageSpriteAnimation thisAnim in HostRageSprite.Animations) {
			if (thisAnim == this) continue;		// Required by RemoveAllFrames
			if (thisAnim.Frames.Contains (frame)) foundAnotherReference = true;
		}
		var removalIdx = HostRageSprite.AllFrames.IndexOf (frame);
		if (!foundAnotherReference && removalIdx != -1)
			HostRageSprite.AllFrames.RemoveAt (removalIdx);
	}

	public void RemoveAllFrames() {
		foreach (RageSpriteFrame thisFrame in Frames)
			ClearFromCacheCheck (thisFrame);
		Frames.Clear();
	}

	public void MoveFrameUp(int frameIndex) {
		if (frameIndex == 0) return;
		var frame = Frames[frameIndex];
		Frames.RemoveAt(frameIndex);
		Frames.Insert(frameIndex - 1, frame);
	}

	public void MoveFrameToTop(int frameIndex) {
		if (frameIndex == 0) return;
		var frame = Frames[frameIndex];
		Frames.RemoveAt(frameIndex);
		Frames.Insert(0, frame);
	}

	public void MoveFrameDown(int frameIndex) {
		if (frameIndex == Frames.Count - 1) return;
		var frame = Frames[frameIndex];
		Frames.RemoveAt(frameIndex);
		Frames.Insert(frameIndex + 1, frame);
	}

	public void MoveFrameToBottom(int frameIndex) {
		if (frameIndex == Frames.Count - 1) return;
		var frame = Frames[frameIndex];
		Frames.RemoveAt(frameIndex);
		Frames.Insert(Frames.Count, frame);
	}

	public void AddFrames(GameObject[] frameObjects) {
		var frameList = new List<GameObject>(frameObjects);
		frameList = frameList.OrderBy(x => x.name).ToList();
		foreach (GameObject frameObject in frameList)
			AddFrame(frameObject);
	}

	public void AddFrame(GameObject frameObject) {
		RageSpriteFrame hostFrame = HostRageSprite.AllFrames.ContainsFrame(frameObject);
		RageSpriteFrame localFrame = Frames.ContainsFrame (frameObject);
		bool hostFrameFound = hostFrame != null;
		bool localFrameFound = localFrame != null;

		if (hostFrameFound) {
			//if (!localFrameFound) 
			Frames.Add(hostFrame);
		} else {
			if (localFrameFound) {
				HostRageSprite.AllFrames.Add(localFrame);
				UpdateAllFramesIdx(localFrame);
			} else {
				var newRageSpriteFrame = new RageSpriteFrame(frameObject);
				Frames.Add(newRageSpriteFrame);
				HostRageSprite.AllFrames.Add(newRageSpriteFrame);
				UpdateAllFramesIdx(newRageSpriteFrame);
			}
		}
	}

	private void UpdateAllFramesIdx (RageSpriteFrame localFrame) {
		var newIdx = HostRageSprite.AllFrames.IndexOf (localFrame);
		HostRageSprite.AllFrames[newIdx].globalIdx = newIdx;
	}

	public void AddChildFrames(GameObject rootObject) {
		var childObjects = new List<GameObject>();
		foreach (Transform childgO in rootObject.transform) {
			childObjects.Add (childgO.gameObject);
		}
		AddFrames (childObjects.ToArray());
	}

	public void CheckPreviewFrame() {
		if (!IsPreviewing) return;
		float pastTime = ((DateTime.Now - _lastFrameTime).Milliseconds) / 1000f;
		if (Frames[_currentFrameIdx].Delay + FrameDuration > pastTime) return;
		StepFrame();
		_lastFrameTime = DateTime.Now;
	}

	private void StepFrame() {
		int nextFrame = _currentFrameIdx + 1;
		if (nextFrame >= Frames.Count) 
			nextFrame = 0;
		CurrentFrameIdx = nextFrame;
	}
}

[System.Serializable]
public class RageSpriteFrame {
	public GameObject gO;
	public float Delay = 0f;
	public int globalIdx;
	public bool startCheck;
	[SerializeField]private bool _enabled;
	public bool Enabled { 
		get { return _enabled; }
		set {
			if (_enabled == value && !startCheck) return;
			if (startCheck) startCheck = false;
			_enabled = value;
#if UNITY_4_0
			gO.SetActive(value);
#else
			gO.SetActiveRecursively(value);
#endif
		}
	}
	public RageSpritePreviewData Preview = null;
	
	public RageSpritePreviewData GetPreview() {
			if (Preview == null || Preview.FramePreviewData.Count == 0)
				Preview = new RageSpritePreviewData(gO);
			return Preview;
	}

	public void SetPreview(RageSpritePreviewData newPreview) {
			Preview = newPreview;
	}
 
	public RageSpriteFrame(GameObject frameObject) {
		gO = frameObject;
	}
}
