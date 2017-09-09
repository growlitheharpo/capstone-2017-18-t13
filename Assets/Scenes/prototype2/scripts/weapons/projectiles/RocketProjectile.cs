using System.Collections;
using System.Collections.Generic;
using Prototype2;
using UnityEngine;

namespace Prototype2
{
	public class RocketProjectile : MonoBehaviour, IProjectile
	{
		[SerializeField] private float mSpeed;
		private Rigidbody mRigidbody;
		private GameObjectPool mPool;
		private WeaponData mData;

		private void Awake()
		{
			mRigidbody = GetComponent<Rigidbody>();
		}


		private void OnCollisionEnter(Collision other)
		{
			IDamageReceiver receiver = other.transform.GetComponent<IDamageReceiver>();
			if (receiver != null)
				receiver.ApplyDamage(mData.damage, other.contacts[0].point);

			if (mPool != null)
				mPool.ReturnItem(gameObject);
			else
				Destroy(gameObject);
		}

		public void PreSetup() { }

		public void PostSetup()
		{
			mRigidbody.velocity = Vector3.zero;
		}

		public void PreDisable()
		{
		}

		public void PostDisable()
		{
		}

		public void Instantiate(Ray ray, WeaponData data)
		{
			mPool = null;
			transform.position = ray.origin;

			mRigidbody.AddForce(ray.direction * mSpeed, ForceMode.Impulse);
			mData = data;
		}

		public void Instantiate(Ray ray, WeaponData data, GameObjectPool pool)
		{
			mPool = pool;
			transform.position = ray.origin;

			mRigidbody.AddForce(ray.direction * mSpeed, ForceMode.Impulse);
			mData = data;
		}
	}
}
