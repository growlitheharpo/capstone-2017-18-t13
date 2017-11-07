﻿using System;
using System.Collections.Generic;
using FiringSquad.Core.State;
using FMOD;
using FMOD.Studio;
using UnityEngine;

namespace FiringSquad.Core.Audio
{
	public enum AudioEvent
	{
		EquipItem = 50,
		LoopGravGun = 40,
		LoopWalking = 30,

		// Weapons
		Reload = 15,
		Shoot = 20,
		ImpactWall = 60,
		ImpactOtherPlayer = 65,
		ImpactCurrentPlayer = 70,
		EnterAimDownSights = 75,

		// VO
		AnnouncerMatchStarts = 80,
		AnnouncerMatchEnds = 85,
		AnnouncerStageAreaSpawns = 90,
		AnnouncerStageAreaCaptured = 95,
	}

	/// <inheritdoc cref="IAudioManager"/>
	public class AudioManager : MonoSingleton<AudioManager>, IAudioManager
	{
		[SerializeField] private bool mShouldSelfInitialize;

		[Serializable]
		private struct EnumFmodBind
		{
			public AudioEvent mEnumVal;
			public string mFmodVal;
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
			public IAudioReference Start()
			{
				mEvent.start();
				return this;
			}

			/// <inheritdoc />
			public IAudioReference Kill(bool allowFade = true)
			{
				mEvent.stop(allowFade ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
				mEvent.release();
				return this;
			}

			/// <inheritdoc />
			public IAudioReference SetVolume(float vol)
			{
				mEvent.setVolume(vol);
				return this;
			}

			/// <inheritdoc />
			public IAudioReference AttachToRigidbody(Rigidbody rb)
			{
				FMODUnity.RuntimeManager.AttachInstanceToGameObject(mEvent, rb.transform, rb);
				return this;
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
			public IAudioReference SetParameter(string name, float value)
			{
				RESULT result = mEvent.setParameterValue(name, value);
				if (result != RESULT.OK)
				{
					throw new ArgumentException(
						string.Format("Could not set parameter: {0} value {1:##.000}. Result was: {2}", name, value, result.ToString()));
				}

				return this;
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

		[SerializeField] private List<EnumFmodBind> mEventBindList;
		private Dictionary<AudioEvent, string> mEventDictionary;

		private void Start()
		{
			if (!ServiceLocator.Get<IGamestateManager>().isAlive && mShouldSelfInitialize)
				InitializeDatabase();
		}

		/// <inheritdoc />
		public void InitializeDatabase()
		{
			// TODO: We can wait async here
			FMODUnity.RuntimeManager.LoadBank("Weapons", true);
			FMODUnity.RuntimeManager.LoadBank("Player", true);
			FMODUnity.RuntimeManager.WaitForAllLoads();

			mEventDictionary = new Dictionary<AudioEvent, string>(mEventBindList.Count);
			foreach (EnumFmodBind e in mEventBindList)
				mEventDictionary.Add(e.mEnumVal, e.mFmodVal);

			EventManager.Notify(EventManager.InitialAudioLoadComplete);
		}

		/// <inheritdoc />
		public IAudioReference CreateSound(AudioEvent e, Transform location, bool autoPlay = true)
		{
			EventInstance fmodEvent = FMODUnity.RuntimeManager.CreateInstance(mEventDictionary[e]);
			AudioReference reference = new AudioReference(fmodEvent);

			if (location != null)
			{
				ATTRIBUTES_3D locationData = FMODUnity.RuntimeUtils.To3DAttributes(location);
				fmodEvent.set3DAttributes(locationData);
			}

			if (autoPlay)
				reference.Start();

			return reference;
		}

		/// <inheritdoc />
		public IAudioReference CreateSound(AudioEvent e, Transform location, Vector3 offset, Space offsetType = Space.Self, bool autoPlay = true)
		{
			EventInstance fmodEvent = FMODUnity.RuntimeManager.CreateInstance(mEventDictionary[e]);
			AudioReference reference = new AudioReference(fmodEvent);

			if (location != null)
			{
				ATTRIBUTES_3D locationData = FMODUnity.RuntimeUtils.To3DAttributes(location);

				Vector3 worldPos = offsetType == Space.World ? offset : location.TransformPoint(offset);
				VECTOR realPos = FMODUnity.RuntimeUtils.ToFMODVector(worldPos);
				locationData.position = realPos;

				fmodEvent.set3DAttributes(locationData);
			}

			if (autoPlay)
				reference.Start();

			return reference;
		}

		/// <inheritdoc />
		public IAudioReference CheckReferenceAlive(ref IAudioReference reference)
		{
			AudioReference realRef = reference as AudioReference;
			if (realRef != null && realRef.isAlive)
				return reference;

			reference = null;
			return null;
		}
	}
}
