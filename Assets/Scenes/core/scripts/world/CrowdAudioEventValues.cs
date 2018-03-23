using System;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// Utility data struct for holding variables related to the crowd audio.
	/// </summary>
	[Serializable]
	public struct CrowdAudioEventValues
	{
		[SerializeField] private float mMinHypeValue;
		[SerializeField] private float mMaxHypeValue;
		[SerializeField] private float mStandardKillGain;
		[SerializeField] private float mEnvironmentalKillGain;
		[SerializeField] private float mStageCaptureGain;
		[SerializeField] private float mLegendaryPartGain;

		[SerializeField] private float mInitialDecreaseTimerLength;
		[SerializeField] private float mSubsequentDecreaseTimerLength;

		/// <summary>
		/// Lowest value that the hype can be set to.
		/// </summary>
		public float minHypeValue { get { return mMinHypeValue; } }

		/// <summary>
		/// Highest value that the hype can be set to.
		/// </summary>
		public float maxHypeValue { get { return mMaxHypeValue; } }

		/// <summary>
		/// Value to gain on a kill.
		/// </summary>
		public float standardKillGain { get { return mStandardKillGain; } }

		/// <summary>
		/// Value to gain when a stage is captured.
		/// </summary>
		public float stageCaptureGain { get { return mStageCaptureGain; } }

		/// <summary>
		/// Value to gain when a legendary part is equipped.
		/// </summary>
		public float legendaryPartGain { get { return mLegendaryPartGain; } }

		/// <summary>
		/// Value to gain when an environmental kill occurs.
		/// </summary>
		public float environmentalKillGain { get { return mEnvironmentalKillGain; } }

		/// <summary>
		/// Length of time to wait before decreasing hype after an event occurs.
		/// </summary>
		public float initialDecreaseTimerLength { get { return mInitialDecreaseTimerLength; } }

		/// <summary>
		/// Length of time to wait before decreasing hype after an event occurs and the first decrease occurs.
		/// </summary>
		public float subsequentDecreaseTimerLength { get { return mSubsequentDecreaseTimerLength; } }
	}
}
