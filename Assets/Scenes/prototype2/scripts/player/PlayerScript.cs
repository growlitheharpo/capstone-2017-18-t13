using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype2
{
	public class PlayerScript : MonoBehaviour
	{
		[SerializeField] private PlayerWeaponScript mWeapon;
		[SerializeField] private GameObject mDefaultScope;
		[SerializeField] private GameObject mDefaultBarrel;

		// Use this for initialization
		private void Start()
		{
			mWeapon.AttachNewPart(PlayerWeaponScript.Attachment.Barrel, Instantiate(mDefaultBarrel).GetComponent<WeaponPartScript>());
			mWeapon.AttachNewPart(PlayerWeaponScript.Attachment.Scope, Instantiate(mDefaultScope).GetComponent<WeaponPartScript>());

			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetMouseButtonDown, 0, INPUT_FireWeapon, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);
		}

		private void INPUT_FireWeapon()
		{
			mWeapon.FireWeapon();
		}
	}
}
