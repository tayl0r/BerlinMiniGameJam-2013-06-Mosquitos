using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DragObject : MonoBehaviour {
	public bool TwoDee;
	public bool DragOnly;
	private Vector3 screenPoint;
	private Vector3 offset;

	void OnMouseDown() {
		if (!DragOnly) return;
		screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
		var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
		offset = gameObject.transform.position - worldPosition;
	}

	void OnMouseDrag() {
		if (!DragOnly) return;
		var curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
		var curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
		transform.position = TwoDee? new Vector3(curPosition.x, curPosition.y, transform.position.z) : curPosition;
	}

	void Update() {
		if (DragOnly) return;
		if (!Input.GetMouseButton(0)) return;
		Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (transform.position.z - Camera.main.transform.position.z)));
		point.z = transform.position.z;
		transform.position = point;
	}
}

