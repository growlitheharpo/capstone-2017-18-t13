using System;
using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;
using Input = UnityEngine.Input;
using InputLevel = KeatsLib.Unity.Input.InputLevel;

public class CltPlayerLocal : MonoBehaviour
{
	[SerializeField] private PlayerInputMap mInputMap;
	public PlayerInputMap inputMap { get { return mInputMap; } }

	[SerializeField] private GameObject mCameraPrefab;

	public CltPlayer playerRoot { get; set; }

	// Use this for initialization
	private void Start()
	{
		ServiceLocator.Get<IInput>()
			.RegisterInput(Input.GetButton, inputMap.fireWeaponButton, playerRoot.CmdWeaponFireHold, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonUp, inputMap.fireWeaponButton, playerRoot.CmdWeaponFireUp, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonDown, inputMap.reloadButton, playerRoot.CmdWeaponReload, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonDown, inputMap.interactButton, playerRoot.CmdActivateInteract, InputLevel.Gameplay)

			.RegisterInput(Input.GetButtonDown, inputMap.pauseButton, INPUT_TogglePause, InputLevel.PauseMenu);

		SetupCamera();
		SetupUI();

		EventManager.Local.OnApplyOptionsData += ApplyOptionsData;
	}

	private void OnDestroy()
	{
		ServiceLocator.Get<IInput>()
			.UnregisterInput(playerRoot.CmdWeaponFireHold)
			.UnregisterInput(playerRoot.CmdWeaponFireUp)
			.UnregisterInput(playerRoot.CmdWeaponReload)
			.UnregisterInput(playerRoot.CmdActivateInteract)

			.UnregisterInput(INPUT_TogglePause);

		EventManager.Local.OnApplyOptionsData -= ApplyOptionsData;
		CleanupUI();
		CleanupCamera();
	}

	private void SetupCamera()
	{
		Camera c = (Camera.main ?? FindObjectOfType<Camera>()) ?? Instantiate(mCameraPrefab).GetComponent<Camera>();
		c.transform.SetParent(playerRoot.eye, false);
		c.transform.ResetLocalValues();
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
