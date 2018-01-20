using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <summary>
	/// A scriptable object class to handle different "aim down sights" effects on different scopes.
	/// </summary>
	public abstract class AimDownSightsEffect : ScriptableObject
	{
		/// <summary>
		/// Immediately activate the effect for this type of Aim Down Sights
		/// </summary>
		/// <param name="weapon">Which weapon this effect is running on.</param>
		/// <param name="part">The weapon part that this effect is attached to.</param>
		public abstract void ActivateEffect(IWeapon weapon, WeaponPartScript part);

		/// <summary>
		/// Deactivate the effect for this type of Aim Down Sights
		/// </summary>
		/// <param name="weapon">Which weapon this effect is running on.</param>
		/// <param name="part">The weapon part that this effect is attached to.</param>
		/// <param name="immediate">Whether or not to jump immediately to the "exit" state instead of lerping.</param>
		public abstract void DeactivateEffect(IWeapon weapon, WeaponPartScript part, bool immediate);
	}
}
