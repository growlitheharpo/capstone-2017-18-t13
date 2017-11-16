using FiringSquad.Core;
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

		/// Private variables
		private CltPlayer mPlayerRef;

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
		/// Set the player that this panel will change.
		/// </summary>
		/// <param name="obj"></param>
		public void SetPlayer(CltPlayer obj)
		{
			mPlayerRef = obj;
		}

		/// <summary>
		/// Handle the player confirming their name.
		/// Hide the panel and re-enable input.
		/// </summary>
		private void ConfirmName()
		{
			mPlayerRef.CmdSetPlayerName(mInputField.text);
			ServiceLocator.Get<IUIManager>().PopPanel(ScreenPanelTypes.PlayerNameEntry);
			Destroy(gameObject);
		}
	}
}
