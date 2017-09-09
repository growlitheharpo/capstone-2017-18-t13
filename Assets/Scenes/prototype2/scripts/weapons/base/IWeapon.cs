using System.Collections.Generic;

namespace Prototype2
{
	public interface IWeapon
	{
		/// <summary>
		/// The character holding this weapon
		/// </summary>
		ICharacter bearer { get; }

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
		/// Instantiate and fire a projectile from this weapon.
		/// </summary>
		void FireWeapon();

		/// <summary>
		/// Reset the amount of ammo in the clip and play some sort of animation.
		/// </summary>
		void Reload();
	}
}
