using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Object that exists only on the server to kill any players that fall through the world.
	/// </summary>
	public class FallingPlayerKiller : NetworkBehaviour, IDamageSource
	{
		/// <inheritdoc />
		public ICharacter source { get { return null; } }

		/// <summary>
		/// Unity Event handler when something hits our collider.
		/// </summary>
		[ServerCallback]
		private void OnTriggerEnter(Collider other)
		{
			IDamageReceiver damageReceiver = other.GetComponent<IDamageReceiver>();
			if (damageReceiver != null)
				damageReceiver.ApplyDamage(damageReceiver.currentHealth + 500000.0f, other.transform.position, Vector3.up, this);
		}
	}
}
