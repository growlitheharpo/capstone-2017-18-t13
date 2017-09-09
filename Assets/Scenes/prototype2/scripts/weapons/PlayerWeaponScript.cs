using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Prototype2
{
	/// <summary>
	/// Component strictly for handling the player's weapon
	/// and how it behaves.
	/// </summary>
	/// <inheritdoc />
	public class PlayerWeaponScript : MonoBehaviour, IWeapon
	{
		public enum Attachment
		{
			Scope,
			Barrel,
		}

		[SerializeField] private ParticleSystem mShotParticles;
		[SerializeField] private WeaponData mBaseData;
		[SerializeField] private Transform mBarrelAttach;
		[SerializeField] private Transform mScopeAttach;

		private Transform mMainCameraRef;
		
		private Dictionary<Attachment, Transform> mAttachPoints;
		private Dictionary<Attachment, WeaponPartScript> mCurrentAttachments;
		private WeaponData mCurrentData;
		private Vector3 mCameraOffset;
		private float mCooldown;
		
		/*public ICharacter bearer { get; private set; }
		public WeaponData baseData { get; private set; }
		public IEnumerable<WeaponPartScript> parts { get; private set; }*/

		private const float CAMERA_FOLLOW_FACTOR = 10.0f;
		private const float DEFAULT_SPREAD_FACTOR = 0.001f;

		private void Awake()
		{
			mAttachPoints = new Dictionary<Attachment, Transform>
			{
				{ Attachment.Scope, mScopeAttach },
				{ Attachment.Barrel, mBarrelAttach },
			};

			mCurrentAttachments = new Dictionary<Attachment, WeaponPartScript>(2);
			mCurrentData = new WeaponData(mBaseData);
		}

		private void Start()
		{
			mMainCameraRef = Camera.main.transform;
			mCameraOffset = mMainCameraRef.InverseTransformPoint(transform.position);
			EventManager.OnConfirmPartAttach += AttachNewPart;
		}

		private void OnDestroy()
		{
			EventManager.OnConfirmPartAttach -= AttachNewPart;
		}


		/// <summary>
		/// Attach a new part to the weapon in the given attachment slot.
		/// </summary>
		/// <param name="part">The new WeaponPartScript to be applied.</param>
		private void AttachNewPart(WeaponPartScript part)
		{
			Attachment place = part.attachPoint;

			if (mCurrentAttachments.ContainsKey(place))
				Destroy(mCurrentAttachments[place].gameObject);

			part.transform.SetParent(mAttachPoints[place]);
			part.transform.localPosition = Vector3.zero;
			part.transform.localRotation = Quaternion.identity;

			mCurrentAttachments[place] = part;
			ActivatePartEffects();
		}

		/// <summary>
		/// Loop through all parts and all effects per part and apply their modifiers.
		/// </summary>
		private void ActivatePartEffects()
		{
			WeaponData start = new WeaponData(mBaseData);
			foreach (WeaponPartScript part in mCurrentAttachments.Values)
			{
				foreach (WeaponPartData effect in part.data)
					start = new WeaponData(start, effect);
			}

			mCurrentData = start;
		}

		void IWeapon.AttachNewPart(WeaponPartScript part)
		{
			AttachNewPart(part);
		}

		/// <summary>
		/// Begin the fire procedure for the weapon based on whatever its current modifications are.
		/// </summary>
		public void FireWeapon()
		{
			if (mCooldown > 0.0f)
				return;

			PlayShotEffect();

			Ray shot = CalculateShotDirection();

			FireShot(shot);
		}

		/// <summary>
		/// Play any SFX and VFX associated with the weapon based on its current mods.
		/// </summary>
		private void PlayShotEffect()
		{
			mShotParticles.Stop();
			mShotParticles.time = 0.0f;
			mShotParticles.Play();
		}

		/// <summary>
		/// Determine the direction our shot will travel based on the camera, current spread variables, etc.
		/// </summary>
		private Ray CalculateShotDirection()
		{
			mCooldown = 1.0f / mCurrentData.mFireRate;

			float spreadFactor = DEFAULT_SPREAD_FACTOR * mCurrentData.mDefaultSpread;
			Vector3 randomness = new Vector3(
				Random.Range(-spreadFactor, spreadFactor),
				Random.Range(-spreadFactor, spreadFactor),
				Random.Range(-spreadFactor, spreadFactor));

			return new Ray(mMainCameraRef.position, mMainCameraRef.forward + randomness);
		}

		/// <summary>
		/// Calculation for a hitscan weapon.
		/// TODO: Refactor this into a projectile class which can be hot-swapped with non-hitscan projectiles.
		/// </summary>
		private void FireShot(Ray ray)
		{
			// Draw a debug line
			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2000.0f, Color.red, mCooldown + 0.2f);

			// See if we hit anything
			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit, 2500.0f))
				return;

			// Try to apply damage to it if we did
			IDamageReceiver component = hit.transform.GetComponent<IDamageReceiver>() ?? hit.transform.parent.GetComponent<IDamageReceiver>();
			if (component != null)
				component.ApplyDamage(mCurrentData.mDefaultDamage, hit.point);
		}

		private void Update()
		{
			FollowCamera();
			mCooldown -= Time.deltaTime;
		}

		/// <summary>
		/// Lerp our position and rotation to match the camera.
		/// </summary>
		private void FollowCamera()
		{
			Vector3 location = transform.position;
			Vector3 targetLocation = mMainCameraRef.TransformPoint(mCameraOffset);

			Quaternion rotation = transform.rotation;
			Quaternion targetRotation = mMainCameraRef.rotation;

			transform.position = Vector3.Lerp(location, targetLocation, Time.deltaTime * CAMERA_FOLLOW_FACTOR);
			transform.rotation = Quaternion.Lerp(rotation, targetRotation, Time.deltaTime * CAMERA_FOLLOW_FACTOR);
		}
	}
}
