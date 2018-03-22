using System.Collections;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class HitscanProjectile : MonoBehaviour, IProjectile
	{
		/// <summary>
		/// Packages the data necessary to display a local effect for a projectile.
		/// </summary>
		public class HitscanMessage : MessageBase
		{
			/// <summary>
			/// The final world position hit.
			/// </summary>
			public Vector3 mEnd;
			
			/// <summary>
			/// The network ID of the source player of this projectile.
			/// </summary>
			public NetworkInstanceId mSource;

			/// <summary>
			/// The Network ID (or INVALID) for the hit object to determine the local sound.
			/// </summary>
			public NetworkInstanceId mHitObject;

			/// <summary>
			/// The mechanism ID that fires this type of projectile.
			/// </summary>
			public byte mMechanismId;
		}
		public const short HITSCAN_MESSAGE_TYPE = MsgType.Highest + 8;

		/// Inspector variables
		[SerializeField] private float mAudioWeaponType;
		[SerializeField] private AnimationCurve mFalloffCurve;
		[SerializeField] private int mMaxPenetrationObjects;
		[SerializeField] private int mMaxHitObjects = 1;

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

				int wallPenetrationCount = mMaxPenetrationObjects,
					hitObjectCount = mMaxHitObjects;

				// Check if each hit has hit a damage receiver and break if so.
				foreach (RaycastHit hit in hits)
				{
					endPoint = hit.point;
					float damage = GetDamage(data, Vector3.Distance(weapon.transform.position, endPoint));
					bool wasHeadshot = false;

					IDamageZone hitZone = hit.GetDamageZone();
					if (hitZone != null)
					{
						hitObject = hitZone.receiver;
						damage = hitZone.damageModification.Apply(damage);
						wasHeadshot = hitZone.isHeadshot;
					}
					else
						hitObject = hit.GetDamageReceiver();

					if (hitObject != null && hitObject != weapon.bearer)
					{
						hitObject.ApplyDamage(damage, endPoint, hit.normal, this, wasHeadshot);
						--hitObjectCount;
					}
					else
						--wallPenetrationCount;

					if (hitObjectCount <= 0 || wallPenetrationCount <= 0)
						break;
				}
			}
			
			// Prepare the network message for this object
			NetworkBehaviour netObject = hitObject as NetworkBehaviour;
			HitscanMessage msg = new HitscanMessage
			{
				mSource = source.netId,
				mEnd = endPoint,
				mHitObject = netObject == null ? NetworkInstanceId.Invalid : netObject.netId,
				mMechanismId = weapon.currentParts.mechanism != null ? weapon.currentParts.mechanism.partId : (byte)0
			};

			NetworkServer.SendToAll(HITSCAN_MESSAGE_TYPE, msg);

			// TODO: Instantiating and immediately destroying is wasteful, is there a better way?
			// TODO: This means that, for the host, each bullet is spawned and destroyed twice. Better than the network waste before, but not good.
			Destroy(gameObject); // We don't actually need this object on the server, so destroy it.
		}

		/// <summary>
		/// Get the amount of damage this hitscan projectile applies based on falloff and weapon data.
		/// </summary>
		/// <returns>The amount of damage this bullet should do.</returns>
		private float GetDamage(WeaponData data, float distance)
		{
			float distancePercent = Mathf.Clamp(distance / data.damageFalloffDistance, 0.0f, 1.0f);
			return mFalloffCurve.Evaluate(distancePercent) * data.damage;
		}

		/// <summary>
		/// Move this GameObject and notify our visualizer how it should display itself.
		/// </summary>
		/// <param name="weapon">The weapon of the player firing this shot.</param>
		/// <param name="endPoint"></param>
		/// <param name="audioEvent">Which audio event type to use.</param>
		public void PositionAndVisualize(IWeapon weapon, Vector3 endPoint, AudioEvent audioEvent)
		{
			sourceWeapon = weapon;

			transform.position = sourceWeapon.currentParts.barrel != null ? sourceWeapon.currentParts.barrel.barrelTip.position : sourceWeapon.transform.position;
			transform.forward = sourceWeapon.transform.forward;

			ServiceLocator.Get<IAudioManager>()
				.CreateSound(audioEvent, transform, endPoint, Space.World, false)
				.SetParameter("WeaponType", mAudioWeaponType)
				.Start();

			mEffect.PlayEffect(endPoint);
		}
	}
}
