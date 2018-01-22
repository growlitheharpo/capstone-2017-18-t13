using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Interface for all objects that can receive damage from IWeapons.
	/// </summary>
	public interface IDamageReceiver
	{
		/// <summary>
		/// Apply damage to this receiver.
		/// </summary>
		/// <param name="amount">The amount of damage to apply.</param>
		/// <param name="point">The world position where this damage occurred.</param>
		/// <param name="normal">The normal/tangent of the hit.</param>
		/// <param name="cause">The source of this damage.</param>
		void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause);
	}
}
