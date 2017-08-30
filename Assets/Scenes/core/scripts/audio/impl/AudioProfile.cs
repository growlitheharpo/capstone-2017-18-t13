using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Data;
using KeatsLib.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioProfile", menuName = "Audio/Profile")]
public class AudioProfile : ScriptableObject, IAudioProfile
{
	[Serializable]
	public struct EventToClipList
	{
		public AudioManager.AudioEvent mEvent;
		public AudioClipData[] mClips;
	}

	public AudioProfile mParent;

	public string mId;
	public string id { get { return mId; } }

	[SerializeField] private AudioManager.ProfileType mProfile;
	public AudioManager.ProfileType profile { get { return mProfile; } }

	[SerializeField] private List<EventToClipList> mClipsArray;
	[SerializeField] private Dictionary<AudioManager.AudioEvent, AudioClipData[]> mClips;

	/// <summary>
	/// Used to synchronize the two 
	/// </summary>
	private void OnEnable()
	{
		SynchronizeCollections();
	}

	/// <summary>
	/// This function is exposed to the Inspector, but NOT to game code.
	/// </summary>
#if UNITY_EDITOR
	public void SynchronizeCollections()
#else
	private void SynchronizeCollections()
#endif
	{
		mClips = new Dictionary<AudioManager.AudioEvent, AudioClipData[]>();
		foreach (EventToClipList st in mClipsArray)
			mClips[st.mEvent] = st.mClips;
	}

	public IAudioClip[] GetAllClips()
	{
		return mClips.Values.SelectMany(array => array.Select(clip => clip as IAudioClip)).ToArray();
	}

	public IAudioClip[] GetClip(AudioManager.AudioEvent e)
	{
		AudioClipData[] clips;
		return mClips.TryGetValue(e, out clips) ? ChooseClip(clips) : null;
	}

	public IAudioClip[] GetClipInParents(AudioManager.AudioEvent e)
	{
		AudioClipData[] clips;
		if (mClips.TryGetValue(e, out clips))
			return ChooseClip(clips);

		return mParent != null ? mParent.GetClipInParents(e) : new IAudioClip[] { };
	}
	
	private IAudioClip[] ChooseClip(IEnumerable<AudioClipData> clips)
	{
		if (mProfile == AudioManager.ProfileType.ChooseRandom)
			return new IAudioClip[] { clips.ChooseRandom() };

		return clips.Select(x => x as IAudioClip).ToArray();
	}
}
