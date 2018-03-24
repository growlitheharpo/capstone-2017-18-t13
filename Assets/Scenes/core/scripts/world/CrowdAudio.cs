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
		[SerializeField] private CrowdAudioEventValues mValues;

		private IAudioReference mCrowdSound;
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
		private void OnPlayerDied()
		{
			IncreaseHypeLevel(mValues.standardDeathGain);
		}
		
		/// <summary>
		/// EVENT HANDLER: LocalGeneric.OnPlayerEquippedLegendaryPart
		/// </summary>
		private void OnPlayerEquippedLegendaryPart()
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
			UnityEngine.Debug.Log(mCurrentCrowdLevel);
			mCrowdSound.crowdHypeLevel = mCurrentCrowdLevel;

			mTimer = mValues.initialDecreaseTimerLength;
		}
	}
}
