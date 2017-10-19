using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core.Audio;
using KeatsLib.Collections;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <inheritdoc cref="IAudioProfile"/>
	[CreateAssetMenu(fileName = "AudioProfile", menuName = "Audio/Profile")]
	public class AudioProfile : ScriptableObject, IAudioProfile
	{
		/// <summary>
		/// Utility struct to bind an event to a list of clips.
		/// </summary>
		[Serializable]
		public struct EventToClipList
		{
			public AudioManager.AudioEvent mEvent;
			public AudioClipData[] mClips;
		}

		public AudioProfile mParent;

		[SerializeField] private string mId;
		public string id { get { return mId; } set { mId = value; } }

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

		/// <inheritdoc />
		public IAudioClip[] GetAllClips()
		{
			return mClips.Values.SelectMany(array => array.Select(clip => clip as IAudioClip)).ToArray();
		}

		/// <inheritdoc />
		public IAudioClip[] GetClip(AudioManager.AudioEvent e)
		{
			AudioClipData[] clips;
			return mClips.TryGetValue(e, out clips) ? ChooseClip(clips) : null;
		}

		/// <inheritdoc />
		public IAudioClip[] GetClipInParents(AudioManager.AudioEvent e)
		{
			AudioClipData[] clips;
			if (mClips.TryGetValue(e, out clips))
				return ChooseClip(clips);

			return mParent != null ? mParent.GetClipInParents(e) : new IAudioClip[] { };
		}

		/// <summary>
		/// Return the appropriate number of clips based on our profile type (ChooseRandom or PlayAll).
		/// </summary>
		/// <param name="clips">The clip array to choose from.</param>
		private IAudioClip[] ChooseClip(IEnumerable<AudioClipData> clips)
		{
			if (mProfile == AudioManager.ProfileType.ChooseRandom)
				return new IAudioClip[] { clips.ChooseRandom() };

			return clips.Select(x => x as IAudioClip).ToArray();
		}
	}
}
