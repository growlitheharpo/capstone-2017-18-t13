using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FiringSquad.Data
{
	[Serializable]
	public struct WeaponData
	{
		[FormerlySerializedAs("mSpread")][SerializeField] private float mMinimumDispersion;
		[SerializeField] private float mMaximumDispersion;
		[SerializeField] private float mDispersionRamp;
		[SerializeField] private float mDamage;
		[SerializeField] private float mFireRate;
		[SerializeField] private float mRecoil;
		[SerializeField] private float mReloadTime;
		[SerializeField] private int mClipSize;

		public float minimumDispersion { get { return mMinimumDispersion; } }
		public float maximumDispersion { get { return mMaximumDispersion; } }
		public float dispersionRamp { get { return mDispersionRamp; } }
		public float damage { get { return mDamage; } }
		public float fireRate { get { return mFireRate; } }
		public float recoil { get { return mRecoil; } }
		public float reloadTime { get { return mReloadTime; } }
		public int clipSize { get { return mClipSize; } }

		public WeaponData(WeaponData other)
		{
			mMinimumDispersion = other.mMinimumDispersion;
			mMaximumDispersion = other.mMaximumDispersion;
			mDispersionRamp = other.mDispersionRamp;
			mDamage = other.mDamage;
			mRecoil = other.mRecoil;
			mFireRate = other.mFireRate;
			mClipSize = other.mClipSize;
			mReloadTime = other.mReloadTime;
		}

		public WeaponData(WeaponData other, WeaponPartData data)
		{
			mDamage = data.damageModifier.Apply(other.mDamage);
			mMinimumDispersion = data.minDispersionModifier.Apply(other.mMinimumDispersion);
			mMaximumDispersion = data.maxDispersionModifier.Apply(other.mMaximumDispersion);
			mDispersionRamp = data.dispersionRampModifier.Apply(other.mDispersionRamp);
			mRecoil = data.recoilModifier.Apply(other.mRecoil);
			mClipSize = data.clipModifier.Apply(other.mClipSize);
			mFireRate = data.fireRateModifier.Apply(other.mFireRate);
			mReloadTime = data.reloadModifier.Apply(other.mReloadTime);
		}

		public override string ToString()
		{
			return string.Format("Spread: {0}, Damage: {1}, FireRate: {2} ClipSize: {3}, Recoil: {4}, Reload: {5}", mMinimumDispersion, mDamage, mFireRate, mClipSize, mRecoil, mReloadTime);
		}
	}
}
