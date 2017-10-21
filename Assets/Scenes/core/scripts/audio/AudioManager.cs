using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core.State;
using FiringSquad.Data;
using UnityEngine;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Core.Audio
{
	/// <inheritdoc cref="IAudioManager"/>
	public class AudioManager : MonoSingleton<AudioManager>, IAudioManager
	{
		public enum ProfileType
		{
			ChooseRandom,
			PlayAll
		}

		public enum AudioEvent
		{
			TestExplosionEffect = -1,
			MainBackgroundEffect = 0,
			PrimaryEffect1 = 1,
			PrimaryEffect2 = 2,
			PrimaryEffect3 = 3,
			DeathSound = 10,

			Reload = 15, //done
			Shoot = 20, //done
			StartWalking = 25, // not yet!
			LoopWalking = 30, // done
			StartGravGun = 35, // not yet!
			LoopGravGun = 40, // done
			InteractReceive = 50, // not yet!

			ImpactWall = 60, // done
			ImpactOtherPlayer = 65, // done
			ImpactCurrentPlayer = 70 // done
		}

		/// <summary>
		/// Private implementation of the IAudioReference interface.
		/// </summary>
		private class AudioReference : IAudioReference
		{
			/// <summary>
			/// The clip(s) played by this reference.
			/// </summary>
			public List<IAudioClip> mClipData;

			/// <summary>
			/// The actual GameObject audio sources created by this reference.
			/// </summary>
			public List<AudioSource> mSources;

			/// <inheritdoc />
			public void Kill()
			{
				instance.StopSoundImmediate(this);
			}

			/// <inheritdoc />
			public void FadeOut(float time)
			{
				throw new NotImplementedException("Fading audio in and out is not yet supported!");
			}

			/// <inheritdoc />
			public void SetRepeat(bool repeat)
			{
				foreach (AudioSource source in mSources)
					source.loop = repeat;
			}

			/// <inheritdoc />
			public void SetVolume(float vol)
			{
				for (int i = 0; i < mClipData.Count; i++)
					mSources[i].volume = mClipData[i].volume * vol;
			}

			/// <inheritdoc />
			public void SetPitch(float pitch)
			{
				for (int i = 0; i < mClipData.Count; i++)
					mSources[i].pitch = pitch;
			}

			public bool isPlaying { get { return mSources.Any(x => x.loop || !(x.time >= x.clip.length - Time.deltaTime)); } }
		}

		[SerializeField] private AudioDatabase mAudioDatabase;
		[SerializeField] private bool mShouldSelfInitialize;
		private HashSet<IAudioReference> mCurrentSounds;

		protected override void Awake()
		{
			base.Awake();
			mCurrentSounds = new HashSet<IAudioReference>();
		}

		private void Start()
		{
			if (!ServiceLocator.Get<IGamestateManager>().isAlive && mShouldSelfInitialize)
				InitializeDatabase();
		}

		/// <inheritdoc />
		public void InitializeDatabase()
		{
			mAudioDatabase.InitializePrefabs(transform);
			EventManager.Notify(EventManager.InitialAudioLoadComplete);
		}

		// Update is called once per frame
		private void Update()
		{
			ProcessFinishedAudio();
		}

		/// <summary>
		/// Loop through all of our currently active references and Destroy any that have finished.
		/// </summary>
		private void ProcessFinishedAudio()
		{
			var refsToDelete = new List<IAudioReference>();
			foreach (IAudioReference reference in mCurrentSounds)
			{
				AudioReference r = reference as AudioReference;
				if (r == null || r.mSources.Count == 0)
				{
					refsToDelete.Add(reference);
					continue;
				}

				for (int i = 0; i < r.mSources.Count; i++)
				{
					if (r.mSources[i] == null)
					{
						r.mSources.RemoveAt(i);
						r.mClipData.RemoveAt(i);
						i--;
						continue;
					}

					if (r.mSources[i].loop || !(r.mSources[i].time >= r.mSources[i].clip.length - Time.deltaTime))
						continue;

					Destroy(r.mSources[i].gameObject);
					r.mSources.RemoveAt(i);
					r.mClipData.RemoveAt(i);
					i--;
				}
			}

			foreach (IAudioReference r in refsToDelete)
				mCurrentSounds.Remove(r);
		}

		/// <inheritdoc />
		public IAudioReference PlaySound(AudioEvent e, IAudioProfile profile, Transform location)
		{
			return PlaySound(e, profile, location, Vector3.zero);
		}

		/// <inheritdoc />
		public IAudioReference PlaySound(AudioEvent e, IAudioProfile profile, Transform location, Vector3 offset)
		{
			if (profile == null)
			{
				Logger.Warn("Trying to play a null profile: " + e, Logger.System.Audio);
				return null;
			}

			var clips = profile.GetClipInParents(e);
			var sources = new List<AudioSource>();

			foreach (IAudioClip clip in clips)
			{
				GameObject newGameObject = Instantiate(mAudioDatabase.GetPrefab(profile, clip));
				newGameObject.name += "__" + e;
				if (clip.playAtSource)
				{
					newGameObject.transform.SetParent(location);
					newGameObject.transform.localPosition = offset;
				}

				AudioSource s = newGameObject.GetComponent<AudioSource>();
				sources.Add(s);

				s.Play();
			}

			//TODO: Remove this!
			if (clips.Any(x => x.fadeInTime > 0.0f || x.fadeOutTime > 0.0f))
				Logger.Warn("Fading audio in and out is not supported yet!", Logger.System.Audio);

			AudioReference newRef = new AudioReference { mClipData = clips.ToList(), mSources = sources };
			mCurrentSounds.Add(newRef);
			return newRef;
		}

		/// <inheritdoc />
		public IAudioReference CheckReferenceAlive(ref IAudioReference reference)
		{
			if (reference == null || mCurrentSounds.Contains(reference))
				return reference;

			reference = null;
			return null;
		}

		private void StopSoundImmediate(IAudioReference sound)
		{
			if (!mCurrentSounds.Contains(sound))
				return;

			AudioReference soundImpl = sound as AudioReference;
			if (soundImpl == null)
				return;

			foreach (AudioSource source in soundImpl.mSources)
			{
				source.Stop();
				Destroy(source.gameObject);
			}

			soundImpl.mSources.Clear();
			soundImpl.mClipData.Clear();
		}
	}
}
