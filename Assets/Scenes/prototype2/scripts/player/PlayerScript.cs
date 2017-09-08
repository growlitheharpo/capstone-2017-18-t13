using UnityEngine;

namespace Prototype2
{
	public class PlayerScript : MonoBehaviour
	{
		[SerializeField] private PlayerWeaponScript mWeapon;
		[SerializeField] private GameObject mDefaultScope;
		[SerializeField] private GameObject mDefaultBarrel;
		[SerializeField] private float mInteractDistance;

		private Transform mMainCameraRef;
		private const string INTERACTABLE_TAG = "interactable";

		private void Start()
		{
			Instantiate(mDefaultBarrel).GetComponent<WeaponPickupScript>().ConfirmAttach();
			Instantiate(mDefaultScope).GetComponent<WeaponPickupScript>().ConfirmAttach();

			mMainCameraRef = Camera.main.transform;

			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetKeyDown, KeyCode.Tab, INPUT_ToggleUIElement, KeatsLib.Unity.Input.InputLevel.None)
				.RegisterInput(Input.GetButton, "Fire1", INPUT_FireWeapon, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, "Interact", INPUT_ActivateInteract, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(INPUT_ActivateInteract)
				.UnregisterInput(INPUT_ToggleUIElement)
				.UnregisterInput(INPUT_FireWeapon);
		}

		private void INPUT_ToggleUIElement()
		{
			EventManager.Notify(EventManager.UIToggle);
		}

		private void INPUT_FireWeapon()
		{
			mWeapon.FireWeapon();
		}

		private void INPUT_ActivateInteract()
		{
			Ray ray = new Ray(mMainCameraRef.position, mMainCameraRef.forward);
			
			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit, mInteractDistance) || !hit.collider.CompareTag(INTERACTABLE_TAG))
				return;

			IInteractable interactable = hit.transform.GetComponent<IInteractable>() ?? hit.transform.parent.GetComponent<IInteractable>();
			if (interactable != null)
				interactable.Interact();
		}
	}
}
