using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FiringSquad.Data
{
	[Serializable]
	public struct WeaponData
	{
		[FormerlySerializedAs("mSpread")]
		[SerializeField] private float mMinimumDispersion;
		[SerializeField] private float mMaximumDispersion;
		[SerializeField] private float mDispersionRamp;

		[FormerlySerializedAs("mRecoil")] [SerializeField] private float mRecoilAmount;
		[SerializeField] private float mRecoilTime;
		[SerializeField] private AnimationCurve mRecoilCurve;

		[SerializeField] private float mDamage;
		[SerializeField] private float mDamageFalloffDistance;
		[SerializeField] private float mFireRate;
		[SerializeField] private float mReloadTime;
		[SerializeField] private int mClipSize;

		public float minimumDispersion { get { return mMinimumDispersion; } }
		public float maximumDispersion { get { return mMaximumDispersion; } }
		public float dispersionRamp { get { return mDispersionRamp; } }

		public float recoilAmount { get { return mRecoilAmount; } }
		public float recoilTime { get { return mRecoilTime; }}
		public AnimationCurve recoilCurve { get { return mRecoilCurve; } }

		public float damage { get { return mDamage; } }
		public float damageFalloffDistance { get { return mDamageFalloffDistance; } }
		public float fireRate { get { return mFireRate; } }
		public float reloadTime { get { return mReloadTime; } }
		public int clipSize { get { return mClipSize; } }

		public WeaponData(WeaponData other)
		{
			mMinimumDispersion = other.mMinimumDispersion;
			mMaximumDispersion = other.mMaximumDispersion;
			mDispersionRamp = other.mDispersionRamp;
			mRecoilAmount = other.mRecoilAmount;
			mRecoilTime = other.mRecoilTime;
			mRecoilCurve = new AnimationCurve(other.mRecoilCurve.keys);
			mDamage = other.mDamage;
			mDamageFalloffDistance = other.mDamageFalloffDistance;
			mFireRate = other.mFireRate;
			mClipSize = other.mClipSize;
			mReloadTime = other.mReloadTime;
		}

		public WeaponData(WeaponData other, WeaponPartData data)
		{
			mDamage = data.damageModifier.Apply(other.mDamage);
			mDamageFalloffDistance = data.damageFalloffDistanceModifier.Apply(other.mDamageFalloffDistance);
			mMinimumDispersion = data.minDispersionModifier.Apply(other.mMinimumDispersion);
			mMaximumDispersion = data.maxDispersionModifier.Apply(other.mMaximumDispersion);
			mDispersionRamp = data.dispersionRampModifier.Apply(other.mDispersionRamp);
			mRecoilAmount = data.recoilAmountModifier.Apply(other.mRecoilAmount);
			mRecoilTime = data.recoilTimeModifier.Apply(other.mRecoilTime);
			mRecoilCurve = new AnimationCurve(other.mRecoilCurve.keys);
			mClipSize = data.clipModifier.Apply(other.mClipSize);
			mFireRate = data.fireRateModifier.Apply(other.mFireRate);
			mReloadTime = data.reloadModifier.Apply(other.mReloadTime);
		}

		public override string ToString()
		{
			return string.Format("Spread: {0}, Damage: {1}, FireRate: {2} ClipSize: {3}, Recoil: {4}, Reload: {5}", mMinimumDispersion, mDamage, mFireRate, mClipSize, mRecoilAmount, mReloadTime);
		}
	}
}
