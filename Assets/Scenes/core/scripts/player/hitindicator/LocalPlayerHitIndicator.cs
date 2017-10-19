using System.Collections;
using KeatsLib.Unity;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	public class LocalPlayerHitIndicator : MonoBehaviour, IPlayerHitIndicator
	{
		private UIImage mVignetteImage;
		private GameObjectPool mIndicatorPool;
		private Color mVisibleRadColor, mHiddenRadColor, mVisibleVinColor, mHiddenVinColor;
		private Coroutine mFadeVignetteRoutine;

		private const float FADE_OUT_TIME = 0.25f;

		private void Awake()
		{
			mVignetteImage = transform.Find("Vignette").GetComponent<UIImage>();
			mVisibleVinColor = mHiddenVinColor = mVignetteImage.color;
			mHiddenVinColor.a = 0.0f;

			UIImage image = GetComponent<UIImage>();

			mVisibleRadColor = mHiddenRadColor = image.color;
			mHiddenRadColor.a = 0.0f;

			image.color = mHiddenRadColor;
			mVignetteImage.color = mHiddenVinColor;
		}

		private void Start()
		{
			GameObject prefab = Instantiate(gameObject);
			DestroyImmediate(prefab.GetComponent<LocalPlayerHitIndicator>());

			mIndicatorPool = new GameObjectPool(25, prefab, transform);
			Destroy(GetComponent<UIImage>());

			RectTransform t = GetComponent<RectTransform>();
			t.anchorMax = Vector2.one;
			t.anchorMin = Vector2.zero;
			t.offsetMax = Vector2.zero;
			t.offsetMin = Vector2.zero;
		}

		public void NotifyHit(ICharacter receiver, Vector3 sourcePosition, Vector3 hitPosition, Vector3 hitNormal, float amount)
		{
			if (mIndicatorPool.usePercentage >= 1.0f)
				return;

			SpawnHitIndicator(receiver, sourcePosition, amount);
			FlashScreenVignette();
		}

		private void SpawnHitIndicator(ICharacter receiver, Vector3 sourcePosition, float amount)
		{
			GameObject newObj = mIndicatorPool.ReleaseNewItem();
			RectTransform t = newObj.GetComponent<RectTransform>().ResetEverything(100.0f);

			Vector3 cam = receiver.gameObject.transform.forward;
			Vector3 a = receiver.gameObject.transform.position;
			Vector3 b = sourcePosition;
			Vector3 dir = b - a;

			cam = new Vector3(cam.x, 0.0f, cam.z);
			dir = new Vector3(dir.x, 0.0f, dir.z);

			float angle = Vector3.SignedAngle(cam.normalized, dir.normalized, Vector3.down);
			t.rotation = Quaternion.Euler(0.0f, 0.0f, angle);

			StartCoroutine(FadeOutColor(newObj.GetComponent<UIImage>(), mVisibleRadColor, mHiddenRadColor, FADE_OUT_TIME * 1.75f, true));
		}

		private void FlashScreenVignette()
		{
			if (mFadeVignetteRoutine != null)
				StopCoroutine(mFadeVignetteRoutine);

			mVignetteImage.color = mVisibleVinColor;
			mFadeVignetteRoutine = StartCoroutine(FadeOutColor(mVignetteImage, mVisibleVinColor, mHiddenVinColor, FADE_OUT_TIME, false));
		}

		private IEnumerator FadeOutColor(UIImage image, Color a, Color b, float time, bool returnToPool)
		{
			float currentTime = 0.0f;
			while (currentTime < time)
			{
				image.color = Color.Lerp(a, b, currentTime / time);
				currentTime += Time.deltaTime;
				yield return null;
			}

			image.color = b;

			if (returnToPool)
			{
				mIndicatorPool.ReturnItem(image.gameObject);
				image.color = a;
			}
		}
	}
}
