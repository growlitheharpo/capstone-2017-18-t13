using System;
using System.Collections;
using System.Collections.Generic;
using FiringSquad.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FiringSquad.Gameplay
{
	/// <inheritdoc cref="IWeapon" />
	public abstract class BaseWeaponScript : MonoBehaviour, IWeapon
	{
		public void FireShotImmediate(List<Ray> shotDirections)
		{
			throw new NotImplementedException();
		}

		public static class DebugHelper
		{
			public static WeaponData GetWeaponData(BaseWeaponScript p)
			{
				return new WeaponData(p.mCurrentData);
			}

			public static Dictionary<Attachment, WeaponPartScript> GetAttachments(BaseWeaponScript p)
			{
				return new Dictionary<Attachment, WeaponPartScript>(p.mCurrentAttachments);
			}

			public static Transform GetWeaponAimRoot(BaseWeaponScript p, bool forceBarrel = false)
			{
				if (!forceBarrel)
					return p.GetAimRoot();

				WeaponPartScript barrel;
				if (p.mCurrentAttachments.TryGetValue(Attachment.Barrel, out barrel) && barrel is WeaponPartScriptBarrel)
					return ((WeaponPartScriptBarrel)barrel).barrelTip;

				return p.bearer.eye;
			}

			public static float GetCurrentDispersion(BaseWeaponScript p)
			{
				return p.GetCurrentDispersionFactor();
			}
		}

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

		private List<float> mRecentShotTimes;
		private float timePerShot { get { return 1.0f / mCurrentData.fireRate; } }
		private bool mReloading;
		private int mShotsSinceRelease;

		protected virtual void Awake()
		{
			mRecentShotTimes = new List<float> { -1.0f };

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

			float count = mCurrentData.clipSize * 1.25f;

			WeaponPartScriptBarrel barrel = mCurrentAttachments[Attachment.Barrel] as WeaponPartScriptBarrel;
			if (barrel != null)
				count *= barrel.projectileCount;

			GameObject newPrefab = part.projectilePrefab;
			mProjectilePool = new GameObjectPool(Mathf.CeilToInt(count), newPrefab, transform);
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

		/*
		public void FireWeaponDown()
		{
			// for now, we'll ignore this.
		}

		public void FireWeaponHold()
		{
			DoWeaponFire();
		}

		public void FireWeaponUp()
		{
			mShotsSinceRelease = 0;
		}

		private void DoWeaponFire()
		{
			CleanupRecentShots();
			WeaponPartScriptBarrel barrel = mCurrentAttachments[Attachment.Barrel] as WeaponPartScriptBarrel;

			float lastShotTime = mRecentShotTimes.Count >= 1 ? mRecentShotTimes[mRecentShotTimes.Count - 1] : -1.0f;
			if (mReloading || Time.time - lastShotTime < timePerShot)
				return;

			if (barrel != null && (barrel.shotsPerClick > 0 && mShotsSinceRelease >= barrel.shotsPerClick))
				return;

			if (mAmountInClip.value <= 0)
			{
				Reload();
				return;
			}

			int count = barrel != null ? barrel.projectileCount : 1;

			var shots = new List<Ray>(count);
			for (int i = 0; i < count; i++)
				shots.Add(CalculateShotDirection(i == 0));

			mRecentShotTimes.Add(Time.time);
			mAmountInClip.value--;
			mShotsSinceRelease++;

			FireShotImmediate(shots);
			((PlayerScript)bearer).ReflectWeaponFire(shots);
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
		/// Determine the direction of the shot based on minimumDispersion, etc.
		/// </summary>
		/// <returns>A new ray (origin + direction) for the next shot.</returns>
		protected virtual Ray CalculateShotDirection(bool firstShot)
		{
			float dispersionFactor = GetCurrentDispersionFactor(forceNotZero: !firstShot);
			Vector3 randomness = Random.insideUnitSphere * dispersionFactor;

			Transform root = GetAimRoot();
			return new Ray(root.position, root.forward + randomness);
		}*/

		private float GetCurrentDispersionFactor(bool forceNotZero = false)
		{
			float percentage = 0.0f;
			float inverseFireRate = 1.0f / mCurrentData.fireRate;

			foreach (float shot in mRecentShotTimes)
			{
				float timeSinceShot = Time.time - shot;
				if (timeSinceShot > inverseFireRate * 2.0f)
					continue;

				float p = Mathf.Pow(Mathf.Clamp(inverseFireRate / timeSinceShot, 0.0f, 1.0f), 2);
				percentage += p * mCurrentData.dispersionRamp;
			}

			if (!forceNotZero && percentage <= 0.005f)
				return 0.0f;

			return Mathf.Lerp(mCurrentData.minimumDispersion, mCurrentData.maximumDispersion, percentage);
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
			if (mReloading)
				return;

			mReloading = true;
			PlayReloadEffect(mCurrentData.reloadTime);
		}

		public void OnReloadComplete()
		{
			mAmountInClip.value = mClipSize.value;
			mReloading = false;
		}

		protected abstract void PlayReloadEffect(float time);

		#endregion

		public float GetCurrentRecoil()
		{
			float value = 0.0f;
			foreach (float v in mRecentShotTimes)
			{
				float timeSinceShot = Time.time - v;
				float percent = Mathf.Clamp(timeSinceShot / mCurrentData.recoilTime, 0.0f, 1.0f);
				float sample = mCurrentData.recoilCurve.Evaluate(percent);
				value += sample;
			}

			return value * mCurrentData.recoilAmount;
		}

		private void CleanupRecentShots()
		{
			float inverseFireRate = (1.0f / mCurrentData.fireRate) * 10.0f;

			for (int i = 0; i < mRecentShotTimes.Count; i++)
			{
				float timeSinceShot = Time.time - mRecentShotTimes[i];

				if (timeSinceShot < inverseFireRate)
					continue;

				mRecentShotTimes.RemoveAt(i);
				--i;
			}
		}
	}
}
