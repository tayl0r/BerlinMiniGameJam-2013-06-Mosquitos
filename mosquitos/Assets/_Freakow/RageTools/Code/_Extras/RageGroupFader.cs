using UnityEngine;

[RequireComponent(typeof(RageGroup))]
public class RageGroupFader : MonoBehaviour {

	private RageGroup _group;
	public bool OnStart;
	public float Delay;
	public float FadeDuration;
	private float _time;						// This is made private just so you can monitor its values in unity

	private float _fadeStartTime;
	private float _fadeEndTime, _fadePassedTime;


	public void Awake() {
		_group = GetComponent<RageGroup>();
	}

	public void Start() {
		if (!OnStart)
			return;
		Fade();
	}

	/// <summary> Use this method as a callback to be fired from an animation clip or another script 
	/// Setting _fadeStartTime is what makes the Update controller active </summary>
	public void Fade() {
		if (_group == null)
			_group = GetComponent<RageGroup>();
		_fadeStartTime = Time.time+Delay;
		_fadeEndTime = _fadeStartTime+FadeDuration;
	}

	public void Update() {
		_time = Time.time;
		if (_time < _fadeStartTime) 
			return;

		if (_fadeEndTime >= _time) {
			_fadePassedTime = _time - _fadeStartTime;
			// From fade end to start time, gets a value from 0 to 1
			var fadeFactor=1-Mathf.InverseLerp(_fadeStartTime, _fadeEndTime, _fadePassedTime);
			SetGroupTransparency(fadeFactor);
		}
	}

	private void SetGroupTransparency(float fadeFactor) {
		foreach (var item in _group.List) {
		    Color thisColor = item.Spline.Rs.GetFillColor1();
			thisColor.a=thisColor.a*fadeFactor;
			item.Spline.Rs.SetFillColor1(thisColor);
			thisColor=item.Spline.Rs.GetFillColor2();
			thisColor.a=thisColor.a*fadeFactor;
			item.Spline.Rs.SetFillColor2(thisColor);

			thisColor = item.Spline.Rs.GetOutlineColor1();
			thisColor.a=thisColor.a*fadeFactor;
			item.Spline.Rs.SetOutlineColor1(thisColor);
			thisColor = item.Spline.Rs.GetOutlineColor2();
			thisColor.a=thisColor.a*fadeFactor;
			item.Spline.Rs.SetOutlineColor2(thisColor);

			// RageSpline Mesh doesn't change at all, so we refresh it as cheaply as possible
			item.Spline.Rs.RefreshMesh(false, false, false);
		}
	}
}
