using FiringSquad.Data;
using FiringSquad.Gameplay;
using UnityEngine;
using Input = UnityEngine.Input;
using InputLevel = KeatsLib.Unity.Input.InputLevel;

public class CltPlayerLocal : MonoBehaviour
{
	[SerializeField] private PlayerInputMap mInputMap;
	public PlayerInputMap inputMap { get { return mInputMap; } }

	[SerializeField] private WeaponDefaultsData mDefaultParts;
	public WeaponDefaultsData defaultParts { get { return mDefaultParts; } }

	[SerializeField] private GameObject mCameraPrefab;
	[SerializeField] private PlayerDefaultsData mInformation;

	private CltPlayer playerRoot { get; set; }
	private CltPlayerWeaponLocal weapon { get { return playerRoot.weapon as CltPlayerWeaponLocal; } }
	private Transform eye { get { return playerRoot.eye; } }

	// Use this for initialization
	private void Start()
	{
		ServiceLocator.Get<IInput>()
			.RegisterInput(Input.GetButtonDown, inputMap.fireWeaponButton, weapon.FireWeaponDown, InputLevel.Gameplay)
			.RegisterInput(Input.GetButton, inputMap.fireWeaponButton, weapon.FireWeaponHold, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonUp, inputMap.fireWeaponButton, weapon.FireWeaponUp, InputLevel.Gameplay)

			.RegisterInput(Input.GetButtonDown, inputMap.reloadButton, weapon.Reload, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonDown, inputMap.interactButton, INPUT_ActivateInteract, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonDown, inputMap.pauseButton, INPUT_TogglePause, InputLevel.PauseMenu);

		SetupCamera();
		SetupUI();

		EventManager.Local.OnApplyOptionsData += ApplyOptionsData;
	}

	private void OnDestroy()
	{
		ServiceLocator.Get<IInput>()
			.UnregisterInput(weapon.FireWeaponDown)
			.UnregisterInput(weapon.FireWeaponHold)
			.UnregisterInput(weapon.FireWeaponUp)
			.UnregisterInput(weapon.Reload)

			.UnregisterInput(INPUT_ActivateInteract)
			.UnregisterInput(INPUT_TogglePause);

		EventManager.Local.OnApplyOptionsData -= ApplyOptionsData;
		CleanupUI();
		CleanupCamera();
	}
}
