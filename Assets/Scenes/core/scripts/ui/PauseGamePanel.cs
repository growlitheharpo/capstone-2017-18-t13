using FiringSquad.Gameplay;
using KeatsLib.Persistence;
using UnityEngine;

public class PauseGamePanel : MonoBehaviour
{
	private const string SETTINGS_ID = "options_menu_options_id";

	private IOptionsData mData;
	[SerializeField] private float mDefaultFieldOfView;
	[SerializeField] private float mDefaultMouseSensitivity;

	[SerializeField] private float mDefaultVolume;
	[SerializeField] private BaseFloatProvider mFieldOfViewProvider;
	[SerializeField] private BaseFloatProvider mMouseSensitivityProvider;
	[SerializeField] private BaseFloatProvider mVolumeProvider;

	[SerializeField] private ActionProvider mQuitButton;

	private void Awake()
	{
		EventManager.OnInitialPersistenceLoadComplete += HandleInitialLoad;
		EventManager.OnApplyOptionsData += ReflectSettings;

		mQuitButton.OnClick += HandleQuit;
		mFieldOfViewProvider.OnValueChange += HandleValueChange;

		EventManager.OnShowPausePanel += HandleToggle;
		gameObject.SetActive(false);
	}

	private void Start()
	{
		HandleInitialLoad();
	}

	private void OnDestroy()
	{
		mQuitButton.OnClick -= HandleQuit;
		mFieldOfViewProvider.OnValueChange -= HandleValueChange;

		EventManager.OnInitialPersistenceLoadComplete -= HandleInitialLoad;
		EventManager.OnApplyOptionsData -= ReflectSettings;
		EventManager.OnShowPausePanel -= HandleToggle;
	}
	
	private void HandleToggle(bool show)
	{
		gameObject.SetActive(show);
		
		if (!show)
			ApplySettings();
	}

	private void HandleInitialLoad()
	{
		if (mData != null)
			return;

		ISaveLoadManager service = ServiceLocator.Get<ISaveLoadManager>();
		mData = service.persistentData.GetOptionsData(SETTINGS_ID);

		if (mData == null)
		{
			mData = SaveLoadManager.instance.persistentData.CreateOptionsData(SETTINGS_ID);
			mData.masterVolume = mDefaultVolume;
			mData.fieldOfView = mDefaultFieldOfView;
			mData.mouseSensitivity = mDefaultMouseSensitivity;
		}

		ReflectSettings(mData);
	}
	
	private void HandleValueChange(float v)
	{
		ApplySettings();
	}

	public void ReflectSettings(IOptionsData data)
	{
		mData = data;
		mVolumeProvider.SetValue(mData.masterVolume);
		mFieldOfViewProvider.SetValue(mData.fieldOfView);
		mMouseSensitivityProvider.SetValue(mData.mouseSensitivity);
	}

	public void ApplySettings()
	{
		mData.fieldOfView = mFieldOfViewProvider.GetValue();
		mData.masterVolume = mVolumeProvider.GetValue() / 100.0f;
		mData.mouseSensitivity = mMouseSensitivityProvider.GetValue();

		EventManager.Notify(() => EventManager.ApplyOptionsData(mData));
	}
	
	private void HandleQuit()
	{
		EventManager.Notify(() => EventManager.TogglePauseState());
		EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.MENU_SCENE));
	}
}
