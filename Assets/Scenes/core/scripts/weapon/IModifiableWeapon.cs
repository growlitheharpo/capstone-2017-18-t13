using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	/// <summary>
	/// Interface for a weapon that can be reassigned.
	/// </summary>
	public interface IModifiableWeapon : IWeapon
	{
		/// <summary>
		/// The bearer that this weapon is bound to.
		/// </summary>
		new IWeaponBearer bearer { get; set; }

		/// <summary>
		/// The aim root for this weapon. Usually the character's eye.
		/// </summary>
		new Transform aimRoot { get; set; }

		/// <summary>
		/// The position offset of this weapon to the character's eye.
		/// </summary>
		new Vector3 positionOffset { set; }

		/// <summary>
		/// Bind the properties of this weapon to the UI.
		/// </summary>
		void BindPropertiesToUI();
	}
}
