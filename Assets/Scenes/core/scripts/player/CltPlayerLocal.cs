using FiringSquad.Core;
using FiringSquad.Core.Input;
using FiringSquad.Core.UI;
using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
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
		private BoundProperty<float> mRespawnTimer;

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
				.RegisterInput(Input.GetKeyDown, KeyCode.J, INPUT_ActivateGunPanic, InputLevel.Gameplay)

				// input levels
				.EnableInputLevel(InputLevel.Gameplay)
				.EnableInputLevel(InputLevel.HideCursor)
				.EnableInputLevel(InputLevel.PauseMenu);

			SetupCamera();
			SetupUI();

			mRespawnTimer = new BoundProperty<float>(0, GameplayUIManager.PLAYER_RESPAWN_TIME);

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
				.UnregisterInput(INPUT_TogglePause)
				.UnregisterInput(INPUT_ActivateGunPanic);
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

		private void INPUT_ActivateGunPanic()
		{
			IModifiableWeapon weapon = playerRoot.weapon as IModifiableWeapon;
			if (weapon == null)
				return;

			foreach (Transform w in weapon.transform)
			{
				UnityEngine.Debug.Log(w.name + ": " + w.localPosition);

				if (w.name.ToLower() != "tip")
					w.localPosition = Vector3.zero;
			}

			Transform offset = playerRoot.gunOffset;
			weapon.transform.SetParent(offset);
			weapon.transform.ResetLocalValues();
			weapon.positionOffset = playerRoot.eye.InverseTransformPoint(offset.position);
			weapon.transform.SetParent(playerRoot.transform);
		}

		private void ApplyOptionsData(IOptionsData data)
		{
			FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(data.masterVolume);
			mCameraRef.fieldOfView = data.fieldOfView;
		}

		private void OnLocalPlayerDied(Vector3 spawnPosition, Quaternion spawnRotation, ICharacter killer)
		{
			ServiceLocator.Get<IInput>()
				.DisableInputLevel(InputLevel.Gameplay)
				.DisableInputLevel(InputLevel.PauseMenu);

			INPUT_ExitAimDownSights(); // force an ADS exit

			// do a cool thing with the camera
			mCameraRef.transform.SetParent(null); // leave the camera here for a second
			if (killer != null && !ReferenceEquals(killer, playerRoot))
			{
				StartCoroutine(Coroutines.LerpRotation(mCameraRef.transform,
					Quaternion.LookRotation(killer.transform.position - mCameraRef.transform.position, Vector3.up), 0.75f));
			}

			playerRoot.transform.position = Vector3.one * -5000.0f;

			Vignette temporaryVignette = SetupDeathVignette();
			PostProcessVolume volume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("postprocessing"), 100, temporaryVignette);
			volume.weight = 1.0f;

			StartCoroutine(Coroutines.InvokeEveryTick(time =>
			{
				if (time < playerRoot.defaultData.respawnTime)
				{
					mRespawnTimer.value = Mathf.Ceil(playerRoot.defaultData.respawnTime - time);
					return true; // signal to continue this coroutine
				}

				mRespawnTimer.value = 0.0f;

				ServiceLocator.Get<IInput>()
					.EnableInputLevel(InputLevel.Gameplay)
					.EnableInputLevel(InputLevel.PauseMenu);

				playerRoot.ResetPlayerValues(spawnPosition, spawnRotation);
				mCameraRef.transform.SetParent(playerRoot.eye, false);
				mCameraRef.transform.ResetLocalValues();

				RuntimeUtilities.DestroyVolume(volume, false);
				Destroy(temporaryVignette);

				return false; // signal to end this coroutine
			}));
		}

		private Vignette SetupDeathVignette()
		{
			Vignette temporaryVignette = ScriptableObject.CreateInstance<Vignette>();
			temporaryVignette.enabled.Override(true);
			temporaryVignette.intensity.Override(1.0f);
			temporaryVignette.color.Override(Color.red);
			temporaryVignette.smoothness.Override(1.0f);
			temporaryVignette.roundness.Override(1.0f);

			return temporaryVignette;
		}
	}
}
