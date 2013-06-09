using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
[ExecuteInEditMode]
public partial class RageCamera : MonoBehaviour {

	[SerializeField]private Camera _cameraMain;
	private bool _started;

	public void OnEnable() {
		if (_started) return;
		_cameraMain = Camera.main;
		_cameraMain.transparencySortMode = TransparencySortMode.Orthographic;
		_started = true;
	}

}
