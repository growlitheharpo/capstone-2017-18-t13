using UnityEngine;

namespace Prototype2
{
	public interface IProjectile : IPoolable
	{
		void Instantiate(Ray initialDirection, WeaponData data);
		void Instantiate(Ray initialDirection, WeaponData data, GameObjectPool pool);
	}
}
