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
		/// Attach a new part to this weapon.
		/// </summary>
		/// <param name="partId">The part to be attached.</param>
		void AttachNewPart(string partId);
		
		/// <summary>
		/// Instantiate and fire a projectile immediately with no rule checking.
		/// </summary>
		/// <param name="shotDirections"></param>
		//void FireShotImmediate(List<Ray> shotDirections);

		/// <summary>
		/// Reset the amount of ammo in the clip and play some sort of animation.
		/// </summary>
		void Reload();

		/// <summary>
		/// The underlying transform property for the weapon.
		/// </summary>
		Transform transform { get; }

		/// <summary>
		/// Get the current set of parts on this weapon
		/// </summary>
		WeaponPartCollection currentParts { get; }

		/// <summary>
		/// Gets the current recoil to apply to the bearer's view.
		/// </summary>
		float GetCurrentRecoil();

		void FireWeaponHold();
		void FireWeaponUp();
	}
}
