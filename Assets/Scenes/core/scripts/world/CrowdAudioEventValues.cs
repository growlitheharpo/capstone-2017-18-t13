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
		[SerializeField] private int mMinHypeValue;
		[SerializeField] private int mMaxHypeValue;
		[SerializeField] private int mStandardDeathGain;
		[SerializeField] private int mStageCaptureGain;
		[SerializeField] private int mLegendaryPartGain;

		[SerializeField] private float mInitialDecreaseTimerLength;
		[SerializeField] private float mSubsequentDecreaseTimerLength;

		/// <summary>
		/// Lowest value that the hype can be set to.
		/// </summary>
		public int minHypeValue { get { return mMinHypeValue; } }

		/// <summary>
		/// Highest value that the hype can be set to.
		/// </summary>
		public int maxHypeValue { get { return mMaxHypeValue; } }

		/// <summary>
		/// Value to gain on any player's death, no matter the method.
		/// </summary>
		public int standardDeathGain { get { return mStandardDeathGain; } }

		/// <summary>
		/// Value to gain when a stage is captured.
		/// </summary>
		public int stageCaptureGain { get { return mStageCaptureGain; } }

		/// <summary>
		/// Value to gain when a legendary part is equipped.
		/// </summary>
		public int legendaryPartGain { get { return mLegendaryPartGain; } }

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
