using System.Collections;
using UnityEngine;

namespace FiringSquad.Prototyping
{
	/// <summary>
	/// Debug HitDecal.
	/// Display where the player hit something. Fades out after instantiation.
	/// </summary>
	public class HitDecalScript : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private float mFadeTime;

		/// Private variables
		private Renderer mRenderer;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			mRenderer = GetComponent<Renderer>();
			StartCoroutine(FadeOut());
		}

		/// <summary>
		/// Fade out the renderer.
		/// </summary>
		/// <returns></returns>
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
