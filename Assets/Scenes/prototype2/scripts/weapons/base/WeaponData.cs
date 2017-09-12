using System;
using UnityEngine;

namespace Prototype2
{
	[Serializable]
	public struct WeaponData
	{
		[SerializeField] private float mSpread;
		[SerializeField] private float mDamage;
		[SerializeField] private float mFireRate;
		[SerializeField] private float mRecoil;
		[SerializeField] private float mReloadTime;
		[SerializeField] private int mClipSize;

		public float spread { get { return mSpread; } }
		public float damage { get { return mDamage; } }
		public float fireRate { get { return mFireRate; } }
		public float recoil { get { return mRecoil; } }
		public float reloadTime { get { return mReloadTime; } }
		public int clipSize { get { return mClipSize; } }

		public WeaponData(WeaponData other)
		{
			mSpread = other.mSpread;
			mDamage = other.mDamage;
			mRecoil = other.mRecoil;
			mFireRate = other.mFireRate;
			mClipSize = other.mClipSize;
			mReloadTime = other.mReloadTime;
		}

		public WeaponData(WeaponData other, WeaponPartData data)
		{
			mDamage = data.damageModifier.Apply(other.mDamage);
			mSpread = data.spreadModifier.Apply(other.mSpread);
			mRecoil = data.recoilModifier.Apply(other.mRecoil);
			mClipSize = data.clipModifier.Apply(other.mClipSize);
			mFireRate = data.fireRateModifier.Apply(other.mFireRate);
			mReloadTime = data.reloadModifier.Apply(other.mReloadTime);
		}

		public override string ToString()
		{
			return string.Format("Spread: {0}, Damage: {1}, FireRate: {2} ClipSize: {3}, Recoil: {4}, Reload: {5}", mSpread, mDamage, mFireRate, mClipSize, mRecoil, mReloadTime);
		}
	}
}
