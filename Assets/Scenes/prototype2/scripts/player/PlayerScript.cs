using UnityEngine;

namespace Prototype2
{
	public class PlayerScript : MonoBehaviour, ICharacter, IDamageReceiver
	{
		[SerializeField] private PlayerWeaponScript mWeapon;
		[SerializeField] private GameObject mDefaultScope;
		[SerializeField] private GameObject mDefaultBarrel;
		[SerializeField] private GameObject mDefaultMechanism;
		[SerializeField] private float mInteractDistance;

		private BoundProperty<float> mHealth;

		private Transform mMainCameraRef;
		private const string INTERACTABLE_TAG = "interactable";

		private void Start()
		{
			mWeapon.bearer = this;
			Instantiate(mDefaultMechanism).GetComponent<WeaponPickupScript>().ConfirmAttach();
			Instantiate(mDefaultBarrel).GetComponent<WeaponPickupScript>().ConfirmAttach();
			Instantiate(mDefaultScope).GetComponent<WeaponPickupScript>().ConfirmAttach();

			mHealth = new BoundProperty<float>(100.0f, GameplayUIManager.PLAYER_HEALTH);

			mMainCameraRef = Camera.main.transform;

			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetKeyDown, KeyCode.Tab, INPUT_ToggleUIElement, KeatsLib.Unity.Input.InputLevel.None)
				.RegisterInput(Input.GetButton, "Fire1", INPUT_FireWeapon, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, "Reload", INPUT_ReloadWeapon, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, "Interact", INPUT_ActivateInteract, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(INPUT_ActivateInteract)
				.UnregisterInput(INPUT_ToggleUIElement)
				.UnregisterInput(INPUT_ReloadWeapon)
				.UnregisterInput(INPUT_FireWeapon);
		}

		GameObject ICharacter.GetGameObject()
		{
			return gameObject;
		}

		Transform ICharacter.eye { get { return mMainCameraRef; } }

		private void INPUT_ToggleUIElement()
		{
			EventManager.Notify(EventManager.UIToggle);
		}

		private void INPUT_FireWeapon()
		{
			mWeapon.FireWeapon();
		}
		
		private void INPUT_ReloadWeapon()
		{
			mWeapon.Reload();
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

		public void ApplyDamage(float amount, Vector3 point, IDamageSource cause = null)
		{
			if (cause != null && cause.source == this)
				amount /= 2.0f;

			mHealth.value -= amount;
		}
	}
}
