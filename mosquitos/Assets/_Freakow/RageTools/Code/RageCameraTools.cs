using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

///RageTools-specific RageCamera properties and methods
public partial class RageCamera : MonoBehaviour {

	public float DefaultOrthoSize = 100f, DefaultResolutionHeight;
	public float LastResolutionHeight, LastOrthoSize;
	public Quaternion LastCameraRotation = Quaternion.identity;
	public bool EditorRealtimeUpdate;
	#region Property - UpdateThreshold _updateThreshold
	[SerializeField]private float _updateThreshold = 0.2f;
	public float UpdateThreshold {
		get {
			if (_updateThreshold <= 0f) _updateThreshold = 0.01f;
			return _updateThreshold;
		}
		set { _updateThreshold = (value <= 0f) ? 0.01f : value; }
	}
	#endregion
	//private bool _changedResolutionHeight, _changedOrthoSize, _changedRotation;

	public delegate void ChangedResolutionHeight();
	public static event ChangedResolutionHeight OnChangedResolutionHeight;

	public delegate void ChangedOrthoSize();
	public static event ChangedOrthoSize OnChangedOrthoSize;

	public delegate void ChangedCameraRotation();
	public static event ChangedCameraRotation OnChangedCameraRotation;

#if UNITY_EDITOR
	public void OnDrawGizmos() {
		if (Application.isPlaying) return;
		if (EditorRealtimeUpdate) UpdateStep();
	}
#endif

	public void Update() {
		if (!Application.isPlaying) return;
		UpdateStep();
	}

	private void UpdateStep( ) {
		CheckCameraChanges();
		//SetCameraTransparencyMode();
	}

	public void Initialize( ) {
		_cameraMain = Camera.main;
		LastResolutionHeight = DefaultResolutionHeight = GetGameViewSize().y;
		LastOrthoSize = DefaultOrthoSize = _cameraMain.orthographicSize;
		LastCameraRotation = _cameraMain.transform.rotation;
	}

	public void CheckCameraChanges() {
		if (_cameraMain == null) return;
		var currentResolutionHeight = GetGameViewSize().y;
		var percentageOfChange = PercentageOfChange(currentResolutionHeight, LastResolutionHeight);
		if (percentageOfChange > UpdateThreshold) {
			LastResolutionHeight = currentResolutionHeight;
			if (OnChangedResolutionHeight != null) OnChangedResolutionHeight();
		}
		if (_cameraMain.isOrthoGraphic) {
			var currentOrthoSize = _cameraMain.orthographicSize;
			percentageOfChange = PercentageOfChange(currentOrthoSize, LastOrthoSize);
			if (percentageOfChange > UpdateThreshold) {
				LastOrthoSize = currentOrthoSize;
				if (OnChangedOrthoSize != null) OnChangedOrthoSize();
			}
		}
		percentageOfChange = Quaternion.Angle(_cameraMain.transform.rotation, LastCameraRotation) / 100f;
		if (percentageOfChange > UpdateThreshold) {
			LastCameraRotation = _cameraMain.transform.rotation;
			if (OnChangedCameraRotation != null) OnChangedCameraRotation();
		}
	}

	private static float PercentageOfChange(float newValue, float oldValue) {
		return Mathf.Abs(newValue - oldValue) / newValue;
	}

	/// <summary> Get Camera view size. If it's in editor, get Game View size through reflection </summary>
	public static Vector2 GetGameViewSize() {
		if (!Application.isEditor)
			return new Vector2(Screen.width, Screen.height);
		Type gameView = Type.GetType("UnityEditor.GameView,UnityEditor");
		if (gameView == null) return Vector2.zero;
		
		MethodInfo methodInfo = gameView.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
		System.Object res = methodInfo.Invoke(null, null);
		return (Vector2) res;
	}
}
