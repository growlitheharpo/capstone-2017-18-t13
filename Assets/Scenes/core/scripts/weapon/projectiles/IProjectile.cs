using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public interface IProjectile : IPoolable, IDamageSource
	{
		IWeapon sourceWeapon { get; }

		void Instantiate(IWeapon weapon, Ray initialDirection, WeaponData data);
		void Instantiate(IWeapon weapon, Ray initialDirection, WeaponData data, GameObjectPool pool);
	}
}
