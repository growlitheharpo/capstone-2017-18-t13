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
		[SerializeField] private UIImage mBackground;
		[SerializeField] private UIImage mDelayBar;
		[SerializeField] private float mDelayTime;
		[SerializeField] private UIImage mFillBar;

		[SerializeField] private UIImage.FillMethod mFillDirection;

		private void Awake()
		{
			if (mBackground == null || mDelayBar == null || mFillBar == null)
			{
				Destroy(this);
				return;
			}

			mDelayBar.fillMethod = mFillDirection;
			mFillBar.fillMethod = mFillDirection;
		}

		/// <summary>
		/// Set the new fill amount of the bar (from 0 to 1)
		/// </summary>
		/// <param name="amount">The new amount.</param>
		public void SetFillAmount(float amount)
		{
			StopAllCoroutines();
			if (mFillBar.fillAmount > amount)
			{
				mFillBar.fillAmount = amount;
				StartCoroutine(DrainRoutine(amount));
			}
			else
			{
				mDelayBar.fillAmount = amount;
				StartCoroutine(FillRoutine(amount));
			}
		}

		private IEnumerator DrainRoutine(float newAmount)
		{
			float startAmount = mDelayBar.fillAmount;
			float currentTime = 0.0f;

			while (currentTime < mDelayTime)
			{
				mDelayBar.fillAmount = Mathf.Lerp(startAmount, newAmount, Mathf.Sqrt(currentTime / mDelayTime));

				currentTime += Time.deltaTime;
				yield return null;
			}

			mDelayBar.fillAmount = newAmount;
		}

		private IEnumerator FillRoutine(float newAmount)
		{
			float startAmount = mFillBar.fillAmount;
			float currentTime = 0.0f;

			while (currentTime < mDelayTime)
			{
				mFillBar.fillAmount = Mathf.Lerp(startAmount, newAmount, Mathf.Sqrt(currentTime / mDelayTime));

				currentTime += Time.deltaTime;
				yield return null;
			}

			mFillBar.fillAmount = newAmount;
		}
	}
}
