using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioFadeController : System.Object {
	public bool fading = false;
}

[ExecuteInEditMode]
public class AudioPlayer : MonoBehaviour {
	
	public float _masterVolumeMusic = 1f;
	public float _masterVolumeSound = 1f;
	
	public AudioPlayerAdditionalClipSource[] _moreSetups;
	public AudioPlayerClipCollectionData[] _setup;
	
	Dictionary<SoundType, AudioPlayerClipCollectionData> _soundMap = new Dictionary<SoundType, AudioPlayerClipCollectionData>();
	Dictionary<SoundGroup, Queue<AudioSource>> _sources = new Dictionary<SoundGroup, Queue<AudioSource>>();
	Dictionary<SoundType, SoundGroup> _typeToGroupMap = new Dictionary<SoundType, SoundGroup>();
	
	Dictionary<SoundGroup, bool> _groupEnabled = new Dictionary<SoundGroup, bool>();
	Dictionary<SoundGroup, float> _groupVolume = new Dictionary<SoundGroup, float>();
	
	Dictionary<SoundGroup, int> _poolSizes = new Dictionary<SoundGroup, int> {
		{SoundGroup.SlotSounds, 20},
		{SoundGroup.EverythingElse, 0},
		{SoundGroup.Important, 20},
		{SoundGroup.Music, 1}
	};
	
	Dictionary<SoundType, AudioSource> _loopingSounds = new Dictionary<SoundType, AudioSource>();
	Dictionary<AudioSource, AudioFadeController> _fadeControllers = new Dictionary<AudioSource, AudioFadeController>();
	
	public AudioSource _musicAudioSource;
	public bool _musicFaded;
	
	//HashSet<AudioSource> _fadingAudioSources = new HashSet<AudioSource>();
	
	public enum SoundType {
		None = 0,
		UI_ButtonClick = 1, //		ButtonPress,
		
		Menu_BounceClosed = 100, //UI_BounceClosed
		Menu_SlideClosedOut, //UI_SlideClosedOut
		Menu_SlideOpenIn, //UI_SlideOpenIn


		BuyScreen_PinDrop = 200, //UI_PinDrop
		BuyScreen_Shuffle,

		Collection_PickedPiece = 600,

		Bonus_SymbolHit=700, //BonusSymbolHit
		Bonus_SuspenseSequence, //Reel_SuspenseEndSequence
		Bonus_SuspenseEndBust, //Reel_SuspenseEndBust
		Bonus_SuspenseEndWin, //Reel_SuspenseEndWin
		BonusGame_SelectObject1,
		BonusGame_SelectObject2,
		BonusGame_SelectObject3,
		
		Jackpot_Start = 800,
		Jackpot_ChestOpen,
		Jackpot_ChestClose,
		Jackpot_ShimmerLoop,
		Jackpot_DiceRollStart,
		Jackpot_DiceRollEnd,
		Jackpot_StarExplode,
		
		BigWin = 875,

		WinAccent1 = 900,
		WinAccent2,
		WinAccent3,
		WinAccents,
		WinAccentBig,
		CoinAccent,
		CoinAccentEnd,
		StarAccent,
		
		BoostAccent = 950,
		
		Tropical_MusicLoop = 1000, //		Music_TropicalLoop,
		Tropical_BonusGame_MusicLoop, //bonusgame_tropical
		Tropical_BonusGame_FadeOut, //BonusGame_FadeOut,
		Tropical_BonusGame_TreeShake, //bonusgame_tropical
		Tropical_BonusGame_MonkeyShake,
		Tropical_WinAccent1,
		Tropical_WinAccent2,
		Tropical_Reel_Start,
		Tropical_Reel_Stop,
		Tropical_Reel_FinalStop,
		Tropical_Reel_SpinLoop,

		Egypt_MusicLoop = 1100, //Music_EgyptLoop = 1200,
		Egypt_BonusGame_MusicLoop, //BonusGame_Egypt_Music,
		Egypt_BonusGame_PyramidFade,
		Egypt_BonusGame_PyramidShort, //BonusGame_Egypt_PyramidShort,
		Egypt_BonusGame_PyramidLong, //BonusGame_Egypt_PyramidLong,
		
		Fairy_MusicLoop = 1200, //	Music_FairyLoop,
		Fairy_BonusGame_MusicLoop, //BonusGame_Fairy_Music
		Fairy_BonusGame_GnomeFade,
		Fairy_BonusGame_GnomeShort,
		Fairy_BonusGame_GnomeLong,		
		
		West_MusicLoop = 1300,
		West_BonusGame_MusicLoop,
		West_BonusGame_HorseFade,
		West_BonusGame_HorseStomp,
		West_BonusGame_CowboyShoot,
		
		Pachinko_Music = 1400,
		Pachinko_BallSpawn,
		Pachinko_BallBounce,
		Pachinko_BallLost,
		Pachinko_BallScore
	}
	
	// lower id group = higher priority
	public enum SoundGroup {
		Music,
		Important,
		EverythingElse,
		SlotSounds
	}
	
	public static AudioPlayer instance;
	void Awake() {
		Debug.Log("AudioPlayer - Awake");
		instance = this;
		
		// clear existing data
		_soundMap.Clear();
		_groupVolume.Clear();
		_groupEnabled.Clear();
		_sources.Clear();
		_fadeControllers.Clear();
		_typeToGroupMap.Clear();
		
		if (Application.isPlaying == false) { return; }

//		// delete existing audio source game objects
//		AudioSource[] existingSources = GetComponentsInChildren<AudioSource>(true);
//		foreach (var eSource in existingSources) {
//			DestroyImmediate(eSource.gameObject);
//		}
		
		foreach (AudioPlayerClipCollectionData child in _setup) {
			//child._volume = 1f;
			
			// error if sound type already exists in map
			if (_soundMap.ContainsKey(child._type)) {
				Debug.LogError("already have sound of this type: " + child._type.ToString());
			}
			
			// add to sound map
			_soundMap.Add(child._type, child);
			
			if (_sources.ContainsKey(child._group) == false) {
				// new group - create new audiosource pool
				var sources = new Queue<AudioSource>();
				_sources.Add(child._group, sources);
				
				// enable it
				_groupEnabled.Add(child._group, true);
				_groupVolume.Add(child._group, 1f);
				
				// add audiosources to this pool
				for (int i = 0; i < _poolSizes[child._group]; ++i) {
					var go = new GameObject("audio source: " + child._group.ToString() + " " + sources.Count);
					go.transform.parent = this.transform;
					var source = go.AddComponent<AudioSource>();
					sources.Enqueue(source);
					_fadeControllers.Add(source, new AudioFadeController());
				}
			}
			
			// add to TypeToGroupMap
			_typeToGroupMap.Add(child._type, child._group);
		}
	}
	
	#region actually plays the sound
	
//	IEnumerator PlayMusicClip(float volume, AudioClip clip, SoundType soundType, float delay = 0f, System.Action<AudioSource> sourceCb = null) {
//		if (delay > 0f) {
//			yield return new WaitForSeconds(delay);
//		}
//
//		// get sound group
//		var soundGroup = _typeToGroupMap[soundType];
//
//		if (_groupEnabled[soundGroup] == false) { yield break; }
//		
//		// get audio source
//		var audioSource = _sources[soundGroup].Dequeue();
//		if (audioSource == null) {
//			Debug.LogError("audio source is null for looping clip. this should never happen");
//		} else {
//			// immediately add back to queue
//			_sources[soundGroup].Enqueue(audioSource);
//			
//			if (audioSource.isPlaying) {
//				// fade the current song out
//				float time = 0f;
//				float totalTime = .5f;
//				float t = 0f;
//				float startVolume = audioSource.volume;
//				float endVolume = 0f;
//				while (t < 1f) {
//					time += Time.deltaTime;
//					t = time / totalTime;
//					audioSource.volume = Mathf.Lerp(startVolume, endVolume, t);
//					yield return 1;
//				}
//				audioSource.Stop();
//			}
//			
//			_musicAudioSource = audioSource;
//			
//			// setup new song
//			audioSource.clip = clip;
//			audioSource.priority = (int)soundGroup;
//			audioSource.volume = volume * _groupVolume[soundGroup];
//			audioSource.loop = true;
//			audioSource.pitch = 1f;
//			
//			// play it
//			audioSource.Play();
//			
//			// send audio source to callback
//			// this is the easiest way to return the audio source we used to the caller so they can stop it later
//			if (sourceCb != null) {
//				sourceCb(audioSource);
//			}
//		}
//	}
	
	IEnumerator PlaySoundClip(float volume, AudioClip clip, SoundType soundType, bool loop, float delay, System.Action<AudioSource> sourceCb, bool music, float pitch, float jumpToTimeInClip) {
		if (delay > 0f) {
			yield return new WaitForSeconds(delay);
		}
		
		// get sound group
		var soundGroup = _typeToGroupMap[soundType];

		if (_groupEnabled[soundGroup] == false) { yield break; }
		
		// get audio source
		if (_sources[soundGroup].Count > 0) {
			//Debug.Log("playing " + soundGroup);
			var audioSource = _sources[soundGroup].Dequeue();
			bool playClip = true;
			
			if (music) {
				// immediately add back to queue
				_sources[soundGroup].Enqueue(audioSource);
				
				if (audioSource.isPlaying && audioSource.clip == clip) {
					playClip = false;
				}
				
				if (audioSource.isPlaying && audioSource.clip != clip) {
					Debug.Log("fading out clip");
					// fade the current song out
					float time = 0f;
					float totalTime = .5f;
					float t = 0f;
					float startVolume = audioSource.volume;
					float endVolume = 0f;
					while (t < 1f) {
						time += Time.deltaTime;
						t = time / totalTime;
						audioSource.volume = Mathf.Lerp(startVolume, endVolume, t);
						yield return 1;
					}
					audioSource.Stop();
				}
				
				_musicAudioSource = audioSource;
				loop = true;
			}
			
			if (playClip) {
				//Debug.Log("playing " + clip.name);
				// setup audio source
				audioSource.clip = clip;
				audioSource.priority = (int)soundGroup;
				if (music == false) {
					audioSource.volume = volume * _groupVolume[soundGroup] * _masterVolumeSound;
				} else {
					audioSource.volume = 0f;
				}
				audioSource.loop = loop;
				audioSource.pitch = pitch;
				if (jumpToTimeInClip > 0f) {
					audioSource.timeSamples = Mathf.RoundToInt(clip.frequency * jumpToTimeInClip);
				}
				
				// play it!
				audioSource.Play();
			}
			
			// send audio source to callback
			// this is the easiest way to return the audio source we used to the caller so they can stop it later
			if (sourceCb != null) {
				sourceCb(audioSource);
			}
			
			// handle looping
			if (loop) {
				if (_loopingSounds.ContainsKey(soundType)) {
					_loopingSounds[soundType] = audioSource;
				} else {
					_loopingSounds.Add(soundType, audioSource);
				}
			}
			
			if (playClip && music) {
				// fade in if we have music
				float time = 0f;
				float totalTime = .5f;
				float t = 0f;
				float startVolume = audioSource.volume;
				float endVolume = volume * _groupVolume[soundGroup] * _masterVolumeSound;
				while (t < 1f) {
					time += Time.deltaTime;
					t = time / totalTime;
					audioSource.volume = Mathf.Lerp(startVolume, endVolume, t);
					yield return 1;
				}
			}
			
			// wait for sound to finish
			yield return new WaitForSeconds(clip.length + .1f);
			while (audioSource.isPlaying) {
				yield return new WaitForSeconds(.25f);
			}
			
			// return audio source to pool
			_sources[soundGroup].Enqueue(audioSource);
		} else {
			//Debug.Log("skipping sound because no more audio sources: " + soundType);
			yield break;
		}
	}
	
	IEnumerator StopSoundClipCor(AudioSource source, float fadeTime, float finalVolume, float delay) {
		if (delay > 0f) {
			yield return new WaitForSeconds(delay);
		}
		if (_fadeControllers.ContainsKey(source) == false) {
			yield break;
		}
		AudioFadeController controller = _fadeControllers[source];
		
		if (controller.fading) {
			// we're already fading, so stop the current one
			controller.fading = false;
			yield return 1;
		}
		controller.fading = true;
		
		float time = 0f;
		float t = 0f;
		float startVolume = source.volume;
		while (t < 1f && source.isPlaying && controller.fading) {
			time += Time.deltaTime;
			t = time / fadeTime;
			source.volume = Mathf.Lerp(startVolume, finalVolume, t);
			yield return 1;
		}
		
//		if (_fadingAudioSources.Contains(source)) {
//			_fadingAudioSources.Remove(source);
//		}
		
		if (finalVolume == 0f && controller.fading) {
			if (source.isPlaying) {
				source.Stop();
			}
		}
		
		controller.fading = false;
	}
	
	IEnumerator ChangePitchOfLoopingClipCor(SoundType soundType, float fadeTime, float newPitch, float startDelay) {
		if (startDelay > 0f) {
			yield return new WaitForSeconds(startDelay);
		}
		if (_loopingSounds.ContainsKey(soundType) == false) {
			yield break;
		}
		AudioSource source = _loopingSounds[soundType];
		
		float time = 0f;
		float t = 0f;
		float startPitch = source.pitch;
		while (t < 1f && source.isPlaying) {
			time += Time.deltaTime;
			t = time / fadeTime;
			source.pitch = Mathf.Lerp(startPitch, 0f, t);
			yield return 1;
		}
	}
	
	#endregion
	#region public methods
	
	public void FadeLoopingSoundType(SoundType soundType, float fadeTime, float finalVolume = 0f, float delay = 0f) {
		if (_loopingSounds.ContainsKey(soundType) == false) { return; }
		AudioSource source = _loopingSounds[soundType];
		_loopingSounds.Remove(soundType);
		StartCoroutine(StopSoundClipCor(source, fadeTime, finalVolume, delay));
	}
	
	public void FadeAudioSource(AudioSource source, float fadeTime, float finalVolume = 0f, float delay = 0f) {
		StartCoroutine(StopSoundClipCor(source, fadeTime, finalVolume, delay));
	}
	
	public void ChangePitchOfLoopingClip(SoundType soundType, float fadeTime, float newPitch, float startDelay) {
		StartCoroutine(ChangePitchOfLoopingClipCor(soundType, fadeTime, newPitch, startDelay));
	}
	
	public AudioClip GetRandomSound(SoundType soundType) {
		AudioPlayerClipCollectionData soundData = _soundMap[soundType];
		int soundIndex = Random.Range(0, soundData._clips.Length);
		return soundData._clips[soundIndex];
	}
	
	public AudioClip GetFirstSound(SoundType soundType) {
		return _soundMap[soundType]._clips[0];
	}
	
	public void PlayFirstSound(float volume, SoundType soundType, float delay = 0f, System.Action<AudioSource> sourceCb = null, float pitch = 1f, float jumpToTimeInClip = 0f) {
		// get sound info
		//Debug.Log(soundType);
		AudioPlayerClipCollectionData soundData = _soundMap[soundType];
		volume *= soundData._volume;
		var clip = soundData._clips[0];
		
		if (soundData._music) {
			// play music sound
			StartCoroutine(PlaySoundClip(volume, clip, soundType, true, delay, sourceCb, true, pitch, jumpToTimeInClip));
		} else {
			// play normal sound
			StartCoroutine(PlaySoundClip(volume, clip, soundType, soundData._looping, delay, sourceCb, false, pitch, jumpToTimeInClip));
		}
	}
	
	public void PlayRandomSound(float volume, SoundType soundType, float delay = 0f) {
		// get sound info
		AudioPlayerClipCollectionData soundData = _soundMap[soundType];
		volume *= soundData._volume;
		var clip = GetRandomSound(soundType);
		
		if (soundData._music) {
			// play music sound
			StartCoroutine(PlaySoundClip(volume, clip, soundType, true, delay, null, true, 1f, 0f));
		} else {
			// play normal sound
			StartCoroutine(PlaySoundClip(volume, clip, soundType, soundData._looping, delay, null, false, 1f, 0f));
		}
	}
	
	public void SetGroupEnabled(SoundGroup soundGroup, bool status) {
		// add key if it doesnt exist
		if (_groupEnabled.ContainsKey(soundGroup) == false) {
			_groupEnabled.Add(soundGroup, status);
		}

		if (_groupEnabled[soundGroup] == status) {
			return;
		}
		
		_groupEnabled[soundGroup] = status;
		
		// find all the sound types on this group and if they're music, stop or play them
		foreach (var soundData in _soundMap.Values) {
			if (soundData._group == soundGroup && soundData._music == true) {
				var audioSource = _sources[soundGroup].Peek(); // _sources[soundGroup].Dequeue();
				if (status) {
					audioSource.Play();
					_musicAudioSource = audioSource;
				} else {
					if (audioSource.isPlaying) {
						audioSource.Stop();
						_musicAudioSource = null;
					}
				}
				//_sources[soundGroup].Enqueue(audioSource);
			}
		}
	}
	
	public void SetGroupVolume(SoundGroup soundGroup, float volume) {
		// add key if doesnt exist
		if (_groupVolume.ContainsKey(soundGroup) == false) {
			_groupVolume.Add(soundGroup, volume);
		}
		
		if (_groupVolume[soundGroup] == volume) {
			return;
		}
		
		_groupVolume[soundGroup] = volume;
		
		// find all the sound types on this group and if they're music, change volume
		foreach (var soundData in _soundMap.Values) {
			if (soundData._group == soundGroup && soundData._music == true) {
				var audioSource = _sources[soundGroup].Dequeue();
				audioSource.volume = volume;
				_sources[soundGroup].Enqueue(audioSource);
			}
		}
	}
	
	public bool GetGroupEnabled(SoundGroup soundGroup) {
		return _groupEnabled[soundGroup];
	}
	
	public float GetGroupVolume(SoundGroup soundGroup) {
		return _groupVolume[soundGroup];
	}
	
	public void SetSoundStatus(float volume, bool status, bool isMusic) {
		HashSet<SoundGroup> seenGroups = new HashSet<SoundGroup>();
		foreach (var soundData in _soundMap.Values) {
			if (soundData._music == isMusic && seenGroups.Contains(soundData._group) == false) {
				SetGroupVolume(soundData._group, volume);
				SetGroupEnabled(soundData._group, status);
				seenGroups.Add(soundData._group);
			}
		}
	}
	
	#endregion
	
} 
