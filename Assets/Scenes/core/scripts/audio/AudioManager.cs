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
		/// <inheritdoc />
		private class AudioReference : IAudioReference
		{
			private EventInstance mEvent;

			/// <inheritdoc />
			public AudioReference(EventInstance e)
			{
				mEvent = e;
			}

			/// <inheritdoc />
			public void Start()
			{
				mEvent.start();
			}

			/// <inheritdoc />
			public void Kill(bool allowFade = true)
			{
				mEvent.stop(allowFade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
				mEvent.release();
			}

			/// <inheritdoc />
			public void SetVolume(float vol)
			{
				mEvent.setVolume(vol);
			}

			/// <inheritdoc />
			public void AttachToRigidbody(Rigidbody rb)
			{
				FMODUnity.RuntimeManager.AttachInstanceToGameObject(mEvent, rb.transform, rb);
			}

			/// <inheritdoc />
			public bool isPlaying
			{
				get
				{
					PLAYBACK_STATE state;
					mEvent.getPlaybackState(out state);
					return state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.SUSTAINING;
				}
			}

			public bool isAlive
			{
				get { return mEvent.isValid() && mEvent.hasHandle() && isPlaying; }
			}

			/// <inheritdoc />
			public float playerSpeed { get { return GetParameter("PlayerSpeed"); } set { SetParameter("PlayerSpeed", value); } }

			/// <inheritdoc />
			public float weaponType { get { return GetParameter("WeaponType"); } set { SetParameter("WeaponType", value); } }

			/// <inheritdoc />
			public void SetParameter(string name, float value)
			{
				RESULT result = mEvent.setParameterValue(name, value);
				if (result != RESULT.OK)
				{
					throw new ArgumentException(
						string.Format("Could not set parameter: {0} value {1:##.000}. Result was: {2}", name, value, result.ToString()));
				}
			}

			/// <inheritdoc />
			public float GetParameter(string name)
			{
				ParameterInstance instance;
				RESULT result = mEvent.getParameter(name, out instance);
				if (result != RESULT.OK)
					throw new ArgumentException(string.Format("Could not get parameter: {0}. Result was: {1}", name, result.ToString()));

				float val;
				result = instance.getValue(out val);
				if (result != RESULT.OK)
					throw new ArgumentException(string.Format("Could not get parameter: {0}. Result was: {1}", name, result.ToString()));

				return val;
			}
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
