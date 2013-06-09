#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AudioPlayerEditor : ScriptableObject {

    [MenuItem ("Utilities/AudioPlayer/SortSoundList")]
    static void  SortSoundList() {
		List<AudioPlayerClipCollectionData> setup = new List<AudioPlayerClipCollectionData>(AudioPlayer.instance._setup);
		setup.Sort(delegate(AudioPlayerClipCollectionData xx, AudioPlayerClipCollectionData yy) {
			if (xx._type > yy._type) { return 1; }
			if (xx._type < yy._type) { return -1; }
			return 0;
		});
		AudioPlayer.instance._setup = new AudioPlayerClipCollectionData[setup.Count];
		setup.CopyTo(AudioPlayer.instance._setup);
		Debug.Log("sorted " + setup.Count + " entries.");
	}
	
}
#endif