using System;
using FiringSquad.Data;
using UnityEngine;
using Input = UnityEngine.Input;
using InputLevel = KeatsLib.Unity.Input.InputLevel;

public class CltPlayerLocal : MonoBehaviour
{
	[SerializeField] private PlayerInputMap mInputMap;
	public PlayerInputMap inputMap { get { return mInputMap; } }

	[SerializeField] private GameObject mCameraPrefab;

	private CltPlayer playerRoot { get; set; }

	// Use this for initialization
	private void Start()
	{
		ServiceLocator.Get<IInput>()
			.RegisterInput(Input.GetButton, inputMap.fireWeaponButton, playerRoot.WeaponFireHold, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonUp, inputMap.fireWeaponButton, playerRoot.WeaponFireUp, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonDown, inputMap.reloadButton, playerRoot.WeaponReload, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonDown, inputMap.interactButton, playerRoot.ActivateInteract, InputLevel.Gameplay)

			.RegisterInput(Input.GetButtonDown, inputMap.pauseButton, INPUT_TogglePause, InputLevel.PauseMenu);

		SetupCamera();
		SetupUI();

		EventManager.Local.OnApplyOptionsData += ApplyOptionsData;
	}

	private void OnDestroy()
	{
		ServiceLocator.Get<IInput>()
			.UnregisterInput(playerRoot.WeaponFireHold)
			.UnregisterInput(playerRoot.WeaponFireUp)
			.UnregisterInput(playerRoot.WeaponReload)
			.UnregisterInput(playerRoot.ActivateInteract)

			.UnregisterInput(INPUT_TogglePause);

		EventManager.Local.OnApplyOptionsData -= ApplyOptionsData;
		CleanupUI();
		CleanupCamera();
	}

	private void SetupCamera()
	{
	}

	private void SetupUI()
	{
	}

	private void CleanupCamera()
	{
	}

	private void CleanupUI()
	{
	}

	private void INPUT_TogglePause()
	{
		EventManager.Local.TogglePause();
	}
	
	private void ApplyOptionsData(IOptionsData data)
	{
		AudioListener.volume = data.masterVolume;
		// TODO: Apply camera FOV
	}
}
