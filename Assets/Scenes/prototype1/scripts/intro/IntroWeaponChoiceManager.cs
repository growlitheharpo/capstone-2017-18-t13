using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class IntroWeaponChoiceManager : MonoBehaviour
	{
		[SerializeField] private WeaponDefaultsData mDefaultParts;

		private WeaponDefaultsData mChosenParts;
		private DemoWeaponScript mDemoWeapon;

		private BoundProperty<float> mSpread, mRecoil, mReload, mFireRate, mDamage, mClipsize;

		private void Awake()
		{
			mChosenParts = new WeaponDefaultsData(mDefaultParts);

			mSpread = new BoundProperty<float>(0.0f, "IntroSpread".GetHashCode());
			mRecoil = new BoundProperty<float>(0.0f, "IntroRecoil".GetHashCode());
			mReload = new BoundProperty<float>(0.0f, "IntroReloadTime".GetHashCode());
			mFireRate = new BoundProperty<float>(0.0f, "IntroFireRate".GetHashCode());
			mDamage = new BoundProperty<float>(0.0f, "IntroDamage".GetHashCode());
			mClipsize = new BoundProperty<float>(0.0f, "IntroClipSize".GetHashCode());
		}

		private void Start()
		{
			mDemoWeapon = FindObjectOfType<DemoWeaponScript>();
			
			foreach (GameObject part in mChosenParts)
				Instantiate(part).GetComponent<WeaponPickupScript>().ConfirmAttach(mDemoWeapon);
		}

		private void Update()
		{
			var data = mDemoWeapon.currentStats;

			mSpread.value = data.spread;
			mRecoil.value = data.recoil;
			mReload.value = data.reloadTime;
			mFireRate.value = data.fireRate;
			mDamage.value = data.damage;
			mClipsize.value = data.clipSize;
		}
	}
}
