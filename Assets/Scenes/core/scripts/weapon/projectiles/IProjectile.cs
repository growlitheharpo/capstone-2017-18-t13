using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public interface IProjectile : IDamageSource
	{
		IWeapon sourceWeapon { get; }

		void Initialize(IWeapon weapon, Ray initialDirection, WeaponData data);
	}
}
