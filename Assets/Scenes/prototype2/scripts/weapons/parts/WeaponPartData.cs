using UnityEngine;

namespace Prototype2
{
	[CreateAssetMenu(menuName = "Weapons/Weapon Part Data")]
	public class WeaponPartData : ScriptableObject
	{
		[SerializeField] private Modifier.Float mSpreadModifier;
		[SerializeField] private Modifier.Float mFireRateModifier;
		[SerializeField] private Modifier.Float mDamageModifier;
		[SerializeField] private Modifier.Int mClipSizeModifier;

		public Modifier.Float spreadModifier { get { return mSpreadModifier; } }
		public Modifier.Float fireRateModifier { get { return mFireRateModifier; } }
		public Modifier.Float damageModifier { get { return mDamageModifier; } }
		public Modifier.Int clipModifier { get { return mClipSizeModifier; } }
	}
}
