using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// This semi-manager handles playing audio feedback for the player through
	/// the form of the crowd noises.
	/// </summary>
	public class CrowdAudio : MonoBehaviour
	{
		[SerializeField] private GameObject mCatcher;
		[SerializeField] private GameObject mFlyingShip01;
		[SerializeField] private GameObject mFlyingShip02;
		[SerializeField] private GameObject mFlyingShip03;
		[SerializeField] private CrowdAudioEventValues mValues; 

		private IAudioReference mCrowdSound;
		private IAudioReference mGrinderAudio;
		private IAudioReference mShipSound01;
		private IAudioReference mShipSound02;
		private IAudioReference mShipSound03;
		private int mCurrentCrowdLevel;
		private float mTimer;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			StartCoroutine(Coroutines.InvokeAfterFrames(5, InitializeSound));

			EventManager.LocalGeneric.OnPlayerCapturedStage += OnPlayerCapturedStage;
			EventManager.LocalGeneric.OnPlayerDied += OnPlayerDied;
			EventManager.LocalGeneric.OnPlayerEquippedLegendaryPart += OnPlayerEquippedLegendaryPart;

			IAudioManager audioService = ServiceLocator.Get<IAudioManager>();
			mGrinderAudio = audioService.CheckReferenceAlive(ref mGrinderAudio);
			if (mGrinderAudio == null)
			{
				mGrinderAudio = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.Grinders, mCatcher.gameObject.transform, true);
				mGrinderAudio.Start();
			}

			mShipSound01 = audioService.CheckReferenceAlive(ref mShipSound01);
			if (mShipSound01 == null)
			{
				mShipSound01 = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.FlyingShips, mFlyingShip01.gameObject.transform, true);
				mShipSound01.AttachToRigidbody(mFlyingShip01.GetComponent<Rigidbody>());
				mShipSound01.Start();
			}
			mShipSound02 = audioService.CheckReferenceAlive(ref mShipSound02);
			if (mShipSound02 == null)
			{
				mShipSound02 = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.FlyingShips, mFlyingShip02.gameObject.transform, true);
				mShipSound02.AttachToRigidbody(mFlyingShip02.GetComponent<Rigidbody>());
				mShipSound02.Start();
			}
			mShipSound03 = audioService.CheckReferenceAlive(ref mShipSound03);
			if (mShipSound03 == null)
			{
				mShipSound03 = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.FlyingShips, mFlyingShip03.gameObject.transform, true);
				mShipSound03.AttachToRigidbody(mFlyingShip03.GetComponent<Rigidbody>());
				mShipSound03.Start();
			}
		}

		private void InitializeSound()
		{
			mCurrentCrowdLevel = mValues.minHypeValue;
			mCrowdSound = ServiceLocator.Get<IAudioManager>()
				.CreateSound(AudioEvent.AmbientCrowd, null);

			mCrowdSound.crowdHypeLevel = mCurrentCrowdLevel;
			mTimer = 0.0f;
		}

		/// <summary>
		/// Unity's OnDestroy function
		/// </summary>
		private void OnDestroy()
		{
			IAudioManager service = ServiceLocator.Get<IAudioManager>();
			if (service == null)
				return;

			if (service.CheckReferenceAlive(ref mCrowdSound) != null)
				mCrowdSound.Kill();

			EventManager.LocalGeneric.OnPlayerCapturedStage -= OnPlayerCapturedStage;
			EventManager.LocalGeneric.OnPlayerDied -= OnPlayerDied;
			EventManager.LocalGeneric.OnPlayerEquippedLegendaryPart -= OnPlayerEquippedLegendaryPart;
		}

		/// <summary>
		/// Unity's Update function
		/// </summary>
		private void Update()
		{
			mTimer -= Time.deltaTime;
			if (mTimer > 0.0f)
				return;

			// If the timer is up, try to decrement the hype level. If it changed, push it to FMOD
			// NOTE: The reason for the complexity is that sending data to FMOD is more expensive than the check.
			int newValue = Mathf.Clamp(mCurrentCrowdLevel - 1, mValues.minHypeValue, mValues.maxHypeValue);
			if (newValue != mCurrentCrowdLevel)
			{
				mCurrentCrowdLevel = newValue;
				mCrowdSound.crowdHypeLevel = mCurrentCrowdLevel;
			}

			mTimer = mValues.subsequentDecreaseTimerLength;
		}

		/// <summary>
		/// EVENT HANDLER: LocalGeneric.OnPlayerCapturedStage
		/// </summary>
		private void OnPlayerCapturedStage()
		{
			IncreaseHypeLevel(mValues.stageCaptureGain);
		}
		
		/// <summary>
		/// EVENT HANDLER: LocalGeneric.OnPlayerDied
		/// </summary>
		private void OnPlayerDied(CltPlayer player)
		{
			IncreaseHypeLevel(mValues.standardDeathGain);
		}
		
		/// <summary>
		/// EVENT HANDLER: LocalGeneric.OnPlayerEquippedLegendaryPart
		/// </summary>
		private void OnPlayerEquippedLegendaryPart(CltPlayer player)
		{
			IncreaseHypeLevel(mValues.legendaryPartGain);
		}

		/// <summary>
		/// Immediately reset any decreasing timers and increase the hype level.
		/// </summary>
		/// <param name="amount"></param>
		private void IncreaseHypeLevel(int amount)
		{
			mCurrentCrowdLevel = Mathf.Clamp(mCurrentCrowdLevel + amount, mValues.minHypeValue, mValues.maxHypeValue);
			mCrowdSound.crowdHypeLevel = mCurrentCrowdLevel;

			mTimer = mValues.initialDecreaseTimerLength;
		}
	}
}
