using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	public interface IProjectile : IDamageSource
	{
		IWeapon sourceWeapon { get; }

		void PreSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data);
		void PostSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data);
	}
}
