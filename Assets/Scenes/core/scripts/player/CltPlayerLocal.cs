using FiringSquad.Core;
using FiringSquad.Core.Input;
using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;
using Input = UnityEngine.Input;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Gameplay
{
	public class CltPlayerLocal : MonoBehaviour
	{
		[SerializeField] private PlayerInputMap mInputMap;
		public PlayerInputMap inputMap { get { return mInputMap; } }

		[SerializeField] private GameObject mCameraPrefab;

		public CltPlayer playerRoot { get; set; }

		private Camera mCameraRef;
		private Vector3 mCameraOriginalPos;
		private Quaternion mCameraOriginalRot;

		public bool inAimDownSightsMode { get; private set; }

		// Use this for initialization
		private void Start()
		{
			ServiceLocator.Get<IInput>()
				// networked
				.RegisterInput(Input.GetButton, inputMap.fireWeaponButton, INPUT_WeaponFireHold, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, inputMap.fireWeaponButton, INPUT_WeaponFireUp, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, inputMap.reloadButton, INPUT_WeaponReload, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, inputMap.interactButton, INPUT_ActivateInteract, InputLevel.Gameplay)
				.RegisterInput(Input.GetButton, inputMap.fireGravGunButton, INPUT_MagnetArmHeld, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, inputMap.fireGravGunButton, INPUT_MagnetArmUp, InputLevel.Gameplay)

				// local
				.RegisterInput(Input.GetButtonDown, inputMap.activateADSButton, INPUT_EnterAimDownSights, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, inputMap.activateADSButton, INPUT_ExitAimDownSights, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, inputMap.pauseButton, INPUT_TogglePause, InputLevel.PauseMenu)

				// input levels
				.EnableInputLevel(InputLevel.Gameplay)
				.EnableInputLevel(InputLevel.HideCursor)
				.EnableInputLevel(InputLevel.PauseMenu);

			SetupCamera();
			SetupUI();

			EventManager.Local.OnApplyOptionsData += ApplyOptionsData;
			EventManager.Local.OnLocalPlayerDied += OnLocalPlayerDied;
		}

		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerDied -= OnLocalPlayerDied;
			EventManager.Local.OnApplyOptionsData -= ApplyOptionsData;
			CleanupUI();
			CleanupCamera();

			ServiceLocator.Get<IInput>()
				// networked
				.UnregisterInput(INPUT_WeaponFireHold)
				.UnregisterInput(INPUT_WeaponFireUp)
				.UnregisterInput(INPUT_WeaponReload)
				.UnregisterInput(INPUT_ActivateInteract)
				.UnregisterInput(INPUT_MagnetArmHeld)
				.UnregisterInput(INPUT_MagnetArmUp)

				// local
				.UnregisterInput(INPUT_EnterAimDownSights)
				.UnregisterInput(INPUT_ExitAimDownSights)
				.UnregisterInput(INPUT_TogglePause);
		}

		private void SetupCamera()
		{
			mCameraRef = Camera.main ?? FindObjectOfType<Camera>();
			if (mCameraRef == null)
			{
				mCameraRef = Instantiate(mCameraPrefab).GetComponent<Camera>();
				mCameraOriginalPos = Vector3.one * -1.0f;
			}
			else
			{
				mCameraOriginalPos = mCameraRef.transform.position;
				mCameraOriginalRot = mCameraRef.transform.rotation;
			}

			mCameraRef.transform.SetParent(playerRoot.eye, false);
			mCameraRef.transform.ResetLocalValues();
		}

		private void SetupUI() { }

		public void CleanupCamera()
		{
			if (mCameraRef == null)
				return;

			if (mCameraOriginalPos == Vector3.one * -1.0f)
				return;

			mCameraRef.transform.SetParent(null);
			mCameraRef.transform.position = mCameraOriginalPos;
			mCameraRef.transform.rotation = mCameraOriginalRot;

			mCameraRef = null;
		}

		private void CleanupUI() { }

		private void INPUT_WeaponFireHold()
		{
			playerRoot.weapon.FireWeaponHold();
		}

		private void INPUT_WeaponFireUp()
		{
			playerRoot.weapon.FireWeaponUp();
		}

		private void INPUT_WeaponReload()
		{
			playerRoot.weapon.Reload();
		}

		private void INPUT_MagnetArmHeld()
		{
			if (inAimDownSightsMode)
				return;

			playerRoot.magnetArm.FireHeld();
		}

		private void INPUT_MagnetArmUp()
		{
			playerRoot.magnetArm.FireUp();
		}

		private void INPUT_ActivateInteract()
		{
			playerRoot.CmdActivateInteract(playerRoot.eye.position, playerRoot.eye.forward);
		}

		private void INPUT_TogglePause()
		{
			if (inAimDownSightsMode)
				return;

			EventManager.Local.TogglePause();
		}

		private void INPUT_EnterAimDownSights()
		{
			inAimDownSightsMode = true;
			EventManager.Notify(EventManager.Local.EnterAimDownSightsMode);
		}

		private void INPUT_ExitAimDownSights()
		{
			inAimDownSightsMode = false;
			EventManager.Notify(EventManager.Local.ExitAimDownSightsMode);
		}

		private void ApplyOptionsData(IOptionsData data)
		{
			FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(data.masterVolume);
			mCameraRef.fieldOfView = data.fieldOfView;
		}

		private void OnLocalPlayerDied(Vector3 spawnPosition, Quaternion spawnRotation)
		{
			playerRoot.ResetPlayerValues(spawnPosition, spawnRotation);
		}
	}
}
