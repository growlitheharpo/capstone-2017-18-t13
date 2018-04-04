using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptGripRocket : WeaponPartScriptGrip
	{
		private IWeapon mWeapon;

		/// <inheritdoc />
		public override WeaponPartScript SpawnForWeapon(IWeapon weapon)
		{
			WeaponPartScriptGripRocket result = (WeaponPartScriptGripRocket)base.SpawnForWeapon(weapon);

			// Fire event on the player if they are equpping it
			if (weapon.bearer != null && weapon.bearer.isCurrentPlayer)
				EventManager.Notify(() => EventManager.Local.EquipRocketGrip());

			result.mWeapon = weapon;

			return result;
		}

		/// <inheritdoc />
		protected override void OnDestroy()
		{
			if (mWeapon != null)
				if (mWeapon.bearer != null && mWeapon.bearer.isCurrentPlayer)
					EventManager.Notify(() => EventManager.Local.UnequipRocketGrip());

			base.OnDestroy();
		}
	}
}