using UnityEngine;

namespace Prototype2
{
	public interface IProjectile : IPoolable, IDamageSource
	{
		IWeapon sourceWeapon { get; }

		void Instantiate(IWeapon weapon, Ray initialDirection, WeaponData data);
		void Instantiate(IWeapon weapon, Ray initialDirection, WeaponData data, GameObjectPool pool);
	}
}
