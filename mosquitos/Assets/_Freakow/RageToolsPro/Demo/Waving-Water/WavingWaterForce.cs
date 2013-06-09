using UnityEngine;
using System.Collections;

public class WavingWaterForce : MonoBehaviour {
	public float PushForce = -1f;

	public IEnumerator Start () {
		yield return new WaitForSeconds(1.5f);
		var rigidBody = GetComponent<Rigidbody>();
		rigidBody.AddForce (0, PushForce, 0);
	}
	
}
