using FiringSquad.Core;
using FiringSquad.Core.Input;
using FiringSquad.Core.UI;
using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using Input = UnityEngine.Input;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// The local class that handles non-movement input for the local player.
	/// Handles some local effects, such as the respawn time.
	/// </summary>
	public class CltPlayerLocal : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private PlayerInputMap mInputMap;
		[SerializeField] private GameObject mCameraPrefab;

		/// Private variables
		private Camera mCameraRef;
		private Vector3 mCameraOriginalPos;
		private Quaternion mCameraOriginalRot;
		private BoundProperty<float> mRespawnTimer;

		/// <summary> The input map for this player. </summary>
		public PlayerInputMap inputMap { get { return mInputMap; } }

		/// <summary> The CltPlayer that this local player is linked to. </summary>
		public CltPlayer playerRoot { get; set; }

		/// <summary> Whether or not this player is currently in AimDownSights mode. </summary>
		public bool inAimDownSightsMode { get; private set; }

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			ServiceLocator.Get<IInput>()
				// networked
				.RegisterInput(Input.GetButton, inputMap.fireWeaponButton, INPUT_WeaponFireHold, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, inputMap.fireWeaponButton, INPUT_WeaponFireUp, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, inputMap.reloadButton, INPUT_WeaponReload, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, inputMap.fireMagnetArmButton, INPUT_MagnetArmDown, InputLevel.Gameplay)
				.RegisterInput(Input.GetButton, inputMap.fireMagnetArmButton, INPUT_MagnetArmHeld, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, inputMap.fireMagnetArmButton, INPUT_MagnetArmUp, InputLevel.Gameplay)

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

			mRespawnTimer = new BoundProperty<float>(0, UIManager.PLAYER_RESPAWN_TIME);

			EventManager.Local.OnApplyOptionsData += OnApplyOptionsData;
			EventManager.Local.OnLocalPlayerDied += OnLocalPlayerDied;
		}

		/// <summary>
		/// Cleanup all listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerDied -= OnLocalPlayerDied;
			EventManager.Local.OnApplyOptionsData -= OnApplyOptionsData;
			CleanupCamera();

			ServiceLocator.Get<IInput>()
				// networked
				.UnregisterInput(INPUT_WeaponFireHold)
				.UnregisterInput(INPUT_WeaponFireUp)
				.UnregisterInput(INPUT_WeaponReload)
				.UnregisterInput(INPUT_MagnetArmDown)
				.UnregisterInput(INPUT_MagnetArmHeld)
				.UnregisterInput(INPUT_MagnetArmUp)

				// local
				.UnregisterInput(INPUT_EnterAimDownSights)
				.UnregisterInput(INPUT_ExitAimDownSights)
				.UnregisterInput(INPUT_TogglePause)
				.UnregisterInput(INPUT_ActivateGunPanic);
		}

		/// <summary>
		/// Instantiate a camera or steal the one that already exists in the scene
		/// and attach it to our face.
		/// </summary>
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

		/// <summary>
		/// Attempt to release our camera from our control while we are being destroyed.
		/// </summary>
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

		/// <summary>
		/// INPUT HANDLER: Hold down the trigger of our weapon.
		/// </summary>
		private void INPUT_WeaponFireHold()
		{
			if (playerRoot.weapon != null)
				playerRoot.weapon.FireWeaponHold();
		}

		/// <summary>
		/// INPUT HANDLER: Release the trigger of our weapon.
		/// </summary>
		private void INPUT_WeaponFireUp()
		{
			if (playerRoot.weapon != null)
				playerRoot.weapon.FireWeaponUp();
		}

		/// <summary>
		/// INPUT HANDLER: Reload our current weapon.
		/// </summary>
		private void INPUT_WeaponReload()
		{
			if (playerRoot.weapon != null)
				playerRoot.weapon.Reload();
		}

		/// <summary>
		/// INPUT HANDLER: Pressed down the trigger of our magnet arm.
		/// </summary>
		private void INPUT_MagnetArmDown()
		{
			// Call this if we have something in our hand
			//playerRoot.CmdActivateInteract(playerRoot.eye.position, playerRoot.eye.forward);
		}

		/// <summary>
		/// INPUT HANDLER: Hold down the trigger of our magnet arm.
		/// </summary>
		private void INPUT_MagnetArmHeld()
		{
			if (inAimDownSightsMode)
				return;

			playerRoot.magnetArm.FireHeld();
		}

		/// <summary>
		/// INPUT HANDLER: Release the trigger of our magnet arm.
		/// </summary>
		private void INPUT_MagnetArmUp()
		{
			playerRoot.magnetArm.FireUp();
		}
		
		/// <summary>
		/// INPUT HANDLER: Toggle the game's pause menu.
		/// </summary>
		private void INPUT_TogglePause()
		{
			if (inAimDownSightsMode)
				return;

			EventManager.Local.TogglePause();
		}

		/// <summary>
		/// INPUT HANDLER: Enter aim down sights mode.
		/// </summary>
		private void INPUT_EnterAimDownSights()
		{
			inAimDownSightsMode = true;

			if (playerRoot.weapon != null)
				playerRoot.weapon.EnterAimDownSightsMode();
		}

		/// <summary>
		/// INPUT HANDLER: End aim down sights mode.
		/// </summary>
		private void INPUT_ExitAimDownSights()
		{
			inAimDownSightsMode = false;

			if (playerRoot.weapon != null)
				playerRoot.weapon.ExitAimDownSightsMode();
		}

		/// <summary>
		/// INPUT HANDLER: Immediately reset the gun's position.
		/// TODO: Remove this when this bug is actually fixed.
		/// </summary>
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
		
		/// <summary>
		/// EVENT HANDLER: Local.OnApplyOptionsData
		/// </summary>
		private void OnApplyOptionsData(IOptionsData data)
		{
			// set the volume on FMOD's master mixer bus.
			FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(data.masterVolume);
			mCameraRef.fieldOfView = data.fieldOfView;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerDied
		/// Starts the respawn timer and then sends this player back to their start position when time is up.
		/// </summary>
		private void OnLocalPlayerDied(Vector3 spawnPosition, Quaternion spawnRotation, ICharacter killer)
		{
			// Disable input (because we're dead).
			ServiceLocator.Get<IInput>()
				.DisableInputLevel(InputLevel.Gameplay)
				.DisableInputLevel(InputLevel.PauseMenu);

			INPUT_ExitAimDownSights(); // force an ADS exit

			// TODO: Do a cool thing with the camera here?
			// For now, just leave it where it was when we died. We'll grab it again when we respawn.
			mCameraRef.transform.SetParent(null);

			if (killer != null && !ReferenceEquals(killer, playerRoot))
			{
				StartCoroutine(Coroutines.LerpRotation(mCameraRef.transform,
					Quaternion.LookRotation(killer.transform.position - mCameraRef.transform.position, Vector3.up), 0.75f));
			}

			// Move us way out of the level.
			playerRoot.transform.position = Vector3.one * -5000.0f;

			// Make the screen red.
			Vignette temporaryVignette = SetupDeathVignette();
			PostProcessVolume volume = PostProcessManager.instance.QuickVolume(LayerMask.NameToLayer("postprocessing"), 100, temporaryVignette);
			volume.weight = 1.0f;

			// InvokeEveryTick: A "temporary" update function:
			StartCoroutine(Coroutines.InvokeEveryTick(time =>
			{
				if (time < playerRoot.defaultData.respawnTime)
				{
					// Update the respawn UI timer.
					mRespawnTimer.value = Mathf.Ceil(playerRoot.defaultData.respawnTime - time);
					return true; // signal to continue this coroutine
				}

				// Time is up!
				mRespawnTimer.value = 0.0f;

				// Re-enable our input
				ServiceLocator.Get<IInput>()
					.EnableInputLevel(InputLevel.Gameplay)
					.EnableInputLevel(InputLevel.PauseMenu);

				// Send is back to the spawn position the server chose for us.
				playerRoot.ResetPlayerValues(spawnPosition, spawnRotation);
				mCameraRef.transform.SetParent(playerRoot.eye, false);
				mCameraRef.transform.ResetLocalValues();

				// Destroy the red "death" effect.
				RuntimeUtilities.DestroyVolume(volume, false);
				Destroy(temporaryVignette);

				return false; // signal to end this coroutine
			}));
		}

		/// <summary>
		/// Create a Vignette to indicate on-screen that we are dead.
		/// </summary>
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
