using UnityEngine;

public class RageEdgetuneData : ScriptableObject {

	public float AaFactor = 1.0f;
	public float DensityFactor = 0.5f;
	public float PerspectiveBlur = 3f;
	public bool AutomaticLod = true;
	public int MaxDensity = 7;
	//public float PixelPerfectHeight;

	public RageEdgetuneData Clone() { return (RageEdgetuneData) MemberwiseClone();}
}
