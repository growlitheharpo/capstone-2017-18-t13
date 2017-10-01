using UnityEngine;
using UnityEngine.Serialization;

namespace FiringSquad.Data
{
	[CreateAssetMenu(menuName = "Weapons/Weapon Part Data")]
	public class WeaponPartData : ScriptableObject
	{
		[FormerlySerializedAs("mSpreadModifier")] [SerializeField] private Modifier.Float mMinDispersionModifier;
		[SerializeField] private Modifier.Float mMaxDispersionModifier;
		[SerializeField] private Modifier.Float mFireRateModifier;
		[SerializeField] private Modifier.Float mDamageModifier;
		[SerializeField] private Modifier.Float mReloadTimeModifier;
		[SerializeField] private Modifier.Float mRecoilModifier;
		[SerializeField] private Modifier.Int mClipSizeModifier;

		public Modifier.Float minDispersionModifier { get { return mMinDispersionModifier; } }
		public Modifier.Float maxDispersionModifier { get { return mMaxDispersionModifier; } }
		public Modifier.Float fireRateModifier { get { return mFireRateModifier; } }
		public Modifier.Float damageModifier { get { return mDamageModifier; } }
		public Modifier.Float reloadModifier { get { return mReloadTimeModifier; } }
		public Modifier.Float recoilModifier { get { return mRecoilModifier; } }
		public Modifier.Int clipModifier { get { return mClipSizeModifier; } }
	}
}
