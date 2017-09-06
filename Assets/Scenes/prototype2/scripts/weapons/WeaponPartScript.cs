using UnityEngine;

using WeaponData = Prototype2.PlayerWeaponScript.WeaponData;

namespace Prototype2
{
	public class WeaponPartScript : MonoBehaviour
	{
		[SerializeField] private Modifier.Float mRecoilModifier;
		[SerializeField] private Modifier.Float mSpreadModifier;
		[SerializeField] private Modifier.Float mFireRateModifier;
		[SerializeField] private Modifier.Float mDamageModifier;

		public WeaponData ApplyEffects(WeaponData start)
		{
			WeaponData newData = new WeaponData(start)
			{
				mDefaultDamage = mDamageModifier.Apply(start.mDefaultDamage),
				mDefaultSpread = mSpreadModifier.Apply(start.mDefaultSpread),
				mDefaultRecoil = mRecoilModifier.Apply(start.mDefaultRecoil),
				mFireRate = mFireRateModifier.Apply(start.mFireRate)
			};


			return newData;
		}
	}
}
