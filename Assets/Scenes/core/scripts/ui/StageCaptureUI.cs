using System;
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

		public void SetMode(Mode m)
		{
			UnityEngine.Debug.Log("MODE: " + m);
			switch (m)
			{
				case Mode.NoPoints:
					mStatusLine.text = "";
					break;
				case Mode.NoCapturing:
					mStatusLine.text = "A Stage is Available for Capture!";
					break;
				case Mode.WereCapturing:
					mStatusLine.text = "Capturing Stage...";
					break;
				case Mode.OtherCapturing:
					mStatusLine.text = "A Stage is Being Contested!";
					break;
				case Mode.PointCaptured:
					mStatusLine.text = "";
					break;
				default:
					throw new ArgumentOutOfRangeException("m", m, null);
			}
		}

		public void SetCapturePercent(float p)
		{
			
		}
	}
}
