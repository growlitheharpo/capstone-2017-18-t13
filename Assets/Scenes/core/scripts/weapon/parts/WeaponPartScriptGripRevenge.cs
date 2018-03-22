using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptGripRevenge : WeaponPartScriptGrip
	{
		/// Inspector variables
		[SerializeField] private GameObject mProjectile;
		[SerializeField] private WeaponData mProjectileData;

		/// Private variables
		private IWeapon mWeapon;

		/// <inheritdoc />
		public override WeaponPartScript SpawnForWeapon(IWeapon weapon)
		{
			WeaponPartScript result = base.SpawnForWeapon(weapon);

			BaseWeaponScript realWeapon = weapon as BaseWeaponScript;
			if (realWeapon == null)
				return result;

			if (!realWeapon.isServer)
				return result;

			mWeapon = weapon;
			EventManager.Server.OnPlayerDied += OnPlayerDied;
			return result;
		}

		/// <inheritdoc />
		protected override void OnDestroy()
		{
			base.OnDestroy();
			EventManager.Server.OnPlayerDied -= OnPlayerDied;
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

			// Spawn the actual projectile
			if (projectile.PreSpawnInitialize(mWeapon, r, mProjectileData))
				NetworkServer.Spawn(instance);
			projectile.PostSpawnInitialize(mWeapon, r, mProjectileData);
		}
	}
}
