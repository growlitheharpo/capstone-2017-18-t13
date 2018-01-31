using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KeatsLib.Unity;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Screen Shake as a hit indicator
	/// </summary>
	/// <inheritdoc cref="IPlayerHitIndicator" />
	public class ScreenShake : MonoBehaviour, IPlayerHitIndicator
	{
		// Private variables
		private Transform mCameraTransform;
		[SerializeField]
		private float mAmount;
		[SerializeField]
		private float mDuration;

		Vector3 mOrigPosition;

		/// <summary>
		/// Unity's awake function
		/// </summary>
		private void Awake()
		{
			// If the camera transform is null
			if (!mCameraTransform)
			{
				mCameraTransform = GetComponent<Transform>(); // Get the transform
			}

			// Get the original position
			mOrigPosition = new Vector3(0, 0, 0);

			// Event handlers 
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
				mCameraTransform.localPosition = new Vector3(new_xy.x, new_xy.y, mOrigPosition.z);

				currentTime += Time.deltaTime;
				yield return null;
			}

			mCameraTransform.localPosition = mOrigPosition;
			mDuration = 0.0f;
		}
	}
}


