using FiringSquad.Core;
using FiringSquad.Core.State;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	public class GameOverPanel : MonoBehaviour
	{
		[SerializeField] private UIText mWhoWinsText;
		[SerializeField] private ActionProvider mQuitButton;

		private void Start()
		{
			EventManager.LocalGUI.OnShowGameoverPanel += HandleGameover;
			mQuitButton.OnClick += HandleQuit;

			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			EventManager.LocalGUI.OnShowGameoverPanel -= HandleGameover;
			mQuitButton.OnClick -= HandleQuit;
		}

		private void HandleQuit()
		{
			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.MENU_SCENE);
		}

		private void HandleGameover(string whoWins)
		{
			gameObject.SetActive(true);
			mWhoWinsText.text = whoWins;
		}
	}
}
