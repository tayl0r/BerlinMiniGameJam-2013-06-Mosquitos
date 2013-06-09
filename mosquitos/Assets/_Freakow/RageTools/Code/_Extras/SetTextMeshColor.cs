using UnityEngine;

[ExecuteInEditMode]
public class SetTextMeshColor : MonoBehaviour {

	public Color TextColor = Color.black;
	private Material _material ;

	void OnEnable () {
		_material = GetComponent<MeshRenderer>().sharedMaterials[0];
		_material.color = TextColor;
	}

	void Update() {
		if (Application.isPlaying) return;
		_material.color = TextColor;
	}
}
