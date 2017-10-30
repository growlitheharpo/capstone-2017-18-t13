﻿using System.Collections;
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

			Vector3 endPoint;
			RaycastHit hitInfo;
			if (Physics.Raycast(initialDirection, out hitInfo, 10000f, int.MaxValue, QueryTriggerInteraction.Ignore))
			{
				endPoint = hitInfo.point;

				IDamageReceiver hitObject = hitInfo.GetDamageReceiver();
				if (hitObject != null)
				{
					float damage = GetDamage(data, Vector3.Distance(weapon.transform.position, endPoint));
					hitObject.ApplyDamage(damage, endPoint, hitInfo.normal, this);
				}

				NetworkBehaviour netObject = hitObject as NetworkBehaviour;
				RpcPlaySound(netObject == null ? NetworkInstanceId.Invalid : netObject.netId, endPoint);
			}
			else
				endPoint = initialDirection.origin + initialDirection.direction * 5000.0f;

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
		private void RpcCreateShot(NetworkInstanceId s, Vector3 endPoint)
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
