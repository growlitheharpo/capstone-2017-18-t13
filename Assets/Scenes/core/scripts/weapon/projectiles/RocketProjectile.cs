using System.Collections;
using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class RocketProjectile : BaseProjectileScript
	{
		/// Inspector variables
		[SerializeField] private ParticleSystem mHitParticles;
		[SerializeField] private float mSpeed;
		[SerializeField] private float mSplashDamageRadius;

		/// Private variables
		private Transform mDirectHit;
		private Rigidbody mRigidbody;
		private GameObject mView;
		private WeaponData mData; // server-only

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mRigidbody = GetComponent<Rigidbody>();
			mView = transform.Find("View").gameObject;
		}

		public override void PreSpawnInitialize(IWeapon weapon, Ray ray, WeaponData data)
		{
			base.PreSpawnInitialize(weapon, ray, data);

			Transform barrelTip = weapon.currentParts.barrel != null ? weapon.currentParts.barrel.barrelTip : weapon.transform;
			transform.position = barrelTip.position + barrelTip.forward;
			transform.forward = barrelTip.forward;

			mRigidbody.AddForce(ray.direction * mSpeed, ForceMode.Impulse);
			mData = data;
		}

		/// <summary>
		/// Unity's OnCollisionEnter callback.
		/// Set to only trigger on the server for hit detection.
		/// </summary>
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

		/// <summary>
		/// Unity's Update function.
		/// Rotate our projectile to face towards our velocity
		/// </summary>
		private void Update()
		{
			transform.right = mRigidbody.velocity;
		}

		/// <summary>
		/// Apply the splash damage for this projectile.
		/// </summary>
		[Server]
		private void ApplySplashDamage()
		{
			var colliders = Physics.OverlapSphere(transform.position, mSplashDamageRadius);
			foreach (Collider col in colliders)
			{
				if (col.transform == mDirectHit)
					continue;

				IDamageReceiver c = col.GetComponentInParent<IDamageReceiver>();
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

		/// <summary>
		/// Activate the explode effect across all clients.
		/// TODO: This probably shouldn't be an RPC!!
		/// </summary>
		[ClientRpc]
		private void RpcActivateExplodeEffect() // TODO: Does this really need to be an RPC?
		{
			StartCoroutine(DisplayExplodeParticles());
		}

		/// <summary>
		/// Show the particles and destroy our normal view.
		/// </summary>
		/// <returns></returns>
		private IEnumerator DisplayExplodeParticles()
		{
			mView.SetActive(false);
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

		/// <summary>
		/// Cleanup this projectile after the explode has finished.
		/// </summary>
		[Server]
		private void OnEffectComplete()
		{
			NetworkServer.Destroy(gameObject);
			Destroy(gameObject);
		}
	}
}
