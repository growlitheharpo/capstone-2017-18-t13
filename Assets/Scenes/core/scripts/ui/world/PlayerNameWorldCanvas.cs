using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to handle displaying a player's name in the world.
	/// </summary>
	public class PlayerNameWorldCanvas : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private Text mDisplayText;

		/// Private variables
		private Transform mLocalPlayerRef;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			StartCoroutine(GrabPlayerReference());
		}

		/// <summary>
		/// Grab a reference to the local player for handling rotations.
		/// </summary>
		private IEnumerator GrabPlayerReference()
		{
			while (mLocalPlayerRef == null)
			{
				yield return null;
				CltPlayer script = CltPlayer.localPlayerReference;

				if (script == null)
					continue;

				mLocalPlayerRef = script.eye.transform;
			}
		}

		/// <summary>
		/// Unity's Update function.
		/// </summary>
		private void Update()
		{
			if (mLocalPlayerRef == null)
				return;

			DoRotate();
		}

		/// <summary>
		/// Rotate towards the player's view.
		/// </summary>
		private void DoRotate()
		{
			Vector3 direction = transform.position - mLocalPlayerRef.position;
			Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
			rot = Quaternion.Euler(0.0f, rot.eulerAngles.y, 0.0f);
			transform.rotation = rot;
		}

		/// <summary>
		/// Set the name displayed on this canvas.
		/// </summary>
		public void SetPlayerName(string newName)
		{
			mDisplayText.text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(newName);
		}

		/// <summary>
		/// Set whether this canvas is on a teammate or an enemy.
		/// </summary>
		/// <param name="isEnemy">True if this is attached to an enemy.</param>
		public void SetIsEnemyPlayer(bool isEnemy)
		{
			mDisplayText.color = isEnemy ? Color.red : Color.grey;
		}
	}
}
