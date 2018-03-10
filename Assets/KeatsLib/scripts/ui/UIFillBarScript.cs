using System.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace KeatsLib.Unity
{
	/// <summary>
	/// A utility UI class attached to a prefab. This is useful for a bar that needs to fill/drain, such as progress or
	/// health.
	/// </summary>
	public class UIFillBarScript : MonoBehaviour
	{
		[SerializeField] private UIImage mDelayBar;
		[SerializeField] private float mDelayTime;
		[SerializeField] private UIImage mFillBar;

		[SerializeField] private UIImage.FillMethod mFillDirection;

		private void Awake()
		{
			if (mDelayBar != null)
				mDelayBar.fillMethod = mFillDirection;

			if (mFillBar != null)
				mFillBar.fillMethod = mFillDirection;
		}

		/// <summary>
		/// Set the new fill amount of the bar (from 0 to 1)
		/// </summary>
		/// <param name="amount">The new amount.</param>
		/// <param name="immediate">Whether to force the value to the new amount immediately</param>
		public void SetFillAmount(float amount, bool immediate = false)
		{
			StopAllCoroutines();
			if (immediate)
			{
				if (mFillBar != null)
					mFillBar.fillAmount = amount;

				if (mDelayBar != null)
					mDelayBar.fillAmount = amount;

				return;
			}

			if (mFillBar != null && mFillBar.fillAmount > amount)
			{
				mFillBar.fillAmount = amount;

				if (mDelayBar != null)
					StartCoroutine(LerpRoutine(amount, mDelayBar));
			}
			else
			{
				if (mDelayBar != null)
					mDelayBar.fillAmount = amount;

				if (mFillBar != null)
					StartCoroutine(LerpRoutine(amount, mFillBar));
			}
		}

		private IEnumerator LerpRoutine(float newAmount, UIImage bar)
		{

			float startAmount = bar.fillAmount;
			float currentTime = 0.0f;

			while (currentTime < mDelayTime)
			{
				bar.fillAmount = Mathf.Lerp(startAmount, newAmount, Mathf.Sqrt(currentTime / mDelayTime));

				currentTime += Time.deltaTime;
				yield return null;
			}

			bar.fillAmount = newAmount;
		}
	}
}
