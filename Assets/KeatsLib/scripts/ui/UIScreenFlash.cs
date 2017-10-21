using System.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace KeatsLib.Unity
{
	/// <summary>
	/// A utility UI class attached to a prefab. Used to flash the whole screen a given color.
	/// </summary>
	public class UIScreenFlash : MonoBehaviour
	{
		private const float DEFAULT_FLASH_TIME = 0.55f;
		[SerializeField] private UIImage mFlashImage;
		private Coroutine mFlashRoutine;

		private void Awake()
		{
			if (mFlashImage == null)
				Destroy(this);
		}

		/// <summary>
		/// Perform the screen flash.
		/// </summary>
		/// <param name="col">The color to flash to.</param>
		/// <param name="flashCount">The number of times to flash the screen. Defaults to 1.</param>
		/// <param name="time">The time a flash should take. Defaults to 0.55s</param>
		public void FlashScreen(Color col, int flashCount = 1, float time = DEFAULT_FLASH_TIME)
		{
			if (mFlashRoutine != null)
				StopCoroutine(mFlashRoutine);

			mFlashRoutine = StartCoroutine(DoFlash(col, flashCount, time));
		}

		private IEnumerator DoFlash(Color col, int flashCount, float time)
		{
			float currentTime = 0.0f;
			float totalTime = flashCount * time;
			Color baseColor = new Color(col.r, col.g, col.b, 0.0f);

			float halfTime = time / 2.0f;

			while (currentTime < totalTime)
			{
				mFlashImage.color = Color.Lerp(baseColor, col, Mathf.PingPong(currentTime, halfTime) / halfTime);

				currentTime += Time.deltaTime;
				yield return null;
			}

			mFlashImage.color = baseColor;
		}
	}
}
