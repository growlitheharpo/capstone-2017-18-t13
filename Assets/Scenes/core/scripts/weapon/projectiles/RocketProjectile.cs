using System.Collections;
using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	public class RocketProjectile : BaseProjectileScript
	{
		[SerializeField] private ParticleSystem mHitParticles;
		[SerializeField] private float mSpeed;
		[SerializeField] private float mSplashDamageRadius;

		private Transform mDirectHit;
		private Rigidbody mRigidbody;
		private Renderer mRenderer;
		private WeaponData mData; // server-only

		private void Awake()
		{
			mRigidbody = GetComponent<Rigidbody>();
			mRenderer = GetComponent<Renderer>();
		}

		public override void PreSpawnInitialize(IWeapon weapon, Ray ray, WeaponData data)
		{
			base.PreSpawnInitialize(weapon, ray, data);

			Transform barrelTip = weapon.currentParts.barrel.barrelTip;
			transform.position = barrelTip.position + barrelTip.forward;

			mRigidbody.AddForce(ray.direction * mSpeed, ForceMode.Impulse);
			mData = data;
		}

		[ServerCallback]
		private void OnCollisionEnter(Collision hit)
		{
			if (hit.transform == source.gameObject.transform)
				return;

			IDamageReceiver component = hit.GetDamageReceiver();
			if (component != null)
			{
				component.ApplyDamage(mData.damage, hit.contacts[0].point, hit.contacts[0].normal, this);
				mDirectHit = hit.transform;
			}

			NetworkBehaviour netObject = component as NetworkBehaviour;
			RpcPlaySound(netObject == null ? NetworkInstanceId.Invalid : netId, hit.contacts[0].point);

			ApplySplashDamage();
			RpcActivateExplodeEffect();
			StartCoroutine(DisplayExplodeParticles());
		}

		[Server]
		private void ApplySplashDamage()
		{
			var colliders = Physics.OverlapSphere(transform.position, mSplashDamageRadius);
			foreach (Collider col in colliders)
			{
				if (col.transform == mDirectHit)
					continue;

				IDamageReceiver c = col.GetComponentUpwards<IDamageReceiver>();
				if (c == null)
					continue;

				Ray ray = new Ray(transform.position, col.transform.position - transform.position);
				RaycastHit hitInfo;
				Physics.Raycast(ray, out hitInfo, mSplashDamageRadius * 1.5f, int.MaxValue, QueryTriggerInteraction.Ignore);

				if (hitInfo.collider != col)
					continue;

				c.ApplyDamage(mData.damage * 0.5f, hitInfo.point, hitInfo.normal, this);
			}
		}

		[ClientRpc]
		private void RpcActivateExplodeEffect()
		{
			StartCoroutine(DisplayExplodeParticles());
		}

		private IEnumerator DisplayExplodeParticles()
		{
			mRenderer.enabled = false;
			mRigidbody.velocity = Vector3.zero;
			mRigidbody.angularVelocity = Vector3.zero;

			mHitParticles.transform.SetParent(null);
			mHitParticles.transform.localScale = Vector3.one;
			mHitParticles.Play();
			yield return new WaitForParticles(mHitParticles);

			mHitParticles.transform.SetParent(transform);
			mHitParticles.transform.ResetLocalValues();

			if (!isServer)
				yield break;

			yield return new WaitForSeconds(0.1f);
			OnEffectComplete();
		}

		[Server]
		private void OnEffectComplete()
		{
			NetworkServer.Destroy(gameObject);
			Destroy(gameObject);
		}

		/*
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
			// TODO: This must be done server-side
			IDamageReceiver component = hit.GetDamageReceiver();
			if (component != null)
			{
				//component.ApplyDamage(mData.damage, hit.contacts[0].point, hit.contacts[0].normal, this);
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
				// TODO: This must be done server-side
				/*if (component != null)
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
		public void Initialize(IWeapon weapon, Ray initialDirection, WeaponData data)
		{
			throw new System.NotImplementedException();
		}

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

		#endregion*/
	}
}
