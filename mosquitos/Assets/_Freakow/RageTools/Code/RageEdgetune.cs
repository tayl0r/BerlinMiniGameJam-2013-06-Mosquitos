using UnityEngine;

[RequireComponent(typeof(RageGroup))]
[AddComponentMenu("RageTools/Rage EdgeTune")]
[ExecuteInEditMode]
[System.Serializable]
public class RageEdgetune : MonoBehaviour {

	public bool On, StartOnly;
	public GameObject TextMeshGO;
	public RageGroup Group;
	public RageEdgetuneData Data;

	public float StartObjectHeight;
	public TextMesh DebugTextMesh;
	public bool DebugDensity;

	private bool _initializeNow;
	[SerializeField]private float _lastZoffset, _lastObjectHeight;
	[SerializeField] private Quaternion _lastObjectRotation = Quaternion.identity;
	[SerializeField]private bool _changedResolutionHeight, _changedZOffset, _changedOrthoSize, 
								 _changedObjectHeight, _changedCameraRotation, _changedObjectRotation;
	#region Property - RageCamera _rageCamera
	[SerializeField]private RageCamera _rageCamera ;
	private RageCamera SceneRageCamera {
		get {
			if (_rageCamera == null) _rageCamera = Camera.main.GetComponent<RageCamera>();
			if (_rageCamera == null) _rageCamera = Camera.main.gameObject.AddComponent<RageCamera>();
			return _rageCamera;
		}
		set { _rageCamera = value; }
	}
	#endregion
	#region Property - MainCamera _mainCamera
	private Camera _mainCamera;
	[SerializeField]private Camera MainCamera {
		get {	if (_mainCamera == null) _mainCamera = Camera.main;
				return _mainCamera; }
		set { _mainCamera = value; }
	}
	#endregion
	#region Property - UpdateThreshold _updateThreshold
	private float _updateThreshold = 0.2f;
	public float UpdateThreshold {
		get {	if (_updateThreshold <= 0f) _updateThreshold = 0.01f;
				return _updateThreshold;
			}
		set { _updateThreshold = (value <= 0f) ? 0.01f : value; }
	}
	#endregion
	#region Property - StartCameraDistance _startCameraDistance
	private float _startCameraDistance = 1f;
	private float StartCameraDistance {
		get { if (Mathf.Approximately(_startCameraDistance, 0f)) _startCameraDistance = 0.01f;
				return _startCameraDistance;
			}
		set { _startCameraDistance = (Mathf.Approximately(value , 0f)) ? 0.01f : value; }
	}
	#endregion

	// Density and Anti-aliasing Factor-In Variables
	[SerializeField]private float _dfScale = 1, _dfResolution = 1, _dfZoffset = 1, _dfOrthoSize = 1f;
	[SerializeField]private float _aafResolution = 1, _aafZoffset = 1, _aafOrthoSize = 1;

	public void ScheduleInitialize() { _initializeNow = true; }

	public void Awake() {
		AddReferences();
	}

	public void OnEnable() {
		RageCamera.OnChangedResolutionHeight += OnChangedResolutionHeight;
		RageCamera.OnChangedOrthoSize += OnChangedOrthoSize;
		RageCamera.OnChangedCameraRotation += OnChangedCameraCameraRotation;
	}
	/// <summary>Will be fired every time RageCamera's events are triggered</summary>
	void OnChangedResolutionHeight() {_changedResolutionHeight = true;}
	void OnChangedOrthoSize() { _changedOrthoSize = true; }
	void OnChangedCameraCameraRotation() { _changedCameraRotation = true; }

	public void OnDisable() {
		RageCamera.OnChangedResolutionHeight -= OnChangedResolutionHeight;
	}

	public void Start() {
		if (!On) return;
		if (!StartOnly) {
			DoEdgeTune();
			return;
		}
		var updateStepBackup = Group.UpdateStep;
		Group.UpdateStep = 0;
		DoEdgeTune();
		Group.UpdateStep = updateStepBackup;
	}

// 	public void OnDrawGizmos() {
// 		if (Application.isPlaying) return;
// 		if (!SceneRageCamera.EditorRealtimeUpdate) return;
// 		if (RefreshCheck()) DoEdgeTune();
// 	}

	public void Update() {
		if (_initializeNow) {
			Initialize();
			_initializeNow = false;
		}
//		if (!Application.isPlaying) return;
		if (StartOnly) return;
		if (RefreshCheck()) DoEdgeTune();
	}

	private void AddReferences() {
		if (Group == null) Group = gameObject.GetComponent<RageGroup>();
		if (Data == null) Data = ScriptableObject.CreateInstance<RageEdgetuneData>();
	}

	/// <summary> Finds the RageGroup if needed, then the current game object's size and z distance (if Perspective camera) </summary>
	public void Initialize() {
		if(Group == null) Group = gameObject.GetComponent<RageGroup>();
		SceneRageCamera.Initialize();
		MainCamera = Camera.main;
		UpdateThreshold = SceneRageCamera.UpdateThreshold;

		_lastObjectHeight = StartObjectHeight = gameObject.transform.lossyScale.y; ;
		_lastZoffset = StartCameraDistance = OffsetFromCamera();
	}

	// TODO: Change to local update threshold
// 	public void UpdatePixelHeight() {
// 		Data.PixelPerfectHeight = RageToolsExtensions.GetGameViewSize().y;
// 	}

	public bool RefreshCheck() {
		if (!On) return false;

		_changedZOffset = ChangedZOffset();
		_changedObjectHeight = ChangedObjectHeight();
		_changedObjectRotation = ChangedObjectRotation();

		bool mustRefresh = (_changedResolutionHeight || _changedOrthoSize || _changedCameraRotation ||
							_changedObjectHeight || _changedZOffset || _changedObjectRotation);
		return mustRefresh;
	}

   #region Check for Valid (object) Changes

	private bool ChangedZOffset() {
		float currentZoffset = OffsetFromCamera();
		var percentageOfChange = PercentageOfChange(currentZoffset, _lastZoffset);
		if (percentageOfChange < UpdateThreshold) return false;
		_lastZoffset = currentZoffset;
		return true;
	}

	private bool ChangedObjectHeight() {
		float currentObjectHeight = gameObject.transform.lossyScale.y;
		var percentageOfChange = PercentageOfChange(currentObjectHeight, _lastObjectHeight);
		if (percentageOfChange < UpdateThreshold) return false;
		_lastObjectHeight = currentObjectHeight;
		return true;
	}

	/// <summary> Checks if the object has changed rotation significantly since the last frame </summary>
	private bool ChangedObjectRotation() {
		if (Mathf.Approximately(Data.PerspectiveBlur, 0f)) return false;
		Quaternion currentObjectRotation = gameObject.transform.rotation;
		var percentageOfChange = Quaternion.Angle(currentObjectRotation, _lastObjectRotation) / 100f;
		if (percentageOfChange < (UpdateThreshold/2)) return false;
		_lastObjectRotation = currentObjectRotation;
		return true;
	}
   #endregion

	public void DoEdgeTune () {
		CheckRageGroupAndList();
		CalculateAndUpdateAa();

		if (Data == null) Data = ScriptableObject.CreateInstance<RageEdgetuneData>();
		
		if (Data.AutomaticLod) CalculateAndUpdateDensity();

		Group.QueueRefresh();
		_changedResolutionHeight = _changedOrthoSize = _changedCameraRotation = false;
	}

	/// <summary> Keeps the AA "look" independent of resolution, shape size and offset from camera </summary>
	private float CalculateAndUpdateAa() {
		if (_changedResolutionHeight) _aafResolution = AaFactorInResolution();
		if (_changedZOffset) _aafZoffset = AaFactorInZOffset();
		if (_changedOrthoSize) _aafOrthoSize = AaFactorInOrthoSize();

		float aaAdjustFactor = _aafResolution * _aafZoffset * _aafOrthoSize;
		aaAdjustFactor = AaZoomOutAttenuation(aaAdjustFactor);
		aaAdjustFactor += AaFactorInAngleDelta();

		if (Data == null) Data = ScriptableObject.CreateInstance<RageEdgetuneData>();
		aaAdjustFactor = Mathf.Pow (aaAdjustFactor, Data.AaFactor);
		if (DebugTextMesh != null && !DebugDensity)
			DebugTextMesh.text = aaAdjustFactor+"";
		Group.List.UpdateAa(aaAdjustFactor, true, !Data.AutomaticLod);
		return aaAdjustFactor;
	}
	
	/// <summary> Keeps the apparent density independent of resolution, shape size and offset from camera </summary>
	private float CalculateAndUpdateDensity() {
		if (_changedObjectHeight) _dfScale = DensityFactorInScale();
		if (_changedResolutionHeight) _dfResolution = DensityFactorInResolution();
		if (_changedZOffset) _dfZoffset = DensityFactorInZOffset();
		if (_changedOrthoSize) _dfOrthoSize = DensityFactorInOrthoSize();
		float densityAdjustFactor = _dfScale * _dfZoffset * _dfResolution * _dfOrthoSize;
		densityAdjustFactor = DensityZoomOutAttenuation(densityAdjustFactor);

		if (DebugTextMesh != null && DebugDensity)
			DebugTextMesh.text = "Scl: "+_dfScale+"  Res: "+_dfResolution+"  Zoffs: "+_dfZoffset+"  Ortho: "+_dfOrthoSize ;
		Group.List.UpdateDensity(densityAdjustFactor, Data.MaxDensity);
		return densityAdjustFactor;
	}


	#region AA Factor Math
	private float AaFactorInResolution() {
		return SceneRageCamera.DefaultResolutionHeight / RageToolsExtension.GetGameViewSize().y;
	}

	private float AaFactorInZOffset() {
		if (MainCamera.isOrthoGraphic) return 1f;
		return (OffsetFromCamera() / StartCameraDistance);
	}

	private float AaFactorInOrthoSize() {
		return (MainCamera.orthographicSize / SceneRageCamera.DefaultOrthoSize);
	}

	private float AaFactorInAngleDelta() {
		if (Mathf.Approximately(Data.PerspectiveBlur, 0f)) return 0f;
		Vector3 crossFowardVector = Vector3.Cross(MainCamera.transform.forward, transform.forward);
		var distanceOffset = Vector3.Distance(Vector3.zero, crossFowardVector);
		return Mathf.Lerp(0, Data.PerspectiveBlur, Mathf.Pow(distanceOffset, 10f));
	}

	private float AaZoomOutAttenuation(float factor) {
		if (factor <= 1.0f) return factor;
		if (factor >= 40f) return 40f; 
		//Debug.Log (1 + Mathf.Log (factor, 1.35f));
		return factor - Mathf.Pow(factor, 2f) / 75f;
	}

   #endregion

   #region Density Factor Math
	private float DensityFactorInScale() {
		return 1 / Mathf.Pow (StartObjectHeight / Mathf.Abs(gameObject.transform.lossyScale.y), Data.DensityFactor);
	}

	private float DensityFactorInResolution() {
		return RageToolsExtension.GetGameViewSize().y / SceneRageCamera.DefaultResolutionHeight;
	}

	private float DensityFactorInZOffset() {
		if (MainCamera.isOrthoGraphic) return 1f;
		return Mathf.Pow(StartCameraDistance / OffsetFromCamera(), Data.DensityFactor);
	}

	private float DensityFactorInOrthoSize( ) {
		if (! MainCamera.isOrthoGraphic) return 1;
		return SceneRageCamera.DefaultOrthoSize / MainCamera.orthographicSize;
	}

	private float DensityZoomOutAttenuation(float factor) {
		if (factor >= 1.0f) return factor;
		return Mathf.Pow(factor, 0.6f);
	}
   #endregion
	
	/// <summary> Calculates the distance from camera 
	/// depending on the X rotation (pitch) angle. </summary>
	private float OffsetFromCamera() {
		return (Vector3.Distance(gameObject.transform.position, SceneRageCamera.transform.position));
	}

	private void CheckRageGroupAndList() {
		if(Group != null && Group.List != null && Group.List.Count != 0) return;
		Group = GetComponent<RageGroup>();
		Group.UpdatePathList();
	}

// 	private void TestSnapShot() {
// 		if (!_initializeNow) return;
// 		Initialize();
// 		_initializeNow = false;
// 	}

	public void GuessMaxDensity() {
		Data.MaxDensity = Group.GetMaxDensity() * 2;
	}

	/// <summary> This function is a replicate from RageCamera's, re-added for being in separate packages (RS & RT) </summary>
	private static float PercentageOfChange(float newValue, float oldValue) {
		return Mathf.Abs(newValue - oldValue) / newValue;
	}
}
