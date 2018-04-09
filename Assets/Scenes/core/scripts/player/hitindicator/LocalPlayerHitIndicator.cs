using System.Collections;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.UI;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	public class LocalPlayerHitIndicator : MonoBehaviour, IPlayerHitIndicator
	{
		/// Private variables
		private UIImage mVignetteImage;
		private UIImage mHitSpikeImage;
		private GameObjectPool mIndicatorPool;
		private Color mVisibleRadColor, mHiddenRadColor, mVisibleVinColor, mHiddenVinColor;
		private Coroutine mFadeVignetteRoutine;

		private const float FADE_OUT_TIME = 0.35f;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mVignetteImage = transform.Find("Vignette").GetComponent<UIImage>();
			mHitSpikeImage = transform.Find("Hit Spike").GetComponent<UIImage>();
			mVisibleVinColor = mHiddenVinColor = mVignetteImage.color = mHitSpikeImage.color;
			mHiddenVinColor.a = 0.0f;

			UIImage image = GetComponent<UIImage>();

			mVisibleRadColor = mHiddenRadColor = image.color;
			mHiddenRadColor.a = 0.0f;

			image.color = mHiddenRadColor;
			mVignetteImage.color = mHiddenVinColor;
			mHitSpikeImage.color = mHiddenVinColor;
		}

		/// <summary>
		/// Unity's Start function.
		/// </summary>
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

		/// <inheritdoc />
		public void NotifyHit(ICharacter receiver, Vector3 sourcePosition, Vector3 hitPosition, Vector3 hitNormal, float amount)
		{
			if (mIndicatorPool.usePercentage >= 1.0f)
				return;

			SpawnHitIndicator(receiver.transform, sourcePosition);
			FlashScreenVignette();
		}

		/// <summary>
		/// Display our hit indicator on the canvas.
		/// </summary>
		/// <param name="receiver">The transform of the damage receiver.</param>
		/// <param name="sourcePosition">The world position of the damage source.</param>
		private void SpawnHitIndicator(Transform receiver, Vector3 sourcePosition)
		{
			// "Instantiate" a new hit indicator.
			GameObject newObj = mIndicatorPool.ReleaseNewItem();
			RectTransform t = newObj.GetComponent<RectTransform>();
			t.SetParent(transform);
			t.ResetEverything(new Vector2(0.4f, 0.4f), new Vector2(0.6f, 0.6f));

			// Determine where the hit came from relative to us.
			Vector3 cam = receiver.forward;
			Vector3 a = receiver.position;
			Vector3 b = sourcePosition;
			Vector3 dir = b - a;

			// Determine distance between two for spike scaling
			float dis = Vector3.Distance(a, b);

			// Scale the damage spike based on distance
			// Clamp between scale of .25 and .5
			float yScale = newObj.transform.Find("Hit Spike").transform.localScale.y + dis / 2.0f / 100.0f;
			Mathf.Clamp(yScale, .25f, .5f);
			newObj.transform.Find("Hit Spike").transform.localScale.Set(newObj.transform.Find("Hit Spike").transform.localScale.x, yScale, newObj.transform.Find("Hit Spike").transform.localScale.z);
 
			// "Flatten" the direction along the vertical axis.
			cam = new Vector3(cam.x, 0.0f, cam.z);
			dir = new Vector3(dir.x, 0.0f, dir.z);

			float angle = Vector3.SignedAngle(cam.normalized, dir.normalized, Vector3.down);
			t.localRotation = Quaternion.Euler(0.0f, 0.0f, angle);

			StartCoroutine(FadeOutColor(newObj.GetComponent<UIImage>(), mVisibleRadColor, mHiddenRadColor, FADE_OUT_TIME * 1.75f, true));

			// Start the same coroutine for the hit spike
			StartCoroutine(FadeOutColor(newObj.transform.Find("Hit Spike").GetComponent<UIImage>(), mVisibleRadColor, mHiddenRadColor, FADE_OUT_TIME * 1.75f, false));
		}

		/// <summary>
		/// Flash a red vignette on the screen.
		/// TODO: This can be done using a PostProcessing QuickVolume instead of a UI Image.
		/// </summary>
		private void FlashScreenVignette()
		{
			if (mFadeVignetteRoutine != null)
				StopCoroutine(mFadeVignetteRoutine);

			mVignetteImage.color = mVisibleVinColor;
			mFadeVignetteRoutine = StartCoroutine(FadeOutColor(mVignetteImage, mVisibleVinColor, mHiddenVinColor, FADE_OUT_TIME, false));
		}

		/// <summary>
		/// Fade out the color of a UI element.
		/// </summary>
		/// <param name="image">The </param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="time"></param>
		/// <param name="returnToPool"></param>
		/// <returns></returns>
		private IEnumerator FadeOutColor(Graphic image, Color a, Color b, float time, bool returnToPool)
		{
			image.color = a;
			yield return Coroutines.LerpUIColor(image, b, time);

			if (!returnToPool)
				yield break;

			mIndicatorPool.ReturnItem(image.gameObject);
			image.color = a;
		}
	}
}
