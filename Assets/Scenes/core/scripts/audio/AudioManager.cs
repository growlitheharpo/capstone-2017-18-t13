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
		FlyingShips = 77,
		AmbientCrowd = 78,
		Grinders = 79,
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
		AnnouncerInterrupt = 170,
		CameraGreeting = 225,
		CameraPain = 230,
		CameraPlayerDeath = 235,
		CameraIntro = 240,

		// UI
		MenuButtonHover = 180,
		MenuButtonPress = 185,
		HypeText = 190,
		CountdownTimer = 195,

		// Music
		MenuMusic = 200,
		IntroMusic = 205,
	}

	/// <inheritdoc cref="IAudioManager"/>
	public partial class AudioManager : MonoSingleton<AudioManager>, IAudioManager
	{
		private enum AnnouncerPriority
		{
			Drop,
			Queue,
			Interrupt,
		}

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

		/// Inspector variables
		[SerializeField] private bool mShouldSelfInitialize;
		[HideInInspector] [SerializeField] private List<EnumFmodBind> mEventBindList;

		/// Private variables
		private Dictionary<AudioEvent, string> mEventDictionary;
		private Dictionary<AudioEvent, AnnouncerPriority> mAnnouncerPriority;
		private Queue<AudioEvent> mAnnouncerLineQueue;

		private IAudioReference mCurrentAnnouncerEvent;

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			mAnnouncerPriority = new Dictionary<AudioEvent, AnnouncerPriority>
			{
				{ AudioEvent.AnnouncerMatchStarts, AnnouncerPriority.Interrupt},
				{ AudioEvent.AnnouncerMatchEnds, AnnouncerPriority.Interrupt },
				{ AudioEvent.AnnouncerStageAreaSpawns, AnnouncerPriority.Queue },
				{ AudioEvent.AnnouncerStageAreaCaptured, AnnouncerPriority.Queue },
				{ AudioEvent.AnnouncerGetsLegendary, AnnouncerPriority.Drop },
				{ AudioEvent.AnnouncerGetsKillstreak, AnnouncerPriority.Queue },
				{ AudioEvent.AnnouncerGetsDeathstreak, AnnouncerPriority.Drop },
				{ AudioEvent.AnnouncerKingslayer, AnnouncerPriority.Queue },
				{ AudioEvent.AnnouncerEnvironmentKill, AnnouncerPriority.Drop },
				{ AudioEvent.AnnouncerHeadshot, AnnouncerPriority.Drop },
				{ AudioEvent.AnnouncerLull, AnnouncerPriority.Drop },
				{ AudioEvent.AnnouncerNewLeader, AnnouncerPriority.Queue },
				{ AudioEvent.AnnouncerSponsor, AnnouncerPriority.Drop },
				{ AudioEvent.AnnouncerTimeWarning, AnnouncerPriority.Interrupt },
				{ AudioEvent.AnnouncerPlayerDeath, AnnouncerPriority.Drop },
				{ AudioEvent.AnnouncerBreaksCamera, AnnouncerPriority.Drop },
				{ AudioEvent.AnnouncerMultiKill, AnnouncerPriority.Queue }
			};

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

		/// <summary>
		/// Unity's update function
		/// </summary>
		private void Update()
		{
			if (mAnnouncerLineQueue.Count > 0)
			{
				if (CheckReferenceAlive(ref mCurrentAnnouncerEvent) != null)
					return;
			}
		}

		/// <summary>
		/// Plays an Announcer line through the special announcer system.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public IAudioReference PlayAnnouncerLine(AudioEvent e)
		{
			throw new NotImplementedException();
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
