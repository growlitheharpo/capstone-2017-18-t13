using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using JetBrains.Annotations;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// A specialized character that can be the bearer of a weapon.
	/// </summary>
	public interface IWeaponBearer : ICharacter
	{
		/// <summary>
		/// The weapon currently bound to this bearer.
		/// Can return null if the weapon has not been instantiated yet.
		/// </summary>
		[CanBeNull] IWeapon weapon { get; }

		/// <summary>
		/// The default parts that this bearer automatically equips when they create a weapon.
		/// </summary>
		WeaponPartCollection defaultParts { get; }

		/// <summary>
		/// Utility for the IWeapon.
		/// Immediately plays the "Fire" or "Shoot" animation on this character.
		/// </summary>
		void PlayFireAnimation();

		/// <summary>
		/// Binds the provided weapon to this bearer.
		/// </summary>
		/// <param name="wep">The weapon to bind.</param>
		/// <param name="bindUI">Whether or not the local UI should be bound to this weapon.</param>
		void BindWeaponToBearer(IModifiableWeapon wep, bool bindUI = false);
	}
}
