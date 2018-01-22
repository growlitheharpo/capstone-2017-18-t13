using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	/// <summary>
	/// Interface for any projectile damage source spawned from a weapon.
	/// </summary>
	public interface IProjectile : IDamageSource
	{
		/// <summary>
		/// The weapon from which this projectile was spawned.
		/// </summary>
		IWeapon sourceWeapon { get; }

		/// <summary>
		/// Initialize this projectile with any data it needs BEFORE it is spawned on the network.
		/// </summary>
		/// <param name="weapon">The weapon source of this projectile.</param>
		/// <param name="initialDirection">The origin and direction this projectile will spawn with.</param>
		/// <param name="data">The data of the current weapon.</param>
		/// <returns>True if this projectile must be spawned on the network, otherwise false.</returns>
		bool PreSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data);

		/// <summary>
		/// Initialize this projectile with any data it needs AFTER it is spawned on the network.
		/// </summary>
		/// <param name="weapon">The weapon source of this projectile.</param>
		/// <param name="initialDirection">The origin and direction this projectile will spawn with.</param>
		/// <param name="data">The data of the current weapon.</param>
		void PostSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data);
	}
}
