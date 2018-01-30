using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to manage the player's name customization panel.
	/// </summary>
	public class PlayerNameCustomizationPanel : MonoBehaviour, IScreenPanel
	{
		/// Inspector variables
		[SerializeField] private InputField mInputField;
		[SerializeField] private ActionProvider mConfirmButton;

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			mConfirmButton.OnClick += ConfirmName;
			
			ServiceLocator.Get<IUIManager>()
				.RegisterPanel(this, ScreenPanelTypes.PlayerNameEntry);
		}

		/// <summary>
		/// Cleanup listeners and event handlers
		/// </summary>
		private void OnDestroy()
		{
			mConfirmButton.OnClick -= ConfirmName;

			ServiceLocator.Get<IUIManager>()
				.UnregisterPanel(this);
		}

		/// <summary>
		/// Handle the player confirming their name.
		/// Hide the panel and re-enable input.
		/// </summary>
		private void ConfirmName()
		{
			ServiceLocator.Get<IUIManager>()
				.PopPanel(ScreenPanelTypes.PlayerNameEntry)
				.PushNewPanel(ScreenPanelTypes.HandleConnection);

			ServiceLocator.Get<IGamestateManager>().currentUserName = mInputField.text;
		}
	}
}
