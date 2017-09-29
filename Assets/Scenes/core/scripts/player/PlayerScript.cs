using System;
using System.Collections.Generic;
using FiringSquad.Data;
using KeatsLib;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Input = UnityEngine.Input;
using InputLevel = KeatsLib.Unity.Input.InputLevel;

namespace FiringSquad.Gameplay
{
	public class PlayerScript : NetworkBehaviour, IWeaponBearer, IDamageReceiver
	{
		[SerializeField] private GameObject mCameraPrefab;
		[SerializeField] private PlayerInputMap mInputMap;
		[SerializeField] private PlayerDefaultsData mData;

		private Vector3 mDefaultPosition;

		private PlayerGravGunWeapon mGravityGun;
		private PlayerMovementScript mMovement;
		private BoundProperty<float> mHealth;
		private PlayerWeaponScript mWeapon;
		private bool mGodmode;
		private Transform mMainCameraRef;

		public Transform eye { get { return mMainCameraRef; } }
		public IWeapon weapon { get { return mWeapon; } }
		public WeaponDefaultsData defaultParts { get { return mDefaultsOverride ?? mData.defaultWeaponParts; } }
		public PlayerInputMap inputMap { get { return mInputMap; } }

		private WeaponDefaultsData mDefaultsOverride;
		private const string INTERACTABLE_TAG = "interactable";

		private void Awake()
		{
			Logger.Info("AWAKE");
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

		public override void OnStartLocalPlayer()
		{
			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetButtonDown, inputMap.toggleMenuButton, INPUT_ToggleUIElement, InputLevel.None)
				.RegisterInput(Input.GetButton, inputMap.fireWeaponButton, INPUT_FireWeapon, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, inputMap.reloadButton, INPUT_ReloadWeapon, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, inputMap.interactButton, INPUT_ActivateInteract, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, inputMap.pauseButton, INPUT_TogglePause, InputLevel.PauseMenu);

			ServiceLocator.Get<IGameConsole>()
				.RegisterCommand("godmode", CONSOLE_ToggleGodmode);

			if (mGravityGun != null)
				mGravityGun.RegisterInput(inputMap);
		}

		private void Start()
		{
			mMainCameraRef = transform.Find("CameraOffset");
			if (isLocalPlayer)
				BindCamera();

			mWeapon.bearer = this;

			int label = isLocalPlayer ? GameplayUIManager.PLAYER_HEALTH : -1;
			mHealth = new BoundProperty<float>(mData.defaultHealth, label);

			if (mGravityGun != null)
				mGravityGun.bearer = this;

			EventManager.OnResetLevel += ReceiveResetEvent;
			EventManager.OnApplyOptionsData += ApplyOptionsData;
			InitializeValues();
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(INPUT_ActivateInteract)
				.UnregisterInput(INPUT_ToggleUIElement)
				.UnregisterInput(INPUT_ReloadWeapon)
				.UnregisterInput(INPUT_FireWeapon)
				.UnregisterInput(INPUT_TogglePause);

			ServiceLocator.Get<IGameConsole>()
				.UnregisterCommand("godmode");

			EventManager.OnResetLevel -= ReceiveResetEvent;
			EventManager.OnApplyOptionsData -= ApplyOptionsData;
			mHealth.Cleanup();
		}

		private void BindCamera()
		{
			Camera c = (Camera.main ?? FindObjectOfType<Camera>()) ?? Instantiate(mCameraPrefab).GetComponent<Camera>();
			c.transform.SetParent(mMainCameraRef, false);
			c.transform.ResetLocalValues();
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
			if (mMovement != null)
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
		
		private void INPUT_TogglePause()
		{
			EventManager.Notify(EventManager.TogglePauseState);
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
		
		private void ApplyOptionsData(IOptionsData settings)
		{
			mMainCameraRef.GetComponentInChildren<Camera>().fieldOfView = settings.fieldOfView;
			AudioListener.volume = settings.masterVolume;
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

		[Command]
		public void CmdReflectWeaponFire(byte[] data)
		{
			RpcFireShotNow(data);
		}

		[ClientRpc]
		public void RpcFireShotNow(byte[] data)
		{
			if (isLocalPlayer) // we already reflected this
				return;

			var shots = new List<Ray>();
			NetworkReader reader = new NetworkReader(data);
			int count = reader.ReadInt32();

			for (int i = 0; i < count; i++)
				shots.Add(reader.ReadRay());

			weapon.FireShotImmediate(shots);
		}
	}
}
