using UnityEngine;
using System.Collections;

[System.Serializable]
public class AudioPlayerClipCollectionData : System.Object {
	
	public AudioPlayer.SoundGroup _group;
	public AudioPlayer.SoundType _type;
	public AudioClip[] _clips;
	
	public bool _music;
	public bool _looping;
	
	public float _volume = 1f;

	public AudioPlayerClipCollectionData(AudioPlayer.SoundGroup soundGroup, AudioPlayer.SoundType soundType, bool music, AudioClip[] clips) {
		_group = soundGroup;
		_type = soundType;
		_music = music;
		_clips = clips;
	}
	
}
