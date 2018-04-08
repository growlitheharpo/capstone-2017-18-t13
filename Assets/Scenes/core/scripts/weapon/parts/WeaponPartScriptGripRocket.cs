using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptGripRocket : WeaponPartScriptGrip
	{
		[SerializeField] private ParticleSystem mBlastEffect;

		private IWeapon mWeapon;

		/// <inheritdoc />
		public override WeaponPartScript SpawnForWeapon(IWeapon weapon)
		{
			WeaponPartScriptGripRocket result = (WeaponPartScriptGripRocket)base.SpawnForWeapon(weapon);

			// Fire event on the player if they are equpping it
			if (weapon.bearer != null && weapon.bearer.isCurrentPlayer)
			{
				EventManager.Notify(EventManager.Local.EquipRocketGrip);
				EventManager.Local.OnLocalPlayerJumped += result.OnLocalPlayerJumped;
			}

			result.mWeapon = weapon;
			return result;
		}

		/// <summary>
		/// Event handler: Local.OnLocalPlayerJumped
		/// </summary>
		private void OnLocalPlayerJumped()
		{
			if (mBlastEffect == null)
				return;

			mBlastEffect.Play(true);
		}

		/// <inheritdoc />
		protected override void OnDestroy()
		{
			if (mWeapon != null && mWeapon.bearer != null && mWeapon.bearer.isCurrentPlayer)
				EventManager.Notify(EventManager.Local.UnequipRocketGrip);

			EventManager.Local.OnLocalPlayerJumped -= OnLocalPlayerJumped;
			base.OnDestroy();
		}
	}
}