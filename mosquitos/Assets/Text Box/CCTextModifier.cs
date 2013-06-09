/*
	Copyright 2012, Catlike Coding
	http://catlikecoding.com/
	Version 1.0.2
	
	1.0.2: Asset reset now triggers UpdateAllCCText.
	1.0.0: Initial version.
*/

using UnityEngine;

/// <summary>
/// Base class for CCText modifiers.
/// Make your own modifier by extending this class and writing your own Modify method.
/// CCText calls Modify before it sends vertices and UV to the mesh.
/// CCText does not send colors to the mesh, neither does it update them. So if you modify
/// colors, assume they are undefined and assign them to the mesh yourself.
/// </summary>
public abstract class CCTextModifier : ScriptableObject {
	
	/// <summary>
	/// Modify a CCText.
	/// </summary>
	/// <param name="text">
	/// A <see cref="CCText"/>.
	/// </param>
	public abstract void Modify(CCText text);
	
	/// <summary>
	/// Update all CCText that use this modifier.
	/// </summary>
	public void UpdateAllCCText () {
		foreach(CCText box in FindObjectsOfType(typeof(CCText))){
			if(box.Modifier == this){
				box.UpdateText();
			}
		}
	}
	
#if UNITY_EDITOR
	
	void Reset () {
		UpdateAllCCText();
	}
	
#endif
	
}
