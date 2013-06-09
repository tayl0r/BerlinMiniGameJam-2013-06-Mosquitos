using UnityEngine;

public class RageGroupGradientData : ScriptableObject {

	public Color StartColor = Color.black;
	public Color EndColor = Color.blue;
	public Vector2 Offset = Vector2.zero;
	public float Scale = 1f;
	public float Angle;
	public bool StyleLocalPositioning = true;

	public void AssignGradient(ISplineGradient splineGradient) {
		splineGradient.Angle = Angle;
		splineGradient.EndColor = EndColor;
		splineGradient.Offset = Offset;
		splineGradient.Scale = Scale;
		splineGradient.StartColor = StartColor;
		splineGradient.StyleLocalPositioning = StyleLocalPositioning;
	}
}
