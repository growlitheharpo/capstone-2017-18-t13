using System.Collections.Generic;
using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public interface IWeapon
	{
		/// <summary>
		/// The character holding this weapon
		/// </summary>
		IWeaponBearer bearer { get; }

		/// <summary>
		/// The base data for this weapon.
		/// </summary>
		WeaponData baseData { get; }
		/// <summary>
		/// The currently attached part modifiers.
		/// </summary>
		IEnumerable<WeaponPartScript> parts { get; }

		/// <summary>
		/// Attach a new part to this weapon.
		/// </summary>
		/// <param name="part">The part to be attached.</param>
		void AttachNewPart(WeaponPartScript part);

		/// <summary>
		/// Notify the weapon that the trigger has been pressed.
		/// Instantiate and fire a projectile from this weapon. Notifies network.
		/// </summary>
		void FireWeaponDown();

		/// <summary>
		/// Notify the weapon every frame the trigger is held.
		/// </summary>
		void FireWeaponHold();

		/// <summary>
		/// Notify the weapon when the trigger is released.
		/// </summary>
		void FireWeaponUp();

		/// <summary>
		/// Instantiate and fire projectiles immediately.
		/// </summary>
		/// <param name="shotDirections"></param>
		void FireShotImmediate(List<Ray> shotDirections);

		/// <summary>
		/// Reset the amount of ammo in the clip and play some sort of animation.
		/// </summary>
		void Reload();

		/// <summary>
		/// The underlying transform property for the weapon.
		/// </summary>
		Transform transform { get; }

		/// <summary>
		/// Gets the current recoil amount.
		/// </summary>
		float GetCurrentRecoil();
	}
}
