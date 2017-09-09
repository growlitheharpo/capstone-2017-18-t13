using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype2
{
	/// <inheritdoc cref="IWeapon" />
	public abstract class BaseWeaponScript : MonoBehaviour, IWeapon
	{
		public enum Attachment
		{
			Scope,
			Barrel,
			Mechanism,
		}

		public ICharacter bearer { get; set; }
		public WeaponData baseData { get { return mBaseData; } }
		public IEnumerable<WeaponPartScript> parts { get { return mCurrentAttachments.Values; } }

		[SerializeField] private WeaponData mBaseData;
		[SerializeField] private Transform mBarrelAttach;
		[SerializeField] private Transform mScopeAttach;
		[SerializeField] private Transform mMechanismAttach;

		private Dictionary<Attachment, Transform> mAttachPoints;
		private Dictionary<Attachment, WeaponPartScript> mCurrentAttachments;
		private WeaponData mCurrentData;
		private float mShotTime;

		private GameObjectPool mProjectilePool;

		protected Transform mAimRoot;

		private const float DEFAULT_SPREAD_FACTOR = 0.001f;

		protected virtual void Awake()
		{
			mAttachPoints = new Dictionary<Attachment, Transform>
			{
				{ Attachment.Scope, mScopeAttach },
				{ Attachment.Barrel, mBarrelAttach },
				{ Attachment.Mechanism, mMechanismAttach },
			};

			mCurrentAttachments = new Dictionary<Attachment, WeaponPartScript>(2);
			mCurrentData = new WeaponData(mBaseData);
			mProjectilePool = null;
		}

		protected virtual void Update()
		{
			mShotTime -= Time.deltaTime;
		}

		public void AttachNewPart(WeaponPartScript part)
		{
			MoveAttachmentToPoint(part);
			mCurrentAttachments[part.attachPoint] = part;

			if (part.attachPoint == Attachment.Mechanism)
				CreateNewProjectilePool((WeaponPartScriptMechanism)part);

			ActivatePartEffects();
		}

		private void CreateNewProjectilePool(WeaponPartScriptMechanism part)
		{
			StartCoroutine(CleanupDeadPool(mProjectilePool));

			GameObject newPrefab = part.projectilePrefab;
			mProjectilePool = new GameObjectPool(25, newPrefab, transform);
		}

		private void MoveAttachmentToPoint(WeaponPartScript part)
		{
			Attachment place = part.attachPoint;

			if (mCurrentAttachments.ContainsKey(place))
				Destroy(mCurrentAttachments[place].gameObject);

			part.transform.SetParent(mAttachPoints[place]);
			part.transform.localPosition = Vector3.zero;
			part.transform.localRotation = Quaternion.identity;
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
			if (mShotTime > 0.0f)
				return;

			mShotTime = 1.0f / mCurrentData.fireRate;

			PlayShotEffect();
			Ray shot = CalculateShotDirection();

			GameObject projectile = mProjectilePool.ReleaseNewItem();
			projectile.GetComponent<IProjectile>().Instantiate(shot, mCurrentData, mProjectilePool);
		}

		protected abstract void PlayShotEffect();

		protected virtual Ray CalculateShotDirection()
		{
			float spreadFactor = DEFAULT_SPREAD_FACTOR * mCurrentData.spread;
			Vector3 randomness = new Vector3(
				Random.Range(-spreadFactor, spreadFactor),
				Random.Range(-spreadFactor, spreadFactor),
				Random.Range(-spreadFactor, spreadFactor));

			Transform root = GetAimRoot();
			return new Ray(root.position, root.forward + randomness);
		}
		
		private Transform GetAimRoot()
		{
			if (mAimRoot != null)
				return mAimRoot;

			WeaponPartScript barrel;
			if (mCurrentAttachments.TryGetValue(Attachment.Barrel, out barrel) && barrel is WeaponPartScriptBarrel)
				return ((WeaponPartScriptBarrel)barrel).barrelTip;

			return bearer.eye;
		}

		private static IEnumerator CleanupDeadPool(GameObjectPool pool)
		{
			if (pool == null)
				yield break;

			while (pool.numInUse > 0)
				yield return new WaitForEndOfFrame();

			pool.Destroy();
		}
	}
}
