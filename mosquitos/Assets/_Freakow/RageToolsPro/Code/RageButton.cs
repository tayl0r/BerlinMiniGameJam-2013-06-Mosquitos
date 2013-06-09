using UnityEngine;

[AddComponentMenu("RageTools/Rage Button")]
public class RageButton : MonoBehaviour {

	[SerializeField]private RageButtonData _data;
	public RageButtonData Data {
		get {
			if (_data == null) _data = ScriptableObject.CreateInstance<RageButtonData>();
			return _data;
		}
		set { _data = value; }
	}
	public bool AdvancedOptions = true;
	private bool _isHovering;

	public void Start() {
		Data.InitTargets (gameObject);
		Data.ClickTweeners.InitializeTweenerList(Data.Click.Target, Data.Click.TweenId);
		Data.HoverInTweeners.InitializeTweenerList(Data.HoverIn.Target, Data.HoverIn.TweenId);
		Data.HoverOutTweeners.InitializeTweenerList(Data.HoverOut.Target, Data.HoverOut.TweenId);
	}

	public void Update() {

		if (Data.ClickOnAwake) {
			Data.ClickOnAwake = false;
			Click();
			return;
		}

		if (collider.NotColliding()) {
			if (!_isHovering) return;
			_isHovering = false;
			HoverOut();
			return;
		}
		if (Input.GetMouseButtonDown(0)) {
			Click();
			return;
		}
		if (_isHovering) return;

		HoverIn();
		_isHovering = true;
	}

	public void Click() {
		Data.Click.Sound.PlayCheck();
		Data.ClickTweeners.PlayTweens(Data.Click.PlayReverse, Data.Click.Reset);
		ClickCallbacks();
		if (AdvancedOptions) Data.ShowAndHide();
		if (Data.UnloadUnusedAssets)
			Resources.UnloadUnusedAssets();
	}
	public void HoverIn() {
		Data.HoverIn.Sound.PlayCheck();
		Data.HoverInTweeners.PlayTweens(Data.HoverIn.PlayReverse, Data.HoverIn.Reset);
		//OnHoverInCallbacks();
	}
	public void HoverOut() {
		Data.HoverOut.Sound.PlayCheck();
		Data.HoverOutTweeners.PlayTweens(Data.HoverOut.PlayReverse, Data.HoverOut.Reset);
		//OnHoverOutCallbacks();
	}

	/// <summary> Used when the user has assigned IRageButton-extended scripts </summary>
	public void ClickCallbacks() {
		foreach (MonoBehaviour script in Data.ToExecuteOnClick)
			script.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);

		foreach (GameObject gO in Data.ToEnableOnClick)
#if UNITY_3_5
			gO.active = true;
#else
			gO.SetActive(true);
#endif

		foreach (GameObject gO in Data.ToInstantiateOnClick)
			Instantiate(gO, transform.position, Quaternion.identity);

		foreach (GameObject gO in Data.ToDisableOnClick)
#if UNITY_3_5
			gO.active = false;
#else
			gO.SetActive(false);
#endif
	}
//  TODO:
// 	public void OnHoverInCallbacks() {
// 		foreach (MonoBehaviour script in Data.ToExecuteOnHoverIn)
// 			script.SendMessage("OnHoverIn");
// 	}
// 	public void OnHoverOutCallbacks() {
// 		foreach (MonoBehaviour script in Data.ToExecuteOnHoverOut)
// 			script.SendMessage("OnHoverOut");
// 	}

}
