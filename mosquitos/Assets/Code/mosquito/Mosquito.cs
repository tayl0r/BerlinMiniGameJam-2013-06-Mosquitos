using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mosquito : MonoBehaviour {
	
	public Transform _t;
	public GameObject _go;
	public tk2dAnimatedSprite _body;
	public tk2dSprite _bloodSaq;
	
	public Vector3 _position;
	public Vector3 _targetPosition;
	public static float _swarmSize;
	public static float _speed;
	public Vector3 _moveVector;
	public float _xDir;
	public MosquitoManager _manager;
	public SkinArea _feedingSkin;
	
	public float _deathTime;
	
	public bool _feeding;
	public bool _movingToTarget;
	
	public AudioSource _soundSource;
	public Transform _soundT;
	
	void Awake() {
		_t = transform;
		_go = gameObject;
		_deathTime = Time.time + 10f;
	}
	
	void Start() {
		StartCoroutine(MoveToSwarm());
		
		_soundT.localPosition = new Vector3(-512f, -384f, 0f);
		Vector3 soundPos = _soundT.position;
		soundPos.z = MosquitoManager.instance._cameraT.position.z;
		_soundT.position = soundPos;
		_soundSource.pitch = Random.Range(.5f, 1f);
	}
	
	public void Setup(MosquitoManager manager) {
		_position = Random.insideUnitSphere * _swarmSize;
		_position.z = 0f;
		_moveVector = Random.insideUnitSphere;
		_moveVector.z = 0f;
		_manager = manager;
		
		_targetPosition = _position;
		//_t.localPosition = _position;
	}
	
	void Update() {
		if (_feeding || _movingToTarget) {
			
		}
		else {
			if (Time.time >= _deathTime) {
				StartCoroutine(Die());
			}
			float distance = Vector3.Distance(_position, Vector3.zero);
			float lerpFactor = distance / _swarmSize;
			
			Vector3 rnd = Random.insideUnitSphere * .3f;
			rnd.z = 0f;
			_moveVector = Vector3.Lerp(_moveVector, -_position.normalized, lerpFactor) + rnd;
			_moveVector.z = 0f;
			_moveVector.Normalize();
			
			_position += _moveVector * _speed * Time.deltaTime;
			_t.localPosition = _position;
			
			_xDir = _xDir + (_moveVector.x - _xDir) / 10f;
			
			if (_xDir >= 0f) {
				_t.localScale = new Vector3(-1f, 1f, 1f);
			} else {
				_t.localScale = new Vector3(1f, 1f, 1f);
			}
		}
	}
	
	IEnumerator Die() {
		_manager._swarm.Remove(this);
		_movingToTarget = true;
		_body.Play("dead");
		Vector3 target = _t.position;
		target.y = -200f;
		Vector3 speed = Vector3.zero;
		float totalTime = (_t.position.y + 200f) / 2000f;
		while (_t.position.y >= -100f) {
			_t.position = Vector3.SmoothDamp(_t.position, target, ref speed, totalTime);
			yield return 1;
		}
		Destroy(_go);
		_manager.MosquitoDied();
	}
	
	void OnTriggerEnter(Collider other) {
		if (_feeding || _movingToTarget) { return; }
		SkinArea skin = other.GetComponent<SkinArea>();
		if (skin != null) {
			if (skin._mosquitoCount < skin._supportedMosquitos) {
				skin._mosquitoCount++;
				if (skin._mosquitoCount >= skin._supportedMosquitos) {
					skin.collider.enabled = false;
				}
				_movingToTarget = true;
				_manager.SetFeeding(this, true);
				_feedingSkin = skin;
				StartCoroutine(MoveToFeed(skin.transform.position + new Vector3(Random.Range(-30f, 30f), Random.Range(-30f, 30f), 0f)));
			}
		} else {
			GiantArm arm = other.GetComponent<GiantArm>();
			if (arm != null) {
				StartCoroutine(Die());
			}
		}
	}
	
	IEnumerator MoveToFeed(Vector3 targetPosition) {
		while (_t.position != targetPosition) {
			float dist = Vector3.Distance(_t.localPosition, targetPosition);
			float speed = _speed * 1.25f;
			if (dist <= 100f) {
				speed = Mathf.Lerp(speed, speed * .25f, dist / 100f);
			}
			_t.position = Vector3.MoveTowards(_t.position, targetPosition, speed * Time.deltaTime);
			yield return 1;
		}
		_manager.AddBiteMark(targetPosition);
		_movingToTarget = false;
		StartCoroutine(Feed());
	}
	
	IEnumerator Feed() {
		_feeding = true;
		_body.Play("feed");
		float time = 2f;
		while (time > 0f) {
			time -= Time.deltaTime;
			yield return 1;
		}
		_feeding = false;
		_feedingSkin = null;
		_manager.SpawnChild(_t.position);
		StartCoroutine(MoveToSwarm());
	}
	
	IEnumerator MoveToSwarm() {
		_movingToTarget = true;
		_body.Play("fly");
		_manager.SetFeeding(this, false);
		while (_t.localPosition != _position) {
			float dist = Vector3.Distance(_t.localPosition, _position);
			float speed = _speed * 1.25f;
			if (dist <= 100f) {
				speed = Mathf.Lerp(speed, speed * .25f, dist / 100f);
			}
			Vector3 previous = _t.localPosition;
			_t.localPosition = Vector3.MoveTowards(_t.localPosition, _position, speed * Time.deltaTime);
			if (previous.x > _t.localPosition.x) {
				_t.localScale = new Vector3(1f, 1f, 1f);
			} else {
				_t.localScale = new Vector3(-1f, 1f, 1f);
			}
			yield return 1;
		}
		_movingToTarget = false;
	}
	
//	public void Move(int idx, List<Mosquito> swarm) {
//		if (!_zombie) {
//			Flock(idx, swarm);
//		}
//		else {
//			//Hunt(swarm);
//		}
//		CheckBounds();
//		CheckSpeed();
//		_position.x += _dX * Time.deltaTime;
//		_position.y += _dY * Time.deltaTime;
//		_t.localPosition = _position;
//	}
//	
//	private void Flock(int idx, List<Mosquito> boids) {
//		foreach (Mosquito boid in boids) {
//			float distance = Vector3.Distance(_position, boid._position);
//			if (boid != this && !boid._zombie) {
//				if (distance < _space) {
//					// Create space.
//					_dX += (_position.x - boid._position.x) * 10f * Time.deltaTime;
//					_dY += (_position.y - boid._position.y) * 10f * Time.deltaTime;
//				}
////				else if (distance < _sight) {
////					// Flock together.
////					_dX += (boid._position.x - _position.x) * 10f * Time.deltaTime;
////					_dY += (boid._position.y - _position.y) * 10f * Time.deltaTime;
////				}
////				if (distance < _sight) {
////					// Align movement.
////					_dX += boid._dX * 10f * Time.deltaTime;
////					_dY += boid._dY * 10f * Time.deltaTime;
////				}
//			}
//			
//			
////			if (boid._zombie && distance < _sight) {
////				// Avoid zombies.
////				_dX += (_position.x - boid._position.x) * Time.deltaTime;
////				_dY += (_position.y - boid._position.y) * Time.deltaTime;
////			}
//		}
//	}
//	
////	private void Hunt(List<Mosquito> boids) {
////		float range = float.MaxValue;
////		Mosquito prey = null;
////		foreach (Mosquito boid in boids) {
////			if (!boid._zombie) {
////				float distance = Vector3.Distance(_position, boid._position);
////				if (distance < sight && distance < range) {
////					range = distance;
////					prey = boid;
////				}
////			}
////		}
////		if (prey != null) {
////			// Move towards closest prey.
////			dX += (prey._position.X - _position.x) * Time.deltaTime;
////			dY += (prey._position.Y - _position.Y) * Time.deltaTime;
////		}
////	}
//	
//	private void CheckBounds() {
//	float val = _boundary - _border;
//	if (_position.x < _border) _dX += (_border - _position.x) * Time.deltaTime;
//	if (_position.y < _border) _dY += (_border - _position.y) * Time.deltaTime;
//	if (_position.x > val) _dX += (val - _position.x) * Time.deltaTime;
//	if (_position.y > val) _dY += (val - _position.y) * Time.deltaTime;
//	}
//	
//	private void CheckSpeed() {
//		float s;
//		if (!_zombie) s = _speed;
//		else s = _speed / 4f;
//		float val = Vector3.Distance(Vector3.zero, new Vector3(_dX, _dY, 0f));
//		if (val > s) {
//			_dX = _dX * s / val;
//			_dY = _dY * s / val;
//		}
//	}

}
