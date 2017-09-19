using System;
using FiringSquad.Data;
using KeatsLib;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class PlayerScript : MonoBehaviour, IWeaponBearer, IDamageReceiver
	{
		[SerializeField] private PlayerDefaultsData mData;
		private Vector3 mDefaultPosition;

		private PlayerGravGunWeapon mGravityGun;
		private PlayerMovementScript mMovement;
		private BoundProperty<float> mHealth;
		private PlayerWeaponScript mWeapon;
		private bool mGodmode;

		private Transform mMainCameraRef;
		Transform ICharacter.eye { get { return mMainCameraRef; } }
		public IWeapon weapon { get { return mWeapon; } }

		private const string INTERACTABLE_TAG = "interactable";

		private void Awake()
		{
			mDefaultPosition = transform.position;
			mMovement = GetComponent<PlayerMovementScript>();

			if (mData.makeWeaponGun && mData.baseWeaponPrefab != null)
			{
				Transform offset = transform.Find("Gun1Offset");
				GameObject newGun = UnityUtils.InstantiateIntoHolder(mData.baseWeaponPrefab, offset, true, true);
				mWeapon = newGun.GetComponent<PlayerWeaponScript>();
			}
			if (mData.makeGravGun && mData.gravityGunPrefab != null)
			{
				Transform offset = transform.Find("Gun2Offset");
				GameObject newGun = UnityUtils.InstantiateIntoHolder(mData.gravityGunPrefab, offset, true, true);
				mGravityGun = newGun.GetComponent<PlayerGravGunWeapon>();
			}
		}

		private void Start()
		{
			mWeapon.bearer = this;

			if (mGravityGun != null)
				mGravityGun.bearer = this;

			mMainCameraRef = Camera.main.transform;
			mHealth = new BoundProperty<float>(mData.defaultHealth, GameplayUIManager.PLAYER_HEALTH);

			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetButtonDown, "ToggleMenu", INPUT_ToggleUIElement, KeatsLib.Unity.Input.InputLevel.None)
				.RegisterInput(Input.GetButton, "Fire1", INPUT_FireWeapon, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, "Reload", INPUT_ReloadWeapon, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, "Interact", INPUT_ActivateInteract, KeatsLib.Unity.Input.InputLevel.Gameplay);

			ServiceLocator.Get<IGameConsole>()
				.RegisterCommand("godmode", CONSOLE_ToggleGodmode);

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
			mHealth.Cleanup();
		}

		private void InitializeValues()
		{
			if (mData.makeParts)
			{
				WeaponDefaultsData defaults = mData.defaultWeaponParts;

				foreach (GameObject part in defaults)
					Instantiate(part).GetComponent<WeaponPickupScript>().ConfirmAttach();
			}

			mHealth.value = mData.defaultHealth;
			transform.position = mDefaultPosition;
			transform.rotation = Quaternion.identity;

			ServiceLocator.Get<IInput>()
				.EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);
		}

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
			if (!Physics.Raycast(ray, out hit, mData.interactDistance) || !hit.collider.CompareTag(INTERACTABLE_TAG))
				return;

			IInteractable interactable = hit.transform.GetComponent<IInteractable>() ?? hit.transform.parent.GetComponent<IInteractable>();
			if (interactable != null)
				interactable.Interact();
		}

		private void CONSOLE_ToggleGodmode(string[] args)
		{
			if (args.Length > 0)
				throw new ArgumentException("Invalid arguments for command 'godmode'");

			ServiceLocator.Get<IGameConsole>()
				.AssertCheatsEnabled();

			mGodmode = !mGodmode;
		}

		public void ApplyDamage(float amount, Vector3 point, IDamageSource cause = null)
		{
			if (mGodmode || mHealth.value <= 0.0f)
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
