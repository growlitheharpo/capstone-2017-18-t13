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
			transform.forward = barrelTip.forward;

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

		private void Update()
		{
			transform.right = mRigidbody.velocity;
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
		private void RpcActivateExplodeEffect() // TODO: Does this really need to be an RPC?
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
	}
}
