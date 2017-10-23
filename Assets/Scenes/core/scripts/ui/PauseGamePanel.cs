using FiringSquad.Core;
using FiringSquad.Core.SaveLoad;
using FiringSquad.Core.State;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	public class PauseGamePanel : MonoBehaviour
	{
		public const string SETTINGS_ID = "options_menu_options_id";

		private IOptionsData mData;
		[SerializeField] private float mDefaultFieldOfView;
		[SerializeField] private float mDefaultMouseSensitivity;

		[SerializeField] private float mDefaultVolume;
		[SerializeField] private BaseFloatProvider mFieldOfViewProvider;
		[SerializeField] private BaseFloatProvider mMouseSensitivityProvider;
		[SerializeField] private BaseFloatProvider mVolumeProvider;

		[SerializeField] private ActionProvider mFirstQuitButton;
		[SerializeField] private ActionProvider mResumeButton;
		[SerializeField] private ActionProvider mConfirmQuitButton;
		[SerializeField] private ActionProvider mCancelQuitButton;

		[SerializeField] private GameObject mConfirmationPanel;

		private void Awake()
		{
			EventManager.OnInitialPersistenceLoadComplete += HandleInitialLoad;
			EventManager.Local.OnApplyOptionsData += ReflectSettings;

			mFirstQuitButton.OnClick += HandleFirstQuit;
			mResumeButton.OnClick += HandleResume;
			mConfirmQuitButton.OnClick += HandleQuit;
			mCancelQuitButton.OnClick += HandleCancelQuit;

			mFieldOfViewProvider.OnValueChange += HandleValueChange;

			EventManager.LocalGUI.OnTogglePauseMenu += HandleToggle;
			mConfirmationPanel.SetActive(false);
			gameObject.SetActive(false);
		}

		private void Start()
		{
			HandleInitialLoad();
		}

		private void OnDestroy()
		{
			mFirstQuitButton.OnClick -= HandleFirstQuit;
			mResumeButton.OnClick -= HandleResume;
			mConfirmQuitButton.OnClick -= HandleQuit;
			mCancelQuitButton.OnClick -= HandleCancelQuit;
			mFieldOfViewProvider.OnValueChange -= HandleValueChange;

			EventManager.OnInitialPersistenceLoadComplete -= HandleInitialLoad;
			EventManager.Local.OnApplyOptionsData -= ReflectSettings;
			EventManager.LocalGUI.OnTogglePauseMenu -= HandleToggle;
		}

		private void HandleToggle(bool show)
		{
			mConfirmationPanel.SetActive(false);
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
			mData.masterVolume = mVolumeProvider.GetValue();
			mData.mouseSensitivity = mMouseSensitivityProvider.GetValue();

			EventManager.Notify(() => EventManager.Local.ApplyOptionsData(mData));
		}

		private void HandleQuit()
		{
			// Call event directly so that it is handled immediately.
			EventManager.Local.TogglePause();

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.MENU_SCENE);
		}

		private void HandleResume()
		{
			EventManager.Notify(EventManager.Local.TogglePause);
		}

		private void HandleFirstQuit()
		{
			mConfirmationPanel.SetActive(true);
		}

		private void HandleCancelQuit()
		{
			mConfirmationPanel.SetActive(false);
		}
	}
}
