using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	public interface IModifiableWeapon : IWeapon
	{
		new IWeaponBearer bearer { get; set; }

		Transform aimRoot { get; set; }

		Vector3 positionOffset { get; set; }

		void BindPropertiesToUI();
	}
}
