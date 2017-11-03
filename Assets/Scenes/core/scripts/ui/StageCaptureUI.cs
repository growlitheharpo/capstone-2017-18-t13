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
		[SerializeField] private UIText mTimerLine;
		[SerializeField] private StageCapturePointer mPointer;
		[SerializeField] private UIFillBarScript mTimerBar;

		public void SetMode(Mode m, StageCaptureArea area)
		{
			switch (m)
			{
				case Mode.NoPoints:
					mStatusLine.text = "";
					mTimerLine.gameObject.SetActive(false);
					mTimerBar.gameObject.SetActive(false);
					mPointer.StopPointing();
					break;
				case Mode.NoCapturing:
					mStatusLine.text = "A Stage is Available for Capture!";
					mTimerLine.gameObject.SetActive(true);
					mTimerBar.gameObject.SetActive(false);
					mTimerBar.SetFillAmount(0.0f, true);
					mPointer.EnableAndPoint(area);
					break;
				case Mode.WereCapturing:
					mTimerLine.gameObject.SetActive(false);
					mTimerBar.gameObject.SetActive(true);
					mStatusLine.text = "Capturing Stage...";
					break;
				case Mode.OtherCapturing:
					mStatusLine.text = "A Stage is Being Contested!";
					mTimerLine.gameObject.SetActive(false);
					mTimerBar.gameObject.SetActive(true);
					mPointer.EnableAndPoint(area);
					break;
				case Mode.PointCaptured:
					mStatusLine.text = "";
					mTimerLine.gameObject.SetActive(false);
					mTimerBar.gameObject.SetActive(false);
					mTimerBar.SetFillAmount(0.0f, true);
					mPointer.StopPointing();
					break;
				default:
					throw new ArgumentOutOfRangeException("m", m, null);
			}
		}

		public void SetCapturePercent(float p)
		{
			mTimerBar.SetFillAmount(p, !mTimerBar.gameObject.activeInHierarchy);
		}

		public void SetRemainingTime(float p)
		{
			mTimerLine.text = p.ToString("##");
		}
	}
}
