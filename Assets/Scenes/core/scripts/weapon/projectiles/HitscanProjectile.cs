using System.Collections;
using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class HitscanProjectile : MonoBehaviour, IProjectile
	{
		public void PreSetup() {}
		public void PostSetup() {}
		public void PreDisable() {}
		public void PostDisable() {}

		private HitscanShootEffect mEffect;

		private void Awake()
		{
			mEffect = GetComponent<HitscanShootEffect>();
		}

		public ICharacter source { get { return sourceWeapon.bearer; } }
		public IWeapon sourceWeapon { get; private set; }

		public void Instantiate(IWeapon weapon, Ray ray, WeaponData data)
		{
			HandleShot(weapon, ray, data);
		}

		public void Instantiate(IWeapon weapon, Ray ray, WeaponData data, GameObjectPool pool)
		{
			HandleShot(weapon, ray, data, pool);
		}

		private void HandleShot(IWeapon weapon, Ray ray, WeaponData data, GameObjectPool pool = null)
		{
			SetupShot(weapon);

			UnityEngine.Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2000.0f, Color.red, 1.0f / data.fireRate + 0.2f);

			// See if we hit anything
			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit))
			{
				KillSelf(pool);
				return;
			}

			// Try to apply damage to it if we did
			IDamageReceiver component = hit.GetDamageReceiver();
			if (component != null)
				component.ApplyDamage(data.damage, hit.point, this);

			StartCoroutine(PlayEffectAndKillSelf(pool, hit.point));
		}
		
		private void SetupShot(IWeapon weapon)
		{
			sourceWeapon = weapon;

			transform.position = weapon.transform.position;
			transform.forward = weapon.transform.forward;
		}

		private IEnumerator PlayEffectAndKillSelf(GameObjectPool pool, Vector3 hitPoint)
		{
			yield return mEffect.Flash(hitPoint);
			KillSelf(pool);
		}

		private void KillSelf(GameObjectPool pool)
		{
			if (pool != null)
				pool.ReturnItem(gameObject);
			else
				Destroy(gameObject);
		}
	}
}
