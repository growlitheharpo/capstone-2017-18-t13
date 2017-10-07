using System.Collections;
using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class RocketProjectile : MonoBehaviour, IProjectile
	{
		[SerializeField] private ParticleSystem mHitParticles;
		[SerializeField] private float mSpeed;

		private Transform mDirectHit;
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
			IDamageReceiver component = hit.GetDamageReceiver();
			if (component != null)
			{
				component.ApplyDamage(mData.damage, hit.contacts[0].point, hit.contacts[0].normal, this);
				mDirectHit = hit.transform;
			}

			ApplyExplodeDamage();
			StartCoroutine(ExplodeEffect());
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(transform.position, 1.5f);
		}

		private void ApplyExplodeDamage()
		{
			var hitInfo = Physics.SphereCastAll(transform.position, 1.5f, Vector3.up, 0.01f);
			foreach (RaycastHit hit in hitInfo)
			{
				if (hit.transform == mDirectHit)
					continue;

				IDamageReceiver component = hit.GetDamageReceiver();
				if (component != null)
					component.ApplyDamage(mData.damage / 2.0f, hit.point, hit.normal, this);
			}
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
			mDirectHit = null;
			mRenderer.enabled = true;
			mRigidbody.velocity = Vector3.zero;
		}

		public void PreDisable() { }
		public void PostDisable() { }
		
		#endregion
		
		#region IProjectile Implementation
		
		public ICharacter source { get { return sourceWeapon.bearer; } }
		public IWeapon sourceWeapon { get; private set; }

		public void Instantiate(IWeapon weapon, Ray ray, WeaponData data)
		{
			sourceWeapon = weapon;
			mPool = null;
			transform.position = ray.origin;

			mRigidbody.AddForce(ray.direction * mSpeed, ForceMode.Impulse);
			mData = data;
		}

		public void Instantiate(IWeapon weapon, Ray ray, WeaponData data, GameObjectPool pool)
		{
			sourceWeapon = weapon;
			mPool = pool;
			transform.position = ray.origin;

			mRigidbody.AddForce(ray.direction * mSpeed, ForceMode.Impulse);
			mData = data;
		}

		#endregion
	}
}
