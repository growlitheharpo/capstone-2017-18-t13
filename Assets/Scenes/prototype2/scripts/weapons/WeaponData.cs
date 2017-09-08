using System;
using UnityEngine;

namespace Prototype2
{
	[Serializable]
	public struct WeaponData
	{
		// Clip size, reload speed, etc.
		// still needs to be determined by design
		[SerializeField] public float mDefaultSpread;
		[SerializeField] public float mDefaultDamage;
		[SerializeField] public float mFireRate;

		public WeaponData(WeaponData other)
		{
			mDefaultSpread = other.mDefaultSpread;
			mDefaultDamage = other.mDefaultDamage;
			mFireRate = other.mFireRate;
		}

		public WeaponData(WeaponData other, WeaponPartData data)
		{
			mDefaultDamage = data.damageModifier.Apply(other.mDefaultDamage);
			mDefaultSpread = data.spreadModifier.Apply(other.mDefaultSpread);
			mFireRate = data.fireRateModifier.Apply(other.mFireRate);
		}

		public override string ToString()
		{
			return string.Format("Spread: {0}, Damage: {1}, FireRate: {2}", mDefaultSpread, mDefaultDamage, mFireRate);
		}
	}
}
