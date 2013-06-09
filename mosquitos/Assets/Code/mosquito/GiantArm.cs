using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class GiantArm : MonoBehaviour {

	public Transform _t;
	public GameObject _go;
	public Vector3 _targetPos;
	public tk2dSprite _sprite;
	
	void Awake() {
		_t = transform;
		_go = gameObject;
	}
	
	void Start() {
		_targetPos = _t.position;
		_t.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
		float scale = 3f;
		_t.localScale = new Vector3(scale, scale, 1f);
		_t.Translate(new Vector3(2000f, 0f, 0f), Space.Self);
		
		StartCoroutine(MoveArmIn());
	}
	
	IEnumerator MoveArmIn() {
		Vector3 oPos = _t.position;
		HOTween.To(_t, 2f, new TweenParms().Prop("position", _targetPos).Ease(EaseType.EaseInOutCubic));
		yield return new WaitForSeconds(2f);
		HOTween.To(_t, .25f, new TweenParms().Prop("localScale", new Vector3(1f, 1f, 1f)));
		Vector3 newTarget = _targetPos;
		newTarget.z = MosquitoManager.instance._swarmParent.position.z;
		HOTween.To(_t, .25f, new TweenParms().Prop("position", newTarget).Ease(EaseType.EaseInOutCubic));
		yield return new WaitForSeconds(1f);
		HOTween.To(_t, 2f, new TweenParms().Prop("position", oPos).Ease(EaseType.EaseInOutCubic));
		yield return new WaitForSeconds(2f);
		HOTween.To(_sprite, 2f, new TweenParms().Prop("color", new Color(1f, 1f, 1f, 0f)).Ease(EaseType.Linear));
		yield return new WaitForSeconds(3f);
		Destroy(_go);
	}
	
	void OnDestroy() {
		MosquitoManager.instance._armSpawned = false;
	}
	
}
