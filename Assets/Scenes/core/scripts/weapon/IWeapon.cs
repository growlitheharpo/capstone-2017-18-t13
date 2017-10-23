using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
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
		/// <param name="durability">The durability to assign to the new part, or -2 to use the default.</param>
		void AttachNewPart(string partId, int durability = WeaponPartScript.USE_DEFAULT_DURABILITY);

		/// <summary>
		/// Reset all the parts on this weapon to the default parts of the bearer.
		/// </summary>
		void ResetToDefaultParts();
		
		/// <summary>
		/// Reset the amount of ammo in the clip and play some sort of animation.
		/// </summary>
		void Reload();

		/// <summary>
		/// The underlying transform property for the weapon.
		/// </summary>
		Transform transform { get; }

		/// <summary>
		/// The underlying gameObject property for the weapon.
		/// </summary>
		GameObject gameObject { get; }

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

		void PlayFireEffect();
	}
}
