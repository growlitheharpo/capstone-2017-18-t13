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
			.RegisterInput(Input.GetButton, inputMap.fireWeaponButton, INPUT_WeaponFireHold, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonUp, inputMap.fireWeaponButton, INPUT_WeaponFireUp, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonDown, inputMap.reloadButton, INPUT_WeaponReload, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonDown, inputMap.interactButton, INPUT_ActivateInteract, InputLevel.Gameplay)

			.RegisterInput(Input.GetButtonDown, inputMap.pauseButton, INPUT_TogglePause, InputLevel.PauseMenu);

		SetupCamera();
		SetupUI();

		EventManager.Local.OnApplyOptionsData += ApplyOptionsData;
	}

	private void OnDestroy()
	{
		ServiceLocator.Get<IInput>()
			.UnregisterInput(INPUT_WeaponFireHold)
			.UnregisterInput(INPUT_WeaponFireUp)
			.UnregisterInput(INPUT_WeaponReload)
			.UnregisterInput(INPUT_ActivateInteract)

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

	private void INPUT_WeaponFireHold()
	{
		playerRoot.CmdWeaponFireHold();
	}

	private void INPUT_WeaponFireUp()
	{
		playerRoot.CmdWeaponFireUp();
	}

	private void INPUT_WeaponReload()
	{
		playerRoot.CmdWeaponReload();
	}

	private void INPUT_ActivateInteract()
	{
		playerRoot.CmdActivateInteract();
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
