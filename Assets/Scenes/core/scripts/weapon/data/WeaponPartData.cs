using UnityEngine;
using UnityEngine.Serialization;

namespace FiringSquad.Data
{
	[CreateAssetMenu(menuName = "Weapons/Weapon Part Data")]
	public class WeaponPartData : ScriptableObject
	{
		[FormerlySerializedAs("mSpreadModifier")] [SerializeField] private Modifier.Float mMinDispersionModifier;
		[SerializeField] private Modifier.Float mMaxDispersionModifier;
		[SerializeField] private Modifier.Float mDispersionRampModifier;

		[FormerlySerializedAs("mRecoilModifier")]
		[SerializeField] private Modifier.Float mRecoilAmountModifier;
		[SerializeField] private Modifier.Float mRecoilTimeModifier;

		[SerializeField] private Modifier.Float mFireRateModifier;
		[SerializeField] private Modifier.Float mDamageModifier;
		[SerializeField] private Modifier.Float mReloadTimeModifier;
		[SerializeField] private Modifier.Int mClipSizeModifier;

		public Modifier.Float minDispersionModifier { get { return mMinDispersionModifier; } }
		public Modifier.Float maxDispersionModifier { get { return mMaxDispersionModifier; } }
		public Modifier.Float dispersionRampModifier { get { return mDispersionRampModifier; } }
		public Modifier.Float recoilAmountModifier { get { return mRecoilAmountModifier; } }
		public Modifier.Float recoilTimeModifier { get { return mRecoilTimeModifier; } }
		public Modifier.Float fireRateModifier { get { return mFireRateModifier; } }
		public Modifier.Float damageModifier { get { return mDamageModifier; } }
		public Modifier.Float reloadModifier { get { return mReloadTimeModifier; } }
		public Modifier.Int clipModifier { get { return mClipSizeModifier; } }
	}
}
