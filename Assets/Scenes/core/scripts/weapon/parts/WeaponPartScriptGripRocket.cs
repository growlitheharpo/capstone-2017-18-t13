using KeatsLib.Unity;
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
				// 5 frames is barely noticeable, but ensures a re-equip won't cause any errors
				result.StartCoroutine(Coroutines.InvokeAfterFrames(5, EventManager.Local.EquipRocketGrip));
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
				EventManager.Local.UnequipRocketGrip();

			EventManager.Local.OnLocalPlayerJumped -= OnLocalPlayerJumped;
			base.OnDestroy();
		}
	}
}