using UnityEngine;
using UnityEngine.Networking;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Component for the healthkits that are scattered throughout the level.
	/// Provide the player with a set amount of health when colided with on the server.
	/// </summary>
	public class PlayerHealthPack : NetworkBehaviour
	{
		[SerializeField] private float mProvidedHealth;
		[SerializeField] private float mRotationRate;

		// Update is called once per frame
		private void Update()
		{
			DoRotation();
		}

		/// <summary>
		/// Rotate the healthkit around the world "up" axis.
		/// Note: the healthkit uses a sphere hitbox instead of the mesh, so rotation
		/// does not need to be synced exactly across the network.
		/// </summary>
		private void DoRotation()
		{
			transform.Rotate(Vector3.up, mRotationRate * Time.deltaTime);
		}

		/// <summary>
		/// Unity callback: The healthkit has been colided with on the server.
		/// </summary>
		[ServerCallback]
		private void OnTriggerEnter(Collider other)
		{
			ICharacter player = other.GetComponent<ICharacter>();
			if (player == null)
				return;

			Logger.Info("Hit by the player!");
		}
	}

}
