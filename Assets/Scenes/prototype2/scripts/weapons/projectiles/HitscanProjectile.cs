using UnityEngine;

namespace Prototype2
{
	public class HitscanProjectile : MonoBehaviour, IProjectile
	{
		public void PreSetup() {}
		public void PostSetup() {}
		public void PreDisable() {}
		public void PostDisable() {}

		public void Instantiate(Ray ray, WeaponData data)
		{
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2000.0f, Color.red, 1.0f / data.fireRate + 0.2f);

			// See if we hit anything
			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit, 2500.0f))
				return;

			// Try to apply damage to it if we did
			IDamageReceiver component = hit.transform.GetComponent<IDamageReceiver>() ?? hit.transform.parent.GetComponent<IDamageReceiver>();
			if (component != null)
				component.ApplyDamage(data.damage, hit.point);

			Destroy(gameObject);
		}

		public void Instantiate(Ray ray, WeaponData data, GameObjectPool pool)
		{
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2000.0f, Color.red, 1.0f / data.fireRate + 0.2f);

			// See if we hit anything
			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit, 2500.0f))
				return;

			// Try to apply damage to it if we did
			IDamageReceiver component = hit.transform.GetComponent<IDamageReceiver>() ?? hit.transform.parent.GetComponent<IDamageReceiver>();
			if (component != null)
				component.ApplyDamage(data.damage, hit.point);

			pool.ReturnItem(gameObject);
		}
	}
}
