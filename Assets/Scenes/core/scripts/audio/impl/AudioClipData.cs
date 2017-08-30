using UnityEngine;
using UnityEngine.Audio;

namespace FiringSquad.Data
{
	public class AudioClipData : ScriptableObject, IAudioClip
	{
		[SerializeField] private AudioClip mSound;
		[SerializeField] private AudioMixerGroup mGroup;

		[SerializeField] private float mFadeInTime;
		[SerializeField] private float mFadeOutTime;
		[SerializeField] private bool mPlayAtSource = true;

		[SerializeField] private bool mBypassEffects;
		[SerializeField] private bool mLooping;

		[SerializeField] private int mPriority = 128;
		[SerializeField] private float mVolume = 1.0f;
		[SerializeField] private float mPitch = 1.0f;
		[SerializeField] private float mStereoPan;
		[SerializeField] private float mSpatialBlend = 1.0f;

		[SerializeField] private AudioRolloffMode mRolloffMode = AudioRolloffMode.Logarithmic;
		[SerializeField] private float mMinDistance = 1.0f;
		[SerializeField] private float mMaxDistance = 500.0f;

		public bool bypassEffects { get { return mBypassEffects; } }
		public AudioMixerGroup group { get { return mGroup; } set { mGroup = value; } }
		public AudioClip sound { get { return mSound; } set { mSound = value; } }
		public bool looping { get { return mLooping; } }
		public int priority { get { return mPriority; } }
		public float volume { get { return mVolume; } }
		public float pitch { get { return mPitch; } }
		public float stereoPan { get { return mStereoPan; } }
		public float spatialBlend { get { return mSpatialBlend; } }
		public AudioRolloffMode rolloffMode { get { return mRolloffMode; } }
		public float minDistance { get { return mMinDistance; } }
		public float maxDistance { get { return mMaxDistance; } }
		public float fadeInTime { get { return mFadeInTime; } }
		public float fadeOutTime { get { return mFadeOutTime; } }
		public bool playAtSource { get { return mPlayAtSource; } }
	}
}
