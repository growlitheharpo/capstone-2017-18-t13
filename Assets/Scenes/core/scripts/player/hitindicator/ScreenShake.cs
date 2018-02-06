using System.Collections;
using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Screen Shake as a hit indicator
	/// </summary>
	/// <inheritdoc cref="IPlayerHitIndicator" />
	public class ScreenShake : MonoBehaviour, IPlayerHitIndicator
	{
		/// Inspector variables
		[SerializeField] private float mAmount;
		[SerializeField] private float mDuration;

		// Private variables
		private Vector3 mOrigPosition;

		/// <summary>
		/// Unity's awake function
		/// </summary>
		private void Awake()
		{
			// Get the original position
			mOrigPosition = Vector3.zero;
		}

		/// <inheritdoc />
		public void NotifyHit(ICharacter receiver, Vector3 sourcePosition, Vector3 hitPosition, Vector3 hitNormal, float amount)
		{
			// Do the screenshake
			mDuration = 0.2f;
			StartCoroutine(ShakeScreen(mDuration));
		}

		/// <summary>
		/// Briefly shake the screen when hit by a projectile
		/// </summary>
		private IEnumerator ShakeScreen(float time)
		{
			float currentTime = 0.0f;
			while (currentTime < time)
			{
				// Get random position within the unit circle to move the camera to
				Vector2 new_xy = new Vector2(mOrigPosition.x, mOrigPosition.y);
				new_xy = new_xy + Random.insideUnitCircle * mAmount;

				transform.localPosition = new Vector3(new_xy.x, new_xy.y, mOrigPosition.z);

				currentTime += Time.deltaTime;
				yield return null;
			}

			transform.localPosition = Vector3.zero;
			mDuration = 0.0f;
		}
	}
}


