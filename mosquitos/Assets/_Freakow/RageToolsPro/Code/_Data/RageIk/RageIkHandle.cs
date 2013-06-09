using UnityEngine;
[ExecuteInEditMode]
public class RageIkHandle : MonoBehaviour {

	public RageIk RageIk;
	public bool ShowGizmo = true;
	public bool AlignEnd {
		get { return RageIk.Chain.AlignEnd; }
		set { RageIk.Chain.AlignEnd = value; }
	}
	[SerializeField]private bool _lastOn;
	public bool On {
		get { return RageIk && RageIk.On; }
		set { if (RageIk) RageIk.On = value; }
	}

	public void OnDrawGizmos() {
		if (!ShowGizmo) return;
		Gizmos.DrawIcon(transform.position, On ? "iktarget.png" : "ikofftarget.png");
	}

	[ExecuteInEditMode]
	public void Update() {
		if (RageIk == null) return;

		if (_lastOn != On) {
			_lastOn = RageIk.On = On;
			return;
		}

		if (RageIk.On != On) _lastOn = On = RageIk.On;
	}
}
