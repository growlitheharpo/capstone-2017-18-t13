using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Data;
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
		private float mCurrentCrowdLevel;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			mCurrentCrowdLevel = mValues.minHypeValue;
			mCrowdSound = ServiceLocator.Get<IAudioManager>()
				.CreateSound(AudioEvent.AmbientCrowd, null);

			mCrowdSound.crowdHypeLevel = mCurrentCrowdLevel;
		}

		/// <summary>
		/// Unity's OnDestroy function
		/// </summary>
		private void OnDestroy()
		{
			
		}
	}
}
