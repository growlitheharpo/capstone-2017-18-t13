using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Core.UI;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to manage the pause panel.
	/// </summary>
	public class PauseGamePanel : MonoBehaviour, IScreenPanel
	{
		/// Inspector variables
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

		/// Private variables
		private IOptionsData mData;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Start()
		{
			mData = OptionsData.GetInstance();

			mFirstQuitButton.OnClick += HandleFirstQuit;
			mResumeButton.OnClick += HandleResume;
			mConfirmQuitButton.OnClick += HandleQuit;
			mCancelQuitButton.OnClick += HandleCancelQuit;

			mFieldOfViewProvider.OnValueChange += HandleValueChange;
			mVolumeProvider.OnValueChange += HandleValueChange;

			EventManager.Local.OnApplyOptionsData += OnApplyOptionsData;

			mConfirmationPanel.SetActive(false);
			ServiceLocator.Get<IUIManager>()
				.RegisterPanel(this, ScreenPanelTypes.Pause);
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			mFirstQuitButton.OnClick -= HandleFirstQuit;
			mResumeButton.OnClick -= HandleResume;
			mConfirmQuitButton.OnClick -= HandleQuit;
			mCancelQuitButton.OnClick -= HandleCancelQuit;
			mFieldOfViewProvider.OnValueChange -= HandleValueChange;

			EventManager.Local.OnApplyOptionsData -= OnApplyOptionsData;

			ServiceLocator.Get<IUIManager>()
				.UnregisterPanel(this);
		}

		/// <summary>
		/// Unity's OnDisable function.
		/// </summary>
		private void OnDisable()
		{
			ApplySettings();
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnApplyOptionsData
		/// </summary>
		private void OnApplyOptionsData(IOptionsData data)
		{
			mData = data;
			mVolumeProvider.SetValue(mData.masterVolume);
			mFieldOfViewProvider.SetValue(mData.fieldOfView);
			mMouseSensitivityProvider.SetValue(mData.mouseSensitivity);
		}

		/// <summary>
		/// Handles any of our values changing.
		/// </summary>
		/// <param name="v"></param>
		private void HandleValueChange(float v)
		{
			ApplySettings();
		}

		/// <summary>
		/// Grab all of our current values and notify the game to change them.
		/// </summary>
		private void ApplySettings()
		{
			mData.fieldOfView = mFieldOfViewProvider.GetValue();
			mData.masterVolume = mVolumeProvider.GetValue();
			mData.mouseSensitivity = mMouseSensitivityProvider.GetValue();

			EventManager.Notify(() => EventManager.Local.ApplyOptionsData(mData));
		}

		/// <summary>
		/// Handle the player clicking the "quit" button.
		/// </summary>
		private void HandleQuit()
		{
			// Call event directly so that it is handled immediately.
			EventManager.Local.TogglePause();

			EventManager.Notify(EventManager.Local.ConfirmQuitGame);
		}

		/// <summary>
		/// Handle the player resuming the game.
		/// </summary>
		private void HandleResume()
		{
			EventManager.Notify(EventManager.Local.TogglePause);
		}

		/// <summary>
		/// Handle the player clicking the first "quit" button and ask to confirm.
		/// </summary>
		private void HandleFirstQuit()
		{
			mConfirmationPanel.SetActive(true);
		}

		/// <summary>
		/// Handle the player cancelling after clicking the first "quit" button.
		/// </summary>
		private void HandleCancelQuit()
		{
			mConfirmationPanel.SetActive(false);
		}

		/// <inheritdoc />
		public void OnEnablePanel() { }

		/// <inheritdoc />
		public void OnDisablePanel() { }
	}
}
