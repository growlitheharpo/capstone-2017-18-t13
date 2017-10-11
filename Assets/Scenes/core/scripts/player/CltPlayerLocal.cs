using FiringSquad.Data;
using UnityEngine;
using Input = UnityEngine.Input;
using InputLevel = KeatsLib.Unity.Input.InputLevel;

public class CltPlayerLocal : MonoBehaviour
{
	[SerializeField] private PlayerInputMap mInputMap;
	public PlayerInputMap inputMap { get { return mInputMap; } }

	[SerializeField] private GameObject mCameraPrefab;
	[SerializeField] private PlayerDefaultsData mInformation;

	private CltPlayer playerRoot { get; set; }
	private Transform eye { get { return playerRoot.eye; } }

	// Use this for initialization
	private void Start()
	{
		ServiceLocator.Get<IInput>()
			.RegisterInput(Input.GetButton, inputMap.fireWeaponButton, playerRoot.WeaponFireHold, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonUp, inputMap.fireWeaponButton, playerRoot.WeaponFireUp, InputLevel.Gameplay)
			.RegisterInput(Input.GetButtonDown, inputMap.reloadButton, playerRoot.WeaponReload, InputLevel.Gameplay)

			.RegisterInput(Input.GetButtonDown, inputMap.interactButton, INPUT_ActivateInteract, InputLevel.Gameplay)
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

			.UnregisterInput(INPUT_ActivateInteract)
			.UnregisterInput(INPUT_TogglePause);

		EventManager.Local.OnApplyOptionsData -= ApplyOptionsData;
		CleanupUI();
		CleanupCamera();
	}
}
