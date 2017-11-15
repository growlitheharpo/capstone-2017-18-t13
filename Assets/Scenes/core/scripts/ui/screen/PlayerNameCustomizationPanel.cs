using FiringSquad.Core;
using FiringSquad.Core.Input;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to manage the player's name customization panel.
	/// </summary>
	public class PlayerNameCustomizationPanel : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private InputField mInputField;
		[SerializeField] private ActionProvider mConfirmButton;

		/// Private variables
		private CltPlayer mPlayerRef;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mConfirmButton.OnClick += ConfirmName;
			EventManager.LocalGUI.OnRequestNameChange += OnRequestNameChange;

			gameObject.SetActive(false);
		}

		/// <summary>
		/// Cleanup listeners and event handlers
		/// </summary>
		private void OnDestroy()
		{
			mConfirmButton.OnClick -= ConfirmName;
			EventManager.LocalGUI.OnRequestNameChange -= OnRequestNameChange;
		}

		/// <summary>
		/// EVENT HANDLER: LocalGUI.OnRequestNameChange
		/// Show the panel and disable input.
		/// </summary>
		private void OnRequestNameChange(CltPlayer obj)
		{
			mPlayerRef = obj;
			gameObject.SetActive(true);

			ServiceLocator.Get<IInput>()
				.DisableInputLevel(InputLevel.Gameplay)
				.DisableInputLevel(InputLevel.HideCursor)
				.DisableInputLevel(InputLevel.PauseMenu);
		}

		/// <summary>
		/// Handle the player confirming their name.
		/// Hide the panel and re-enable input.
		/// </summary>
		private void ConfirmName()
		{
			ServiceLocator.Get<IInput>()
				.EnableInputLevel(InputLevel.Gameplay)
				.EnableInputLevel(InputLevel.HideCursor)
				.EnableInputLevel(InputLevel.PauseMenu);

			UnityEngine.Debug.Log("Name: " + mInputField.text);
			mPlayerRef.CmdSetPlayerName(mInputField.text);
			Destroy(gameObject);
		}
	}
}
