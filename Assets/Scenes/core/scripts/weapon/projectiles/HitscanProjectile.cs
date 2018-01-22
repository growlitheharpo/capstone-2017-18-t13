using System.Collections;
using System.Linq;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class HitscanProjectile : MonoBehaviour, IProjectile
	{
		/// Inspector variables
		[SerializeField] private float mAudioWeaponType;
		[SerializeField] private AnimationCurve mFalloffCurve;

		/// Private variables
		private HitscanShootEffect mEffect;

		/// <inheritdoc />
		public ICharacter source { get { return sourceWeapon.bearer; } }

		/// <inheritdoc />
		public IWeapon sourceWeapon { get; private set; }

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mEffect = GetComponent<HitscanShootEffect>();
		}

		/// <inheritdoc />
		public bool PreSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data)
		{
			sourceWeapon = weapon;
			return false;
		}

		/// <inheritdoc />
		public void PostSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data)
		{
			sourceWeapon = weapon;

			// Check for whatever object we hit
			IDamageReceiver hitObject = null;
			Vector3 endPoint = initialDirection.origin + initialDirection.direction * 2000.0f;
			var hits = Physics.RaycastAll(initialDirection, 10000.0f, int.MaxValue, QueryTriggerInteraction.Ignore);
			if (hits.Length > 0)
			{
				// Ensure that the hits are sorted by distance
				hits = hits.OrderBy(x => Vector3.Distance(x.point, initialDirection.origin)).ToArray();

				// Check if each hit has hit a damage receiver and break if so.
				foreach (RaycastHit hit in hits)
				{
					if (hit.collider.gameObject == weapon.bearer.gameObject)
						continue;

					endPoint = hit.point;

					hitObject = hit.GetDamageReceiver();
					if (hitObject != null)
					{
						float damage = GetDamage(data, Vector3.Distance(weapon.transform.position, endPoint));
						hitObject.ApplyDamage(damage, endPoint, hit.normal, this);
					}

					break;
				}
			}

			SetupShot(endPoint);
			PositionAndVisualize(endPoint);
			StartCoroutine(WaitAndKillSelf());
		}

		/// <summary>
		/// Get the amount of damage this hitscan projectile applies based on falloff and weapon data.
		/// </summary>
		/// <returns>The amount of damage this bullet should do.</returns>
		[Server]
		private float GetDamage(WeaponData data, float distance)
		{
			float distancePercent = Mathf.Clamp(distance / data.damageFalloffDistance, 0.0f, 1.0f);
			return mFalloffCurve.Evaluate(distancePercent) * data.damage;
		}

		/// <summary>
		/// Notify all local clients to create a shot locally so that it's visual effect matches what happened on the server.
		/// </summary>
		/// <param name="endPoint">The world postiion where this bullet hit.</param>
		[Server]
		private void SetupShot(Vector3 endPoint)
		{
			RpcCreateShot(source.netId, endPoint);
		}

		/// <summary>
		/// Reflect a visual shot that occurred on the server so that the local visualizer matches what happened.
		/// </summary>
		/// <param name="s">The network ID of the source of this shot.</param>
		/// <param name="endPoint">The world position where this bullet hit.</param>
		[ClientRpc]
		private void RpcCreateShot(NetworkInstanceId s, Vector3 endPoint) // TODO: Does this need to be an RPC?? Poorly optimized
		{
			GameObject theSource = ClientScene.FindLocalObject(s);
			if (theSource == null)
				return;

			sourceWeapon = theSource.GetComponent<IWeaponBearer>().weapon;
			PositionAndVisualize(endPoint);
		}

		/// <summary>
		/// Move this GameObject and notify our visualizer how it should display itself.
		/// </summary>
		/// <param name="endPoint"></param>
		private void PositionAndVisualize(Vector3 endPoint)
		{
			transform.position = sourceWeapon.currentParts.barrel != null ? sourceWeapon.currentParts.barrel.barrelTip.position : sourceWeapon.transform.position;
			transform.forward = sourceWeapon.transform.forward;

			mEffect.PlayEffect(endPoint);
		}

		/// <summary>
		/// Wait until our visualizer has finished and then destroy this object on the network.
		/// </summary>
		private IEnumerator WaitAndKillSelf()
		{
			yield return new WaitForSeconds(0.25f);
			NetworkServer.Destroy(gameObject);
		}
	}
}
