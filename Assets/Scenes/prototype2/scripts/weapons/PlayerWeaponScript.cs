using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Prototype2
{
	public class PlayerWeaponScript : MonoBehaviour
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
		}

		public void AttachNewPart(Attachment place, WeaponPartScript part)
		{
			if (mCurrentAttachments.ContainsKey(place))
				Destroy(mCurrentAttachments[place].gameObject);

			part.transform.SetParent(mAttachPoints[place]);
			part.transform.localPosition = Vector3.zero;
			part.transform.localRotation = Quaternion.identity;

			mCurrentAttachments[place] = part;
			ActivatePartEffects();
		}

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

		public void FireWeapon()
		{
			if (mCooldown > 0.0f)
				return;

			Transform playerEye = Camera.main.transform;
			mCooldown = 1.0f / mCurrentData.mFireRate;

			float spreadFactor = DEFAULT_SPREAD_FACTOR * mCurrentData.mDefaultSpread;
			Vector3 randomness = new Vector3(Random.Range(-spreadFactor, spreadFactor), Random.Range(-spreadFactor, spreadFactor), Random.Range(-spreadFactor, spreadFactor));
			Ray ray = new Ray(playerEye.position, playerEye.forward + randomness);

			Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2000.0f, Color.red, mCooldown + 0.2f);

			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit, 2500.0f))
				return;

			mShotParticles.Stop();
			mShotParticles.time = 0.0f;
			mShotParticles.Play();

			IDamageReceiver component = hit.transform.parent.GetComponent<IDamageReceiver>();
			if (component != null)
				component.ApplyDamage(mCurrentData.mDefaultDamage, hit.point);
		}

		private void Update()
		{
			FollowCamera();
			mCooldown -= Time.deltaTime;
		}

		private void FollowCamera()
		{
			Vector3 location = transform.position;
			Vector3 targetLocation = mMainCameraRef.TransformPoint(mCameraOffset);

			transform.position = Vector3.Lerp(location, targetLocation, Time.deltaTime * CAMERA_FOLLOW_FACTOR);
			transform.rotation = Quaternion.Lerp(transform.rotation, mMainCameraRef.rotation, Time.deltaTime * CAMERA_FOLLOW_FACTOR);
		}
	}
}
