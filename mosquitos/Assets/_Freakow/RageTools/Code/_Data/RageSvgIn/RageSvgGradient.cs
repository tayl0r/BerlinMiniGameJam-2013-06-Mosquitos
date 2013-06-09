using UnityEngine;

public class RageSvgGradient : ScriptableObject {

	public string Id = "";
	public enum GradientType { Linear = 0, Radial };
	public GradientType Type;
	public float X1;
	public float X2;
	public float Y1;
	public float Y2;
	public Color StartColor;
	public Color EndColor;

	private RageSvgGradient() {
		Type = GradientType.Linear;
		X1 = X2 = Y1 = Y2 = 0f;
		Id = ""; 
		StartColor = EndColor = Color.black;
	}

	public void CopyDataFrom(RageSvgGradient src) {
		Id = src.Id;
		Type = src.Type;
		X1 = src.X1;
		X2 = src.X2;
		Y1 = src.Y1;
		Y2 = src.Y2;
		StartColor = src.StartColor;
		EndColor = src.EndColor;
	}

	public static RageSvgGradient NewInstance() {
		return (RageSvgGradient)CreateInstance(typeof(RageSvgGradient));
	}

	public void SetStartPos(Vector2 position) {
		X1 = position.x;
		Y1 = -position.y;
	}

	public void SetEndPos(Vector2 position) {
		X2 = position.x;
		Y2 = -position.y;
	}
}