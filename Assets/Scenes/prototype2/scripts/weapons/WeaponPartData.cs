using UnityEngine;

namespace Prototype2
{
	[CreateAssetMenu(menuName = "Weapons/Weapon Part Data")]
	public class WeaponPartData : ScriptableObject
	{
		[SerializeField] private Modifier.Float mSpreadModifier;
		[SerializeField] private Modifier.Float mFireRateModifier;
		[SerializeField] private Modifier.Float mDamageModifier;

		public Modifier.Float spreadModifier { get { return mSpreadModifier; } }
		public Modifier.Float fireRateModifier { get { return mFireRateModifier; } }
		public Modifier.Float damageModifier { get { return mDamageModifier; } }
	}
}
