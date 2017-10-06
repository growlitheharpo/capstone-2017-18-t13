using System.Collections;
using KeatsLib.Unity;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay
{
	public interface IPlayerHitIndicator
	{
		void NotifyHit(ICharacter receiver, ICharacter source, float amount);
	}

	public class NullHitIndicator : IPlayerHitIndicator
	{
		public void NotifyHit(ICharacter receiver, ICharacter source, float amount)
		{
			// Do nothing
		}
	}

	public class PlayerHitIndicator : MonoBehaviour, IPlayerHitIndicator
	{
		private const float FADE_OUT_TIME = 0.25f;

		private Color mVisibleColor, mHiddenColor;

		private GameObjectPool mIndicatorPool;

		private void Awake()
		{
			UIImage image = GetComponent<UIImage>();

			mVisibleColor = image.color;
			mHiddenColor = new Color(image.color.r, image.color.g, image.color.b, 0.0f);
			image.color = mHiddenColor;
		}

		private void Start()
		{
			GameObject prefab = Instantiate(gameObject);
			DestroyImmediate(prefab.GetComponent<PlayerHitIndicator>());

			mIndicatorPool = new GameObjectPool(25, prefab, transform);
			Destroy(GetComponent<UIImage>());

			RectTransform t = GetComponent<RectTransform>();
			t.anchorMax = Vector2.one;
			t.anchorMin = Vector2.zero;
			t.offsetMax = Vector2.zero;
			t.offsetMin = Vector2.zero;
		}

		public void NotifyHit(ICharacter receiver, ICharacter source, float amount)
		{
			if (mIndicatorPool.usePercentage >= 1.0f)
				return;

			Vector3 a = receiver.gameObject.transform.position;
			Vector3 b = source.gameObject.transform.position;
			Vector3 dir = b - a;
			float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

			Vector3 cam = receiver.gameObject.transform.forward;
			float camAngle = Mathf.Atan2(cam.z, cam.x) * Mathf.Rad2Deg;

			GameObject newObj = mIndicatorPool.ReleaseNewItem();
			RectTransform t = newObj.GetComponent<RectTransform>();

			t.ResetEverything(100.0f).rotation =
				Quaternion.Euler(0.0f, 0.0f, angle)
				* Quaternion.Euler(0.0f, 0.0f, camAngle);

			StartCoroutine(FadeOutColor(newObj));
		}

		private IEnumerator FadeOutColor(GameObject obj)
		{
			UIImage image = obj.GetComponent<UIImage>();

			float currentTime = 0.0f;
			while (currentTime < FADE_OUT_TIME)
			{
				image.color = Color.Lerp(mVisibleColor, mHiddenColor, currentTime / FADE_OUT_TIME);
				currentTime += Time.deltaTime;
				yield return null;
			}

			image.color = mHiddenColor;
			mIndicatorPool.ReturnItem(obj);
			image.color = mVisibleColor;
		}
	}
}
