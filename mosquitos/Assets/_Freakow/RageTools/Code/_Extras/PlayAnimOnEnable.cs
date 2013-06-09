using UnityEngine;

public class PlayAnimOnEnable : MonoBehaviour {
	public float AnimationSpeed = 1f;
	private float _currentAnimationSpeed = 1f;
	public Animation animationComponent;
	public string clipName;

	void OnEnable() {
		if (!animationComponent.IsPlaying(clipName)) animationComponent.Play(clipName);
	}

	public void Update() {
		if (Mathf.Approximately(_currentAnimationSpeed, AnimationSpeed)) return;
		_currentAnimationSpeed = AnimationSpeed;
		foreach (AnimationState animState in animationComponent)
			animState.speed = AnimationSpeed;
	}

}
