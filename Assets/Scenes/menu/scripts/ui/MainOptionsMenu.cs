using UnityEngine;

/// <summary>
/// Class to handle the main options of the game.
/// </summary>
public class MainOptionsMenu : MonoBehaviour
{
	private const string SETTINGS_ID = "options_menu_options_id";

	private IOptionsData mData;
	[SerializeField] private float mDefaultFieldOfView;
	[SerializeField] private float mDefaultMouseSensitivity;

	[SerializeField] private float mDefaultVolume;
	[SerializeField] private BaseFloatProvider mFieldOfViewProvider;
	[SerializeField] private BaseFloatProvider mMouseSensitivityProvider;

	[SerializeField] private ActionProvider mOnApplyButton;
	[SerializeField] private BaseFloatProvider mVolumeProvider;

	private void Awake()
	{
		EventManager.OnInitialPersistenceLoadComplete += HandleInitialLoad;
		EventManager.OnApplyOptionsData += ReflectSettings;
		mOnApplyButton.OnClick += ApplySettings;
	}

	private void OnDestroy()
	{
		EventManager.OnInitialPersistenceLoadComplete -= HandleInitialLoad;
		EventManager.OnApplyOptionsData -= ReflectSettings;
	}

	private void HandleInitialLoad()
	{
		mData = SaveLoadManager.instance.persistentData.GetOptionsData(SETTINGS_ID);
		if (mData == null)
		{
			mData = SaveLoadManager.instance.persistentData.CreateOptionsData(SETTINGS_ID);
			mData.masterVolume = mDefaultVolume;
			mData.fieldOfView = mDefaultFieldOfView;
			mData.mouseSensitivity = mDefaultMouseSensitivity;
		}

		ReflectSettings(mData);
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
}
