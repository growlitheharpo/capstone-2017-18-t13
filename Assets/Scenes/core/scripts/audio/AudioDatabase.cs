using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioDatabase : ScriptableObject
{
	[SerializeField] private List<AudioProfile> mProfiles;
	[SerializeField] private Dictionary<IAudioProfile, Dictionary<IAudioClip, GameObject>> mPrefabs;

	public void InitializePrefabs(Transform holder)
	{
		if (mPrefabs != null)
			DestroyPrefabs();

		mPrefabs = new Dictionary<IAudioProfile, Dictionary<IAudioClip, GameObject>>();

		foreach (IAudioProfile profile in mProfiles)
		{
			mPrefabs[profile] = new Dictionary<IAudioClip, GameObject>();
			var clips = profile.GetAllClips();
			foreach (IAudioClip clip in clips)
			{
				if (clip == null)
					continue;

				if (clip.sound == null)
					throw new NullReferenceException("Audio Clip in profile " + profile.id + " is missing its reference to a sound file.");

				GameObject newGo = new GameObject(profile.id + "__" + clip.sound.name);
				newGo.transform.SetParent(holder);
				newGo.transform.localPosition = Vector3.zero;

				AudioSource source = newGo.AddComponent<AudioSource>();
				source.playOnAwake = false;
				source.outputAudioMixerGroup = clip.group;
				source.clip = clip.sound;
				source.bypassEffects = clip.bypassEffects;
				source.loop = clip.looping;
				source.priority = clip.priority;
				source.volume = clip.volume;
				source.pitch = clip.pitch;
				source.panStereo = clip.stereoPan;
				source.spatialBlend = clip.spatialBlend;
				source.rolloffMode = clip.rolloffMode;
				source.minDistance = clip.minDistance;
				source.maxDistance = clip.maxDistance;

				mPrefabs[profile][clip] = newGo;
			}
		}
	}

	public GameObject GetPrefab(IAudioProfile p, IAudioClip c)
	{
		if (mPrefabs.ContainsKey(p) && mPrefabs[p].ContainsKey(c))
			return mPrefabs[p][c];
		return null;
	}

	private void DestroyPrefabs()
	{
		foreach (var profilePair in mPrefabs)
		{
			foreach (var clipPair in profilePair.Value)
				Destroy(clipPair.Value);
		}

		mPrefabs = null;
	}
}
