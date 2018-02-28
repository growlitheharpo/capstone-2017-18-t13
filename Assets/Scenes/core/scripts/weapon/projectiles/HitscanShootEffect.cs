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
		/// Inspector variables
		[SerializeField] private float mVelocity;

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
				mEffectRoutine = StartCoroutine(Flash(realEnd));
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
	}
}
