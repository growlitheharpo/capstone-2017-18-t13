using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FiringSquad.Data
{
	[Serializable]
	public class WeaponPartData
	{
		[Header("Dispersion")]
		[FormerlySerializedAs("mSpreadModifier")] [SerializeField] private Modifier.Float mMinDispersionModifier;
		[SerializeField] private Modifier.Float mMaxDispersionModifier;
		[SerializeField] private Modifier.Float mDispersionRampModifier;

		[Header("Recoil")]
		[FormerlySerializedAs("mRecoilModifier")]
		[SerializeField] private Modifier.Float mRecoilAmountModifier;
		[SerializeField] private Modifier.Float mRecoilTimeModifier;

		[Header("Damage")]
		[SerializeField] private Modifier.Float mDamageModifier;
		[SerializeField] private Modifier.Float mDamageFalloffDistanceModifier;

		[Header("Other")]
		[SerializeField] private Modifier.Float mFireRateModifier;
		[SerializeField] private Modifier.Float mReloadTimeModifier;
		[SerializeField] private Modifier.Int mClipSizeModifier;

		public Modifier.Float minDispersionModifier { get { return mMinDispersionModifier; } }
		public Modifier.Float maxDispersionModifier { get { return mMaxDispersionModifier; } }
		public Modifier.Float dispersionRampModifier { get { return mDispersionRampModifier; } }
		public Modifier.Float recoilAmountModifier { get { return mRecoilAmountModifier; } }
		public Modifier.Float recoilTimeModifier { get { return mRecoilTimeModifier; } }
		public Modifier.Float damageModifier { get { return mDamageModifier; } }
		public Modifier.Float damageFalloffDistanceModifier { get { return mDamageFalloffDistanceModifier; } }
		public Modifier.Float fireRateModifier { get { return mFireRateModifier; } }
		public Modifier.Float reloadModifier { get { return mReloadTimeModifier; } }
		public Modifier.Int clipModifier { get { return mClipSizeModifier; } }
	}
}
