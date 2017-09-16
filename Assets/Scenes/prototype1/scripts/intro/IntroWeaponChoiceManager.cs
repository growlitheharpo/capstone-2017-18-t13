using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class IntroWeaponChoiceManager : MonoBehaviour
	{
		[SerializeField] private WeaponDefaultsData mDefaultParts;

		private WeaponDefaultsData mChosenParts;
		private DemoWeaponScript mDemoWeapon;

		private void Awake()
		{
			mChosenParts = new WeaponDefaultsData(mDefaultParts);
		}

		private void Start()
		{
			mDemoWeapon = FindObjectOfType<DemoWeaponScript>();
			
			foreach (GameObject part in mChosenParts)
				Instantiate(part).GetComponent<WeaponPickupScript>().ConfirmAttach(mDemoWeapon);
		}
	}
}
