using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class MosquitoManager : MonoBehaviour {
	
	public GameObject _biteMarkPrefab;
	public Transform _biteMarks;
	
	public GiantArm _armPrefab;
	public bool _armSpawned;
	
	public Transform _bg;
	public Mosquito _mosquitoPrefab;
	public Transform _swarmParent;
	public Transform _swarmParentFeeding;
	public Mosquito _swarmMaster;
	public Camera _camera;
	public Transform _cameraT;
	public float _swarmSpeed;
	public float _unitSpeed;
	public int _numBites;
	
	public List<Mosquito> _swarm = new List<Mosquito>();
	const float _boundary = 100f;
	
	public static MosquitoManager instance;
	
	void Awake() {
		instance = this;
	}
	
	void Start() {
		for (int i = 0; i < 1; i++) {
			SpawnChild(_swarmParent.position);
		}
		_swarmMaster = _swarm[0];
	}
	
	public Mosquito SpawnChild(Vector3 pos) {
		var child = Instantiate(_mosquitoPrefab) as Mosquito;
		child._t.parent = _swarmParent;
		child._t.position = pos;
		pos = child._t.localPosition;
		pos.z = 0f;
		child._t.localPosition = pos;
		_swarm.Add(child);
		Mosquito._swarmSize = Mathf.Max(200f, (float)_swarm.Count * 45f);
		child.Setup(this);
		return child;
	}

    void Update() {
		Mosquito._speed = _unitSpeed;
//		for (int i = 0; i < _swarm.Count; ++i) {
//			var child = _swarm[i];
//			child.Move(i, _swarm);
//		}

		// add some speed towards our target
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = _swarmParent.position.z - _camera.transform.position.z;
		Vector3 screenPosition = _camera.ScreenToWorldPoint(mousePosition);
		//Debug.Log(screenPosition);

//		Vector3 targetVector = target - _swarmParent.position;
//		targetVector.Normalize();
		float dist = Vector3.Distance(_swarmParent.position, screenPosition);
		float speed = _swarmSpeed;
		if (dist <= 100f) {
			speed = Mathf.Lerp(_swarmSpeed, _swarmSpeed * .25f, dist / 100f);
		}
		_swarmParent.position = Vector3.MoveTowards(_swarmParent.position, screenPosition, speed * Time.deltaTime);
		
		
//		// see if we should move the camera
//		Vector3 centerPos = _camera.ViewportToWorldPoint(new Vector3(.5f, .5f, mousePosition.z));
//		float distanceFromCenter = Vector3.Distance(screenPosition, centerPos);
//		if (distanceFromCenter >= 250f) {
//			dist = Vector3.Distance(_swarmParent.position, screenPosition);
//			speed = _swarmSpeed;
//			if (dist <= 100f) {
//				speed = Mathf.Lerp(_swarmSpeed, _swarmSpeed * .25f, dist / 100f);
//			}
//			_swarmParent.position = Vector3.MoveTowards(_swarmParent.position, screenPosition, speed * Time.deltaTime);
//		}
		Vector3 cameraPos = _swarmParent.position - new Vector3(512f, 384f, 0f);
		cameraPos.z = _cameraT.position.z;
		Vector3 prevCameraPos = _cameraT.position;
		_cameraT.position = cameraPos;
		Vector3 cameraDiff = cameraPos - prevCameraPos;
		_bg.Translate(cameraDiff * .5f, Space.World);
	}
	
	public void SetFeeding(Mosquito mosq, bool feeding) {
		if (feeding) {
			mosq._t.parent = _swarmParentFeeding;
		} else {
			mosq._t.parent = _swarmParent;
		}
	}
	
	public void AddBiteMark(Vector3 pos) {
		var biteMark = Instantiate(_biteMarkPrefab) as GameObject;
		biteMark.transform.parent = _biteMarks;
		biteMark.transform.position = pos;
		pos = biteMark.transform.localPosition;
		pos.z = 0f;
		biteMark.transform.localPosition = pos;
		
		tk2dSprite sprite = biteMark.GetComponentInChildren<tk2dSprite>();
		sprite.color = new Color(1f, 1f, 1f, 0f);
		HOTween.To(sprite, .5f, new TweenParms().Prop("color", new Color(1f, 1f, 1f, 1f)).Ease(EaseType.Linear));
		
		_numBites++;
		if (_armSpawned == false) {
			StartCoroutine(SpawnArm(biteMark.transform.position));
		}
	}
	
	IEnumerator SpawnArm(Vector3 pos) {
		while (_armSpawned) {
			yield return 1;
		}
		_armSpawned = true;
		GiantArm arm = Instantiate(_armPrefab) as GiantArm;
		pos.z = 500f;
		pos.x += Random.Range(-300f, 300f);
		pos.y += Random.Range(-300f, 300f);
		arm._t.position = pos;
	}
	
	int lvl = 0;
	public void MosquitoDied() {
		if (_swarm.Count <= 0) {
			++lvl;
			Application.LoadLevel(lvl % 2);
		}
	}
	
}
