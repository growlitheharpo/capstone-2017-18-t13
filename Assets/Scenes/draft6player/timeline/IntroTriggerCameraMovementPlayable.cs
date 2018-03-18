using UnityEngine;

namespace FiringSquad.Gameplay.Timeline
{
	/// <summary>
	/// Utility Playable that handles sending messages about the camera
	/// from timeline to the gameplay code.
	/// </summary>
	public class IntroTriggerCameraMovementPlayable : MonoBehaviour
	{
		/// <summary>
		/// Unity's OnEnable function
		/// </summary>
		private void OnEnable()
		{
			EventManager.Local.IntroBegin();
		}

		/// <summary>
		/// Unity's OnDisable function.
		/// </summary>
		private void OnDisable()
		{
			EventManager.Local.IntroEnd();
		}
	}
}
