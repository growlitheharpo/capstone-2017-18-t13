using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype2
{
	public class RocketProjectile : MonoBehaviour, IProjectile
	{
		[SerializeField] private ParticleSystem mHitParticles;
		[SerializeField] private float mSpeed;

		private Rigidbody mRigidbody;
		private Renderer mRenderer;
		private GameObjectPool mPool;
		private WeaponData mData;

		private void Awake()
		{
			mRigidbody = GetComponent<Rigidbody>();
			mRenderer = GetComponent<Renderer>();
		}

		private void OnCollisionEnter(Collision hit)
		{
			IDamageReceiver component = hit.transform.GetComponent<IDamageReceiver>();
			if (component == null && hit.transform.parent != null)
				component = hit.transform.parent.GetComponent<IDamageReceiver>();

			if (component != null)
				component.ApplyDamage(mData.damage, hit.contacts[0].point);

			StartCoroutine(ExplodeEffect());
		}
		
		private IEnumerator ExplodeEffect()
		{
			mRenderer.enabled = false;
			mRigidbody.velocity = Vector3.zero;
			mRigidbody.angularVelocity = Vector3.zero;

			mHitParticles.transform.SetParent(null);
			mHitParticles.transform.localScale = Vector3.one;
			mHitParticles.Play();
			yield return new WaitForParticles(mHitParticles);

			OnEffectComplete();
		}

		private void OnEffectComplete()
		{
			mHitParticles.transform.SetParent(transform);
			mHitParticles.transform.localPosition = Vector3.zero;

			if (mPool != null)
				mPool.ReturnItem(gameObject);
			else
				Destroy(gameObject);
		}

		#region IPoolable Implementation

		public void PreSetup() { }
		public void PostSetup()
		{
			transform.SetParent(null);
			mRenderer.enabled = true;
			mRigidbody.velocity = Vector3.zero;
		}

		public void PreDisable() { }
		public void PostDisable() { }
		
		#endregion
		
		#region IProjectile Implementation

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

		#endregion
	}
}
