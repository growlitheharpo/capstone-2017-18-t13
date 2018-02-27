using System;
using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <summary>
	/// The four weapon attachment spots.
	/// </summary>
	[Flags]
	public enum Attachment
	{
		Scope = 0x1,
		Barrel = 0x2,
		Mechanism = 0x4,
		Grip = 0x8,
	}

	/// <summary>
	/// The interface for a basic weapon in the game.
	/// </summary>
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
		/// The current data for this weapon, including things like recoil effects.
		/// </summary>
		WeaponData currentData { get; }

		/// <summary>
		/// Whether or not this weapon is currently in Aim Down Sights mode.
		/// </summary>
		bool aimDownSightsActive { get; }

		/// <summary>
		/// Attach a new part to this weapon.
		/// </summary>
		/// <param name="partId">The part to be attached.</param>
		/// <param name="durability">The durability to assign to the new part, or -2 to use the default.</param>
		void AttachNewPart(byte partId, int durability = WeaponPartScript.USE_DEFAULT_DURABILITY);

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
		/// The aim root for this weapon. Usually the character's eye.
		/// </summary>
		Transform aimRoot { get; }

		/// <summary>
		/// The position offset of this weapon to the character's eye.
		/// </summary>
		Vector3 positionOffset { get; }

		/// <summary>
		/// Get the current set of parts on this weapon
		/// </summary>
		WeaponPartCollection currentParts { get; }

		/// <summary>
		/// Gets the current recoil to apply to the bearer's view.
		/// </summary>
		float GetCurrentRecoil();

		/// <summary>
		/// Get the current dispersion factor based on the current stats of the weapon.
		/// </summary>
		/// <param name="forceNotZero">Whether or not this is the first shot and should be 0.</param>
		/// <returns></returns>
		float GetCurrentDispersionFactor(bool forceNotZero);

		/// <summary>
		/// Input handler: Handle the trigger being held for this weapon.
		/// </summary>
		void FireWeaponHold();

		/// <summary>
		/// Input handler: Handle the trigger being released for this weapon.
		/// </summary>
		void FireWeaponUp();

		/// <summary>
		/// Input handler: Handle the player entering AimDownSights for this weapon.
		/// Generally should only be run on the local player's weapon.
		/// </summary>
		void EnterAimDownSightsMode();

		/// <summary>
		/// Input handler: Handle the player exiting AimDownSights for this weapon.
		/// Generally should only be run on the local player's weapon.
		/// </summary>
		void ExitAimDownSightsMode();
	}
}
