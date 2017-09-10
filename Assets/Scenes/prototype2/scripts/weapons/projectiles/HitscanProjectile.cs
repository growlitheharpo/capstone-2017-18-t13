using UnityEngine;

namespace Prototype2
{
	public class HitscanProjectile : MonoBehaviour, IProjectile
	{
		public void PreSetup() {}
		public void PostSetup() {}
		public void PreDisable() {}
		public void PostDisable() {}

		public ICharacter source { get { return sourceWeapon.bearer; } }
		public IWeapon sourceWeapon { get; private set; }

		public void Instantiate(IWeapon weapon, Ray ray, WeaponData data)
		{
			sourceWeapon = weapon;
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2000.0f, Color.red, 1.0f / data.fireRate + 0.2f);

			// See if we hit anything
			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit, 2500.0f))
				return;

			// Try to apply damage to it if we did
			IDamageReceiver component = hit.GetDamageReceiver();
			if (component != null)
				component.ApplyDamage(data.damage, hit.point, this);

			Destroy(gameObject);
		}

		public void Instantiate(IWeapon weapon, Ray ray, WeaponData data, GameObjectPool pool)
		{
			sourceWeapon = weapon;
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2000.0f, Color.red, 1.0f / data.fireRate + 0.2f);

			// See if we hit anything
			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit, 2500.0f))
				return;

			// Try to apply damage to it if we did
			IDamageReceiver component = hit.GetDamageReceiver();
			if (component != null)
				component.ApplyDamage(data.damage, hit.point, this);

			pool.ReturnItem(gameObject);
		}
	}
}
