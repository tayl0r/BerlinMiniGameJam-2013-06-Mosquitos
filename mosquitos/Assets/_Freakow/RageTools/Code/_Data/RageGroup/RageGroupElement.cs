using UnityEngine;

[System.Serializable]
public class RageGroupElement: ScriptableObject {
	
	public RageSplineAdapter Spline;
	public float DefaultAa;
	public int DefaultDensity;
	public Color DefaultFillColor1;
	public Color DefaultFillColor2;
	public Color DefaultOutlineColor;
	public Vector3 PositionCache;
	public RageGroupPointCache[] GroupPointCache;
	public Vector3 GradientOffsetCache;
	public Quaternion RotationCache;
	public Vector3 ScaleCache;
	public Vector2 TextureOffsetCache;
	public float TextureAngleCache;
	public float TextureScaleCache;
	public Vector2 TextureOffsetCache2;
	public float TextureAngleCache2;
	public float TextureScaleCache2;
	public MeshRenderer MeshRenderer;
	[SerializeField]private bool _visible;
	public bool Visible {
		get { return _visible; }
		set {
			MeshRenderer.enabled = value;
			_visible = value;
		}
	}

	public RageGroupElement() {
		Spline = null;
		DefaultAa = 0;
		DefaultDensity = 0;
		DefaultFillColor1 = Color.black;
		DefaultFillColor2 = Color.black;
		DefaultOutlineColor = Color.black;
		PositionCache = Vector3.zero;
		GroupPointCache = null;
		GradientOffsetCache = Vector3.zero;
		RotationCache = Quaternion.identity;
		ScaleCache = Vector3.zero;
		TextureOffsetCache = Vector2.zero;
		TextureAngleCache = 0f;
		TextureScaleCache = 0.1f;
		TextureOffsetCache2 = Vector2.zero;
		TextureAngleCache2 = 0f;
		TextureScaleCache2 = 0.1f;
		MeshRenderer = null;
	}


}
