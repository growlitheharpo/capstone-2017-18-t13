using System;
using UnityEngine;

namespace Prototype2
{
	[Serializable]
	public struct WeaponData
	{
		// Clip size, reload speed, etc.
		// still needs to be determined by design
		[SerializeField] private float mSpread;
		[SerializeField] private float mDamage;
		[SerializeField] private float mFireRate;

		public float spread { get { return mSpread; } }
		public float damage { get { return mDamage; } }
		public float fireRate { get { return mFireRate; } }

		public WeaponData(WeaponData other)
		{
			mSpread = other.mSpread;
			mDamage = other.mDamage;
			mFireRate = other.mFireRate;
		}

		public WeaponData(WeaponData other, WeaponPartData data)
		{
			mDamage = data.damageModifier.Apply(other.mDamage);
			mSpread = data.spreadModifier.Apply(other.mSpread);
			mFireRate = data.fireRateModifier.Apply(other.mFireRate);
		}

		public override string ToString()
		{
			return string.Format("Spread: {0}, Damage: {1}, FireRate: {2}", mSpread, mDamage, mFireRate);
		}
	}
}
