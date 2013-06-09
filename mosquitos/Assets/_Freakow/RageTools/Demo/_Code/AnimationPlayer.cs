using UnityEngine;
using System.Collections;

public class AnimationPlayer : MonoBehaviour {
	public float AnimationSpeed = 1f;
	private float _currentAnimationSpeed = 1f;
	private Animation _anim;

	void Start () {
		_anim = GetComponent<Animation>();
		UpdateAnimSpeed();
		_anim.Play();
	}

	public void Update() {
		if (_currentAnimationSpeed == AnimationSpeed) return;
		_currentAnimationSpeed = AnimationSpeed;
		UpdateAnimSpeed();
	}

	private void UpdateAnimSpeed() {
		foreach (AnimationState animState in _anim)
			animState.speed = AnimationSpeed;
	}
}
