using System.Collections.Generic;
using Holoville.HOTween;
using UnityEngine;
using Rage;

public interface IRageButton {
	void OnClick( );
	void OnHoverIn( );
	void OnHoverOut( );
}

public static class RageButtonExtensionMethods {

	public static void ShowAndHide(this RageButtonData buttonData) {
		buttonData.ToDisableOnClick.RecursiveActivate(false);
		buttonData.ToEnableOnClick.RecursiveActivate(true);
		foreach (GameObject gO in buttonData.ToDeleteOnClick) gO.SmartDestroy();
		foreach (GameObject gO in buttonData.ToInstantiateOnClick) Object.Instantiate(gO);
	}

	public static bool NotColliding(this Collider collider) {
		try {
			if (collider == null) return false;
			RaycastHit hit;
			return !collider.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity);
		} catch { return false; }
	}

	public static void PlayCheck(this AudioSource sound) {
		if (sound == null) return;
		sound.Play();
	}

	public static void PlayTweens(this List<Holoville.HOTween.Tweener> tweenerList, bool reverse, bool reset) {
		foreach (Holoville.HOTween.Tweener tweener in tweenerList) {
			tweener.autoKillOnComplete = false;
			if (!reverse) {
				if (reset) tweener.Restart();
				tweener.PlayForward();
			} else {
				if (reset) tweener.GoTo(tweener.duration);
				tweener.PlayBackwards();
			}
		}
	}

	/// <summary> Finds and returns a list of child tweeners with a given ID </summary>
	public static void InitializeTweenerList(this List<Holoville.HOTween.Tweener> tweenerList, GameObject root, string tweenId) {
		if (tweenerList == null) 
			tweenerList = new List<Holoville.HOTween.Tweener>();
		var tweensWithId = HOTween.GetTweensById(tweenId, true);
		if (tweensWithId.Count == 0) return;
		var visualEditors = root.GetComponentsInChildren<HOTweenComponent>();
		if (visualEditors == null) return;
		foreach (HOTweenComponent visualEditor in visualEditors)
			if (visualEditor.generatedTweeners != null)
				foreach (Holoville.HOTween.Tweener tweener in visualEditor.generatedTweeners) {
					if (tweener.id != tweenId) continue;
					tweenerList.Add(tweener);
				}
	}
}
