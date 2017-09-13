using UnityEngine;

namespace Prototype2
{
	public class PlayerScript : MonoBehaviour, IWeaponBearer, IDamageReceiver
	{
		[SerializeField] private PlayerWeaponScript mWeapon;
		[SerializeField] private GameObject mDefaultScope;
		[SerializeField] private GameObject mDefaultBarrel;
		[SerializeField] private GameObject mDefaultMechanism;
		[SerializeField] private GameObject mDefaultGrip;
		[SerializeField] private float mInteractDistance;
		[SerializeField] private float mDefaultHealth;
		private Vector3 mDefaultPosition;

		private PlayerMovementScript mMovement;
		private BoundProperty<float> mHealth;

		private Transform mMainCameraRef;
		private const string INTERACTABLE_TAG = "interactable";

		private void Awake()
		{
			mDefaultPosition = transform.position;
			mMovement = GetComponent<PlayerMovementScript>();
		}

		private void Start()
		{
			mWeapon.bearer = this;
			mMainCameraRef = Camera.main.transform;
			mHealth = new BoundProperty<float>(mDefaultHealth, GameplayUIManager.PLAYER_HEALTH);

			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetButtonDown, "ToggleMenu", INPUT_ToggleUIElement, KeatsLib.Unity.Input.InputLevel.None)
				.RegisterInput(Input.GetButton, "Fire1", INPUT_FireWeapon, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, "Reload", INPUT_ReloadWeapon, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, "Interact", INPUT_ActivateInteract, KeatsLib.Unity.Input.InputLevel.Gameplay);

			EventManager.OnResetLevel += ReceiveResetEvent;

			InitializeValues();
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(INPUT_ActivateInteract)
				.UnregisterInput(INPUT_ToggleUIElement)
				.UnregisterInput(INPUT_ReloadWeapon)
				.UnregisterInput(INPUT_FireWeapon);

			EventManager.OnResetLevel -= ReceiveResetEvent;
		}

		private void InitializeValues()
		{
			Instantiate(mDefaultMechanism).GetComponent<WeaponPickupScript>().ConfirmAttach();
			Instantiate(mDefaultBarrel).GetComponent<WeaponPickupScript>().ConfirmAttach();
			Instantiate(mDefaultScope).GetComponent<WeaponPickupScript>().ConfirmAttach();
			Instantiate(mDefaultGrip).GetComponent<WeaponPickupScript>().ConfirmAttach();

			mHealth.value = mDefaultHealth;
			transform.position = mDefaultPosition;
			transform.rotation = Quaternion.identity;

			ServiceLocator.Get<IInput>()
				.EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);
		}

		GameObject ICharacter.GetGameObject()
		{
			return gameObject;
		}

		Transform ICharacter.eye { get { return mMainCameraRef; } }

		public void ApplyRecoil(Vector3 direction, float amount)
		{
			mMovement.AddRecoil(direction, amount);
		}

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
			if (mHealth.value <= 0.0f)
				return;

			if (cause != null && ReferenceEquals(cause.source, this))
				amount /= 2.0f;

			mHealth.value = Mathf.Clamp(mHealth.value - amount, 0.0f, float.MaxValue);

			if (mHealth.value <= 0.0f)
				EventManager.Notify(EventManager.PlayerDied);
		}

		private void ReceiveResetEvent()
		{
			InitializeValues();
		}
	}
}
