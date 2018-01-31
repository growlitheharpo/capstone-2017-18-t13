using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Class used to give the HUD inertia
	/// </summary>
	public class HudInertia : MonoBehaviour
	{
		// Serialized fields for number tweaking
		//[SerializeField]
		//private float mDTscalar;
		//[SerializeField]
		//private float mMaxOffset;

		// private variables
		private CltPlayer mPlayer; // keeping track of the player in order to get their rotation
		// keeping track of the player's previous rotation
		private Quaternion mPlayerPrevRot;
		private Vector3 mOrigPosition;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			EventManager.Local.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
			//mOrigPosition = transform.localPosition;
		}

		/// <summary>
		/// Unity's fixed update function
		/// </summary>
		void FixedUpdate()
		{
			CheckPlayerRotations(mPlayerPrevRot);
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerSpawned
		/// </summary>
		private void OnLocalPlayerSpawned(CltPlayer obj)
		{
			mPlayer = obj; // Save a reference to the local player for their movement rotation
			mPlayerPrevRot = mPlayer.transform.localRotation;
			mOrigPosition = transform.localPosition;
		}

		/// <summary>
		/// Function to see if the player has rotated or not
		/// </summary>
		/// <param name="prevRot"> The player's previous rotation </param>
		private void CheckPlayerRotations(Quaternion prevRot)
		{
			// Make sure the player reference exists first
			if (mPlayer)
			{
				Quaternion newRot = mPlayer.transform.localRotation;
				// if the previous rotation is not the same as the current rotation
				if (prevRot != newRot)
				{
					// check if the rotation is greater in euler angles than the previous
					if (newRot.eulerAngles.y > prevRot.eulerAngles.y)
					{
						transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(mOrigPosition.x - 50.0f, mOrigPosition.y, mOrigPosition.z), Time.deltaTime / 2f);
					}
					else if (newRot.eulerAngles.y < prevRot.eulerAngles.y)
					{
						transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(mOrigPosition.x + 50.0f, mOrigPosition.y, mOrigPosition.z), Time.deltaTime / 2f);
					}
				}
				else
				{
					transform.localPosition = Vector3.Lerp(transform.localPosition, mOrigPosition, Time.deltaTime * 3);
				}

				mPlayerPrevRot = newRot;
			}  
		}
	}
}

