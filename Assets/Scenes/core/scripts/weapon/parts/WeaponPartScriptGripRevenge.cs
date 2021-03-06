﻿using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptGripRevenge : WeaponPartScriptGrip
	{
		/// Inspector variables
		[SerializeField] private GameObject mProjectile;
		[SerializeField] private float mProjectileDamage;

		/// Private variables
		private IWeapon mWeapon;

		/// <inheritdoc />
		public override WeaponPartScript SpawnForWeapon(IWeapon weapon)
		{
			WeaponPartScriptGripRevenge result = (WeaponPartScriptGripRevenge)base.SpawnForWeapon(weapon);

			BaseWeaponScript realWeapon = weapon as BaseWeaponScript;
			if (realWeapon == null)
				return result;

			if (!realWeapon.isServer)
				return result;

			result.mWeapon = weapon;
			EventManager.Server.OnPlayerDied += result.OnPlayerDied;
			return result;
		}

		/// <inheritdoc />
		protected override void OnDestroy()
		{
			EventManager.Server.OnPlayerDied -= OnPlayerDied;
			base.OnDestroy();
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnPlayerDied
		/// Spawn our projectile. This happens ON THE SERVER so we can call it directly.
		/// </summary>
		private void OnPlayerDied(CltPlayer player, PlayerKill killInfo)
		{
			// Make sure it was our player that died
			if (player == null || mWeapon == null || !ReferenceEquals(mWeapon.bearer, player))
				return;

			// Create a fake ray
			Ray r = new Ray(transform.position, Vector3.up);
			GameObject instance = Instantiate(mProjectile, r.origin, Quaternion.identity);
			IProjectile projectile = instance.GetComponent<IProjectile>();

			// Create some fake weapon data to go along with it.
			WeaponData tmpWeaponData = new WeaponData();
			tmpWeaponData.ForceModifyDamage(new Modifier.Float(mProjectileDamage, Modifier.ModType.SetAbsolute));

			// Spawn the actual projectile
			if (projectile.PreSpawnInitialize(mWeapon, r, tmpWeaponData))
				NetworkServer.Spawn(instance);
			projectile.PostSpawnInitialize(mWeapon, r, tmpWeaponData);
		}
	}
}
