using System.Collections;
using UnityEngine;

namespace FiringSquad.Prototyping
{
	public class HitDecalScript : MonoBehaviour
	{
		[SerializeField] private float mFadeTime;

		private Renderer mRenderer;

		private void Start()
		{
			mRenderer = GetComponent<Renderer>();
			StartCoroutine(FadeOut());
		}

		private IEnumerator FadeOut()
		{
			Color startColor = mRenderer.material.color;
			Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0.0f);
			
			float currentTime = 0.0f;
			while (currentTime < mFadeTime)
			{
				mRenderer.material.color = Color.Lerp(startColor, endColor, Mathf.Pow(currentTime / mFadeTime, 2));
				currentTime += Time.deltaTime;

				yield return null;
			}

			Destroy(gameObject);
		}
	}
}
