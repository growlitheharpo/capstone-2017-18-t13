using System;
using KeatsLib.Unity;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	public class StageCaptureUI : MonoBehaviour
	{
		public enum Mode
		{
			NoPoints,
			NoCapturing,
			WereCapturing,
			OtherCapturing,
			PointCaptured,
		}

		[SerializeField] private UIText mStatusLine;
		[SerializeField] private UIFillBarScript mTimerBar;

		public void SetMode(Mode m)
		{
			UnityEngine.Debug.Log("MODE: " + m);
			switch (m)
			{
				case Mode.NoPoints:
					mStatusLine.text = "";
					mTimerBar.gameObject.SetActive(false);
					mTimerBar.SetFillAmount(0.0f, true);
					break;
				case Mode.NoCapturing:
					mStatusLine.text = "A Stage is Available for Capture!";
					mTimerBar.gameObject.SetActive(true);
					mTimerBar.SetFillAmount(0.0f, true);
					break;
				case Mode.WereCapturing:
					mStatusLine.text = "Capturing Stage...";
					break;
				case Mode.OtherCapturing:
					mStatusLine.text = "A Stage is Being Contested!";
					break;
				case Mode.PointCaptured:
					mStatusLine.text = "";
					mTimerBar.gameObject.SetActive(false);
					mTimerBar.SetFillAmount(0.0f, true);
					break;
				default:
					throw new ArgumentOutOfRangeException("m", m, null);
			}
		}

		public void SetCapturePercent(float p)
		{
			mTimerBar.SetFillAmount(p, !mTimerBar.gameObject.activeInHierarchy);
		}
	}
}
