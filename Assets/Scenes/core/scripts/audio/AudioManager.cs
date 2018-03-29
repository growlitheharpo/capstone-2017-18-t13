using System;
using System.Collections.Generic;
using FiringSquad.Core.State;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Core.Audio
{
	/// <summary>
	/// The enum for the audio events. Mapped 1-1 to our FMOD events.
	/// </summary>
	public enum AudioEvent
	{
		// Player
		EquipItem = 50,
		LoopWalking = 30,
		Jump = 35,
		Land = 38,
		LoopGravGun = 40,
		MagnetArmGrab = 45,
		LocalDealDamage = 48,
		PlayerHealthPickup = 53,
		GetKill = 55,

		// Weapons
		Reload = 15,
		Shoot = 20,
		BarrelLayer = 25,
		ImpactWall = 60,
		ImpactOtherPlayer = 65,
		ImpactCurrentPlayer = 70,
		EnterAimDownSights = 75,

		// VO
		AmbientCrowd = 78,
		AnnouncerMatchStarts = 80,
		AnnouncerMatchEnds = 85,
		AnnouncerStageAreaSpawns = 90,
		AnnouncerStageAreaCaptured = 95,
		PlayerDamagedGrunt = 100,
		AnnouncerGetsLegendary = 105,
		AnnouncerGetsKillstreak = 110,
		AnnouncerGetsDeathstreak = 115,
		AnnouncerKingslayer = 120,
		AnnouncerEnvironmentKill = 125,
		AnnouncerHeadshot = 130,
		AnnouncerLull = 135,
		AnnouncerNewLeader = 140,
		AnnouncerSponsor = 145,
		AnnouncerTimeWarning = 150,
		AnnouncerPlayerDeath = 155,
		AnnouncerBreaksCamera = 160,
		AnnouncerMultiKill = 165,

		// UI
		MenuButtonHover = 180,
		MenuButtonPress = 185,
		HypeText = 190,

		// Music
		MenuMusic = 200,
		IntroMusic = 205,
	}

	/// <inheritdoc cref="IAudioManager"/>
	public class AudioManager : MonoSingleton<AudioManager>, IAudioManager
	{
		/// <summary>
		/// Utility class to bind an enum audio event to an FMOD value.
		/// </summary>
		[Serializable]
		private struct EnumFmodBind
		{
			public AudioEvent mEnumVal;

			[FMODUnity.EventRef]
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
			public bool isPlaying
			{
				get
				{
					PLAYBACK_STATE state;
					mEvent.getPlaybackState(out state);
					return state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING || state == PLAYBACK_STATE.SUSTAINING;
				}
			}

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

			/// <summary>
			/// True if this reference is valid and currently playing.
			/// </summary>
			public bool isAlive
			{
				get { return mEvent.isValid() && mEvent.hasHandle() && isPlaying; }
			}

			/// <inheritdoc />
			public float playerSpeed { get { return GetParameter("PlayerSpeed"); } set { SetParameter("PlayerSpeed", value); } }

			/// <inheritdoc />
			public float weaponType { get { return GetParameter("WeaponType"); } set { SetParameter("WeaponType", value); } }

			/// <inheritdoc />
			public float barrelType { get { return GetParameter("BarrelType"); } set { SetParameter("BarrelType", value); } }

			/// <inheritdoc />
			public float isCurrentPlayer { get { return GetParameter("IsCurrentPlayer"); } set { SetParameter("IsCurrentPlayer", value); } }

			/// <inheritdoc />
			public float healthGained { get { return GetParameter("HealthGained"); } set { SetParameter("HealthGained", value); } }

			/// <inheritdoc />
			public float crowdHypeLevel { get { return GetParameter("CrowdHypeLevel"); } set { SetParameter("CrowdHypeLevel", value); } }

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

		/// Inspector variables
		[SerializeField] private bool mShouldSelfInitialize;
		[HideInInspector] [SerializeField] private List<EnumFmodBind> mEventBindList;

		/// Private variables
		private Dictionary<AudioEvent, string> mEventDictionary;

		/// <summary>
		/// Unity's Start function.
		/// </summary>
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

			EventManager.Notify(EventManager.Local.InitialAudioLoadComplete);
		}

		/// <inheritdoc />
		public IAudioReference CreateSound(AudioEvent e, Transform location, bool autoPlay = true)
		{
			Logger.Info("Creating sound: " + e, Logger.System.Audio);

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
			EventInstance fmodEvent;
			try
			{
				fmodEvent = FMODUnity.RuntimeManager.CreateInstance(mEventDictionary[e]);
			}
			catch (FMODUnity.EventNotFoundException except)
			{
				UnityEngine.Debug.LogException(except);
				return null;
			}

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
