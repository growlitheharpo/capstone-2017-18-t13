using System;
using JetBrains.Annotations;
using KeatsLib.Unity;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to handle the overall UI for the stage capture points.
	/// </summary>
	public class StageCaptureUI : MonoBehaviour
	{
		/// <summary>
		/// States for the stage UI to be in.
		/// </summary>
		public enum Mode
		{
			NoPoints,
			NoCapturing,
			PlayerCapturing,
			PlayerTeamCapturing,
			OtherCapturing,
			PointCaptured,
		}

		/// Inspector variables
		[SerializeField] private UIText mStatusLine;
		[SerializeField] private UIText mTimerLine;
		[SerializeField] private StageCapturePointer mPointer;
		[SerializeField] private UIFillBarScript mPlayerTimerBar;
		[SerializeField] private UIFillBarScript mEnemyTimerBar;

		private Mode mCurrentMode;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			SetMode(Mode.NoPoints, null);
		}

		/// <summary>
		/// Update the state of the stage capture UI and bind to an area.
		/// </summary>
		/// <param name="m">Which state to enter.</param>
		/// <param name="area">The stage to bind to, if applicable.</param>
		public void SetMode(Mode m, [CanBeNull] StageCaptureArea area)
		{
			mCurrentMode = m;

			switch (m)
			{
				case Mode.NoPoints:
					// No points means to hide all elements.
					mStatusLine.text = "";

					mTimerLine.gameObject.SetActive(false);

					mPlayerTimerBar.gameObject.SetActive(false);
					mEnemyTimerBar.gameObject.SetActive(false);

					mPointer.StopPointing();
					break;

				case Mode.NoCapturing:
					// A stage is available, but no one is capturing it.
					mStatusLine.text = "A Stage is Available for Capture!";

					mTimerLine.gameObject.SetActive(true);

					mPlayerTimerBar.gameObject.SetActive(false);
					mPlayerTimerBar.SetFillAmount(0.0f, true);
					mEnemyTimerBar.gameObject.SetActive(false);
					mEnemyTimerBar.SetFillAmount(0.0f, true);

					mPointer.EnableAndPoint(area);
					break;

				case Mode.PlayerCapturing:
					// A stage is available, and we're the ones capping it.
					mStatusLine.text = "Capturing Stage...";

					mTimerLine.gameObject.SetActive(false);

					mPlayerTimerBar.gameObject.SetActive(true);
					mEnemyTimerBar.gameObject.SetActive(false);
					mEnemyTimerBar.SetFillAmount(0.0f, true);

					mPointer.StopPointing();
					break;

				case Mode.PlayerTeamCapturing:
					// A stage is available, and we're the ones capping it.
					mStatusLine.text = "Your Team Is Capturing The Stage...";

					mTimerLine.gameObject.SetActive(false);

					mPlayerTimerBar.gameObject.SetActive(true);
					mEnemyTimerBar.gameObject.SetActive(false);
					mEnemyTimerBar.SetFillAmount(0.0f, true);

					mPointer.StopPointing();
					break;

				case Mode.OtherCapturing:
					// A stage is available, and someone else is capping.
					mStatusLine.text = "The Stage is Being Captured By Someone Else!";

					mTimerLine.gameObject.SetActive(false);

					mPlayerTimerBar.gameObject.SetActive(true);
					mPlayerTimerBar.SetFillAmount(0.0f, true);
					mEnemyTimerBar.gameObject.SetActive(true);

					mPointer.EnableAndPoint(area);
					break;

				case Mode.PointCaptured:
					// A stage was just captured.
					mStatusLine.text = "";
					mTimerLine.gameObject.SetActive(false);

					mPlayerTimerBar.gameObject.SetActive(false);
					mPlayerTimerBar.SetFillAmount(0.0f, true);
					mEnemyTimerBar.gameObject.SetActive(false);
					mEnemyTimerBar.SetFillAmount(0.0f, true);

					mPointer.StopPointing();
					break;

				default:
					throw new ArgumentOutOfRangeException("m", m, null);
			}
		}

		/// <summary>
		/// Set the percentage of the fill bar for the capture percent.
		/// </summary>
		public void SetCapturePercent(float p)
		{
			if (mCurrentMode == Mode.OtherCapturing)
				mEnemyTimerBar.SetFillAmount(p, !mEnemyTimerBar.gameObject.activeInHierarchy);
			else
				mPlayerTimerBar.SetFillAmount(p, !mPlayerTimerBar.gameObject.activeInHierarchy);
		}

		/// <summary>
		/// Set the remaining time before this stage expires and disappears.
		/// </summary>
		public void SetRemainingTime(float p)
		{
			mTimerLine.text = p.ToString("##");
		}
	}
}
