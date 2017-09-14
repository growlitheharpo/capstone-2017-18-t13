using UnityEngine;
using UnityEngine.Audio;

namespace FiringSquad.Data
{
	/// <inheritdoc cref="IAudioClip" />
	[CreateAssetMenu(menuName = "Audio/Audio Clip Data")]
	public class AudioClipData : ScriptableObject, IAudioClip
	{
		[SerializeField] private bool mBypassEffects;
		
		[SerializeField] private float mFadeInTime;
		[SerializeField] private float mFadeOutTime;
		[SerializeField] private AudioMixerGroup mGroup;
		[SerializeField] private bool mLooping;
		[SerializeField] private float mMaxDistance = 500.0f;
		[SerializeField] private float mMinDistance = 1.0f;
		[SerializeField] private float mPitch = 1.0f;
		[SerializeField] private bool mPlayAtSource = true;

		[SerializeField] private int mPriority = 128;

		[SerializeField] private AudioRolloffMode mRolloffMode = AudioRolloffMode.Logarithmic;
		[SerializeField] private AudioClip mSound;
		[SerializeField] private float mSpatialBlend = 1.0f;
		[SerializeField] private float mStereoPan;
		[SerializeField] private float mVolume = 1.0f;

		/// <inheritdoc />
		public bool bypassEffects { get { return mBypassEffects; } }

		/// <inheritdoc />
		public AudioMixerGroup group { get { return mGroup; } set { mGroup = value; } }

		/// <inheritdoc />
		public AudioClip sound { get { return mSound; } set { mSound = value; } }

		/// <inheritdoc />
		public bool looping { get { return mLooping; } }

		/// <inheritdoc />
		public int priority { get { return mPriority; } }

		/// <inheritdoc />
		public float volume { get { return mVolume; } }

		/// <inheritdoc />
		public float pitch { get { return mPitch; } }

		/// <inheritdoc />
		public float stereoPan { get { return mStereoPan; } }

		/// <inheritdoc />
		public float spatialBlend { get { return mSpatialBlend; } }

		/// <inheritdoc />
		public AudioRolloffMode rolloffMode { get { return mRolloffMode; } }

		/// <inheritdoc />
		public float minDistance { get { return mMinDistance; } }

		/// <inheritdoc />
		public float maxDistance { get { return mMaxDistance; } }

		/// <inheritdoc />
		public float fadeInTime { get { return mFadeInTime; } }

		/// <inheritdoc />
		public float fadeOutTime { get { return mFadeOutTime; } }

		/// <inheritdoc />
		public bool playAtSource { get { return mPlayAtSource; } }
	}
}
