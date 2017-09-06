using UnityEngine;

namespace Prototype2
{
	public class PlayerScript : MonoBehaviour
	{
		[SerializeField] private PlayerWeaponScript mWeapon;
		[SerializeField] private GameObject mDefaultScope;
		[SerializeField] private GameObject mDefaultBarrel;

		private void Start()
		{
			mWeapon.AttachNewPart(PlayerWeaponScript.Attachment.Barrel, Instantiate(mDefaultBarrel).GetComponent<WeaponPartScript>());
			mWeapon.AttachNewPart(PlayerWeaponScript.Attachment.Scope, Instantiate(mDefaultScope).GetComponent<WeaponPartScript>());

			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetKeyDown, KeyCode.Space, INPUT_ToggleUIElement, KeatsLib.Unity.Input.InputLevel.None)
				.RegisterInput(Input.GetMouseButton, 0, INPUT_FireWeapon, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);
		}

		private void INPUT_ToggleUIElement()
		{
			EventManager.Notify(EventManager.UIToggle);
		}

		private void INPUT_FireWeapon()
		{
			mWeapon.FireWeapon();
		}
	}
}
