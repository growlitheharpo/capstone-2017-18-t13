using System.Collections;
using System.Linq;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	public class HitscanProjectile : BaseProjectileScript
	{
		[SerializeField] private AnimationCurve mFalloffCurve;

		private HitscanShootEffect mEffect;

		private void Awake()
		{
			mEffect = GetComponent<HitscanShootEffect>();
		}

		[Server]
		public override void PostSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data)
		{
			base.PostSpawnInitialize(weapon, initialDirection, data);

			Vector3 endPoint = initialDirection.origin + initialDirection.direction * 2000.0f;
			var hits = Physics.RaycastAll(initialDirection, 10000.0f, int.MaxValue, QueryTriggerInteraction.Ignore);
			if (hits.Length > 0)
			{
				hits = hits.OrderBy(x => Vector3.Distance(x.point, initialDirection.origin)).ToArray();
				foreach (RaycastHit hit in hits)
				{
					if (hit.collider.gameObject == weapon.bearer.gameObject)
						continue;

					endPoint = hit.point;

					IDamageReceiver hitObject = hit.GetDamageReceiver();
					if (hitObject != null)
					{
						float damage = GetDamage(data, Vector3.Distance(weapon.transform.position, endPoint));
						hitObject.ApplyDamage(damage, endPoint, hit.normal, this);
					}

					NetworkBehaviour netObject = hitObject as NetworkBehaviour;
					RpcPlaySound(netObject == null ? NetworkInstanceId.Invalid : netObject.netId, endPoint);
					break;

				}
			}

			SetupShot(endPoint);
			PositionAndVisualize(endPoint);
			StartCoroutine(WaitAndKillSelf());
		}

		[Server]
		private float GetDamage(WeaponData data, float distance)
		{
			float distancePercent = Mathf.Clamp(distance / data.damageFalloffDistance, 0.0f, 1.0f);
			return mFalloffCurve.Evaluate(distancePercent) * data.damage;
		}

		[Server]
		private void SetupShot(Vector3 endPoint)
		{
			RpcCreateShot(source.netId, endPoint);
		}

		[ClientRpc]
		private void RpcCreateShot(NetworkInstanceId s, Vector3 endPoint) // TODO: Does this need to be an RPC?? Poorly optimized
		{
			GameObject theSource = ClientScene.FindLocalObject(s);
			if (theSource == null)
				return;

			sourceWeapon = theSource.GetComponent<IWeaponBearer>().weapon;
			PositionAndVisualize(endPoint);
		}

		private void PositionAndVisualize(Vector3 endPoint)
		{
			transform.position = sourceWeapon.currentParts.barrel.barrelTip.position;
			transform.forward = sourceWeapon.transform.forward;

			mEffect.PlayEffect(endPoint);
		}

		private IEnumerator WaitAndKillSelf()
		{
			yield return new WaitForSeconds(0.25f);
			NetworkServer.Destroy(gameObject);
		}
	}
}
