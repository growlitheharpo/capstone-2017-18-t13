using System;
using System.Collections;
using System.Collections.Generic;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace FiringSquad.Gameplay
{
	/// <inheritdoc cref="IWeapon" />
	public abstract class BaseWeaponScript : MonoBehaviour, IWeapon
	{
		public enum Attachment
		{
			Scope,
			Barrel,
			Mechanism,
			Grip,
		}

		public IWeaponBearer bearer { get; set; }
		public WeaponData baseData { get { return mBaseData; } }
		public IEnumerable<WeaponPartScript> parts { get { return mCurrentAttachments.Values; } }

		[SerializeField] private WeaponData mBaseData;
		[SerializeField] private Transform mBarrelAttach;
		[SerializeField] private Transform mScopeAttach;
		[SerializeField] private Transform mMechanismAttach;
		[SerializeField] private Transform mGripAttach;
		[SerializeField] protected AudioProfile mAudioProfile;

		private Dictionary<Attachment, Transform> mAttachPoints;
		private Dictionary<Attachment, WeaponPartScript> mCurrentAttachments;
		private bool mOverrideHitscanEye;

		private GameObjectPool mProjectilePool;

		protected WeaponData mCurrentData;
		protected Transform mAimRoot;
		protected BoundProperty<int> mClipSize;
		protected BoundProperty<int> mAmountInClip;
		protected float mShotTime;

		private const float DEFAULT_SPREAD_FACTOR = 0.001f;

		protected virtual void Awake()
		{
			mAttachPoints = new Dictionary<Attachment, Transform>
			{
				{ Attachment.Scope, mScopeAttach },
				{ Attachment.Barrel, mBarrelAttach },
				{ Attachment.Mechanism, mMechanismAttach },
				{ Attachment.Grip, mGripAttach },
			};

			mCurrentAttachments = new Dictionary<Attachment, WeaponPartScript>(2);
			mCurrentData = new WeaponData(mBaseData);
			mProjectilePool = null;
		}

		protected virtual void Update()
		{
			mShotTime -= Time.deltaTime;
		}

		#region Part Attachment

		/// <inheritdoc />
		public void AttachNewPart(WeaponPartScript part)
		{
			int clipSize = mCurrentData.clipSize;

			MoveAttachmentToPoint(part);
			mCurrentAttachments[part.attachPoint] = part;

			ActivatePartEffects();

			if (part.attachPoint == Attachment.Mechanism || mCurrentData.clipSize != clipSize)
			{
				if (!mCurrentAttachments.ContainsKey(Attachment.Mechanism))
					return;

				WeaponPartScriptMechanism mech = (WeaponPartScriptMechanism)mCurrentAttachments[Attachment.Mechanism];
				CreateNewProjectilePool(mech);
				mOverrideHitscanEye = mech.overrideHitscanMethod;
				mAmountInClip.value = mClipSize.value;
			}
		}

		/// <summary>
		/// When a new mechanism is attached, we create a new pool of the projectiles it fires.
		/// </summary>
		private void CreateNewProjectilePool(WeaponPartScriptMechanism part)
		{
			StartCoroutine(CleanupDeadPool(mProjectilePool));

			GameObject newPrefab = part.projectilePrefab;
			mProjectilePool = new GameObjectPool(Mathf.CeilToInt(mCurrentData.clipSize * 1.25f), newPrefab, transform);
		}

		/// <summary>
		/// Physically move the attachment to the correct position in the game world.
		/// </summary>
		private void MoveAttachmentToPoint(WeaponPartScript part)
		{
			Attachment place = part.attachPoint;

			if (mCurrentAttachments.ContainsKey(place))
				Destroy(mCurrentAttachments[place].gameObject);

			part.transform.SetParent(mAttachPoints[place]);
			part.transform.localPosition = Vector3.zero;
			part.transform.localScale = Vector3.one;
			part.transform.localRotation = Quaternion.identity;
		}

		/// <summary>
		/// Loop through all the parts and effects and apply them to our WeaponData to be used.
		/// </summary>
		private void ActivatePartEffects()
		{
			WeaponData start = new WeaponData(mBaseData);

			Action<WeaponPartScript> apply = part =>
			{
				foreach (WeaponPartData data in part.data)
					start = new WeaponData(start, data);
			};

			var partOrder = new[] { Attachment.Mechanism, Attachment.Barrel, Attachment.Scope, Attachment.Grip };

			foreach (Attachment part in partOrder)
			{
				if (mCurrentAttachments.ContainsKey(part))
					apply(mCurrentAttachments[part]);
			}
			
			mCurrentData = start;
			mClipSize.value = mCurrentData.clipSize;
		}
		
		/// <summary>
		/// Removes all the old projectiles from previous firing mechanisms once they are no longer in use.
		/// </summary>
		private static IEnumerator CleanupDeadPool(GameObjectPool pool)
		{
			if (pool == null)
				yield break;

			while (pool.numInUse > 0)
				yield return new WaitForEndOfFrame();

			pool.Destroy();
		}

		#endregion

		#region Fire Weapon

		/// <inheritdoc />
		public void FireWeapon()
		{
			if (mShotTime > 0.0f)
				return;

			if (mAmountInClip.value <= 0)
			{
				Reload();
				return;
			}

			mShotTime = 1.0f / mCurrentData.fireRate;
			mAmountInClip.value--;

			WeaponPartScriptBarrel barrel = mCurrentAttachments[Attachment.Barrel] as WeaponPartScriptBarrel;
			int count = barrel != null ? barrel.projectileCount : 1;

			NetworkWriter writer = new NetworkWriter();
			writer.Write(count);
			var shots = new List<Ray>(count);

			for (int i = 0; i < count; i++)
			{
				shots.Add(CalculateShotDirection());
				writer.Write(shots[i]);
			}

			FireShotImmediate(shots);
			((PlayerScript)bearer).CmdReflectWeaponFire(writer.ToArray());
		}

		public void FireShotImmediate(List<Ray> shots)
		{
			OnPreFireShot();

			WeaponPartScriptBarrel barrel = mCurrentAttachments[Attachment.Barrel] as WeaponPartScriptBarrel;
			PlayShotEffect(barrel != null ? barrel.barrelTip.position : transform.position);

			foreach (Ray shot in shots)
			{
				GameObject projectile = mProjectilePool.ReleaseNewItem();
				projectile.GetComponent<IProjectile>().Instantiate(this, shot, mCurrentData, mProjectilePool);
			}

			OnPostFireShot();

			bearer.ApplyRecoil(Vector3.up, mCurrentData.recoil * Random.Range(0.75f, 1.25f));
		}
		
		/// <summary>
		/// Mini-event fired to subclasses when it's been confirmed we are going to shoot but haven't
		/// created the IProjectile yet.
		/// </summary>
		protected virtual void OnPreFireShot() { }

		/// <summary>
		/// Mini-event fired to subclasses after we've created an IProjectile and have
		/// fired the weapon.
		/// </summary>
		protected virtual void OnPostFireShot() { }

		/// <summary>
		/// To be implemented in child class. Play SFX, VFX, etc. for shot fired.
		/// </summary>
		protected abstract void PlayShotEffect(Vector3 shotOrigin);

		/// <summary>
		/// Determine the direction of the shot based on spread, etc.
		/// </summary>
		/// <returns>A new ray (origin + direction) for the next shot.</returns>
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
		
		/// <summary>
		/// Determine the aim root. First we look for one specified by the base class, then the barrel tip, then the bearer's eye.
		/// </summary>
		private Transform GetAimRoot()
		{
			if (!mOverrideHitscanEye && mAimRoot != null)
				return mAimRoot;

			WeaponPartScript barrel;
			if (mCurrentAttachments.TryGetValue(Attachment.Barrel, out barrel) && barrel is WeaponPartScriptBarrel)
				return ((WeaponPartScriptBarrel)barrel).barrelTip;

			return bearer.eye;
		}
		
		#endregion

		#region Reloading

		public void Reload()
		{
			if (mShotTime >= float.MaxValue - 1.0f)
				return;

			mShotTime = float.MaxValue; //no shooting while reloading.
			PlayReloadEffect(mCurrentData.reloadTime);
		}

		public void OnReloadComplete()
		{
			mAmountInClip.value = mClipSize.value;
			mShotTime = -1.0f;
		}

		protected abstract void PlayReloadEffect(float time);

		#endregion
	}
}
