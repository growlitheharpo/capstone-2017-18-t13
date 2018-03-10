using System.Collections;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <summary>
	/// Display the effect for a hitscan bullet.
	/// TODO: This was supposed to just be placeholder. HOW are we still using it??
	/// </summary>
	public class HitscanShootEffect : MonoBehaviour
	{
		enum tEffectType { flash, fade };
		/// Inspector variables
		[SerializeField] private tEffectType mEffectType;
		
		/// <summary>
		/// Used for Flash
		/// </summary>
		[SerializeField] private float mVelocity;
		/// <summary>
		/// Used for Fade
		/// </summary>
		[SerializeField] private float mLifetime;
		/// Private variables
		private LineRenderer mRenderer;
		private Coroutine mEffectRoutine;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mRenderer = GetComponent<LineRenderer>();
		}

		/// <summary>
		/// Start our "flash" bullet effect.
		/// </summary>
		/// <param name="end">The end position of the effect..</param>
		/// <param name="endSpace">Whether the end space is in local or world space. Defaults to world.</param>
		/// <returns>The Coroutine that runs the effect.</returns>
		public void PlayEffect(Vector3 end, Space endSpace = Space.World)
		{
			// Don't change the signature of this function, or this next line here:
			Vector3 realEnd = endSpace == Space.World ? end : transform.TransformPoint(end);

			// TODO: Everything after here can be changed for the new hitscan effect!

			if (mEffectRoutine == null)
			{
				if (mEffectType == 0)
				{
					mEffectRoutine = StartCoroutine(Flash(realEnd));
				}

				else
				{
					mEffectRoutine = StartCoroutine(Fade(realEnd));
				}
			}
		}
		/// <summary>
		/// Actually perform the flash effect by lerping values across our line renderer.
		/// </summary>
		/// <param name="realEnd">The world-space end position of the effect</param>
		private IEnumerator Flash(Vector3 realEnd)
		{
			Vector3 start = transform.position;

			mRenderer.positionCount = 2;
			mRenderer.SetPosition(0, start);
			mRenderer.SetPosition(1, realEnd);

			float time = Vector3.Distance(start, realEnd) / mVelocity;

			float currentTime = 0.0f;
			while (currentTime < time)
			{
				mRenderer.SetPosition(0, Vector3.Lerp(start, realEnd, currentTime / time));
				currentTime += Time.deltaTime;
				yield return null;
			}

			mRenderer.positionCount = 0;
			Destroy(gameObject);
		}

		private IEnumerator Fade(Vector3 realEnd)
		{
			Vector3 start = transform.position;
			GradientAlphaKey[] aStart = (GradientAlphaKey[])mRenderer.colorGradient.alphaKeys.Clone();
			GradientAlphaKey[] newA = (GradientAlphaKey[])mRenderer.colorGradient.alphaKeys.Clone();
			Gradient gr = new Gradient();
			mRenderer.positionCount = 3;
			mRenderer.SetPosition(0, start);
			mRenderer.SetPosition(1, new Vector3((start.x + realEnd.x) / 2, (start.y + realEnd.y) / 2, (start.z + realEnd.z) / 2));
			mRenderer.SetPosition(2, realEnd);

			float time = mLifetime;

			float currentTime = 0.0f;
			while (currentTime < time)
			{
				for (int i = 0; i < aStart.Length; i++)
				{
					float a = Mathf.Lerp(aStart[i].alpha, 0, currentTime / time);
					newA[i] = new GradientAlphaKey(a, newA[i].time);
			   
				}
				gr.SetKeys(mRenderer.colorGradient.colorKeys,newA);
				mRenderer.colorGradient = gr;
				currentTime += Time.deltaTime;
				yield return null;
			}

			mRenderer.positionCount = 0;
			Destroy(gameObject);
		}
	}
}
