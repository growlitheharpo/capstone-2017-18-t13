using System.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

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
