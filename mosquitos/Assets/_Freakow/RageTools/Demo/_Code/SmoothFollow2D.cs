using UnityEngine;
using System.Collections;

public class SmoothFollow2D : MonoBehaviour {

	public Transform target;
	public float smoothTime = 0.3f;
	private Transform thisTransform;
	private Vector2 velocity;

	void Start () {
		thisTransform = transform;
	}
	
	void Update () {
		thisTransform.position = new Vector3 (Mathf.SmoothDamp(thisTransform.position.x,target.position.x, ref velocity.x, smoothTime),
											  Mathf.SmoothDamp(thisTransform.position.y,target.position.y, ref velocity.y, smoothTime));
	}
}
