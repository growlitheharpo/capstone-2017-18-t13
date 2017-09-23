﻿using System;
using FiringSquad.Data;
using KeatsLib;
using UnityEngine;
using InputLevel = KeatsLib.Unity.Input.InputLevel;

namespace FiringSquad.Gameplay
{
	public class PlayerScript : MonoBehaviour, IWeaponBearer, IDamageReceiver
	{
		[SerializeField] private PlayerInputMap mInputMap;
		[SerializeField] private PlayerDefaultsData mData;

		[SerializeField] private string mOverrideUIName;
		public string overrideUIName { get { return mOverrideUIName; } }

		private Vector3 mDefaultPosition;

		private PlayerGravGunWeapon mGravityGun;
		private PlayerMovementScript mMovement;
		private BoundProperty<float> mHealth;
		private PlayerWeaponScript mWeapon;
		private bool mGodmode;
		private Transform mMainCameraRef;

		Transform ICharacter.eye { get { return mMainCameraRef; } }
		public IWeapon weapon { get { return mWeapon; } }
		public WeaponDefaultsData defaultParts { get { return mDefaultsOverride ?? mData.defaultWeaponParts; } }
		public PlayerInputMap inputMap { get { return mInputMap; } }

		private WeaponDefaultsData mDefaultsOverride;
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

			mMainCameraRef = GetComponentInChildren<Camera>().transform;

			// TODO: GET RID OF THIS MESS
			int val = string.IsNullOrEmpty(mOverrideUIName) ? GameplayUIManager.PLAYER_HEALTH : (mOverrideUIName + "-health").GetHashCode();
			mHealth = new BoundProperty<float>(mData.defaultHealth, val);

			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetButtonDown, inputMap.toggleMenuButton, INPUT_ToggleUIElement, InputLevel.None)
				.RegisterInput(Input.GetButton, inputMap.fireWeaponButton, INPUT_FireWeapon, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, inputMap.reloadButton, INPUT_ReloadWeapon, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, inputMap.interactButton, INPUT_ActivateInteract, InputLevel.Gameplay);

			if (mGravityGun != null)
				mGravityGun.RegisterInput(inputMap);

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

			ServiceLocator.Get<IGameConsole>()
				.UnregisterCommand("godmode");

			EventManager.OnResetLevel -= ReceiveResetEvent;
			mHealth.Cleanup();
		}

		private void InitializeValues(bool reposition = false)
		{
			if (mData.makeParts)
			{
				WeaponDefaultsData defaults = defaultParts;

				foreach (GameObject part in defaults)
				{
					Instantiate(part)
						.GetComponent<WeaponPickupScript>()
						.OverrideDurability(WeaponPartScript.INFINITE_DURABILITY)
						.ConfirmAttach(mWeapon);
				}
			}

			mHealth.value = mData.defaultHealth;

			if (reposition)
			{
				transform.position = mDefaultPosition;
				transform.rotation = Quaternion.identity;
			}
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
			IInteractable interactable = null;

			if (mGravityGun != null)
				interactable = mGravityGun.heldObject;

			if (interactable == null)
			{
				Ray ray = new Ray(mMainCameraRef.position, mMainCameraRef.forward);
			
				RaycastHit hit;
				if (!Physics.Raycast(ray, out hit, mData.interactDistance) || !hit.collider.CompareTag(INTERACTABLE_TAG))
					return;

				interactable = hit.GetInteractableComponent();
			}

			if (interactable != null)
				interactable.Interact(this);
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
				EventManager.Notify(() => EventManager.PlayerDied(this));
		}

		private void ReceiveResetEvent()
		{
			InitializeValues(true);
		}

		public void OverrideDefaultParts(GameObject mechanism, GameObject barrel, GameObject scope, GameObject grip)
		{
			mDefaultsOverride = new WeaponDefaultsData(mechanism, barrel, scope, grip);
			InitializeValues();

		}

		/// <summary>
		/// Resets the player's health and weapon.
		/// </summary>
		public void ResetArenaPlayer()
		{
			InitializeValues();
		}
	}
}
