using FiringSquad.Core;
using FiringSquad.Core.State;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	public class GameOverPanel : MonoBehaviour
	{
		[SerializeField] private UIText mWhoWinsText;
		[SerializeField] private ActionProvider mRestartButton;
		[SerializeField] private ActionProvider mQuitButton;

		private void Start()
		{
			EventManager.LocalGUI.OnShowGameoverPanel += HandleGameover;

			mRestartButton.OnClick += HandleRestart;
			mQuitButton.OnClick += HandleQuit;

			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			EventManager.LocalGUI.OnShowGameoverPanel -= HandleGameover;
			mRestartButton.OnClick -= HandleRestart;
			mQuitButton.OnClick -= HandleQuit;
		}

		private void HandleRestart()
		{
			//EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.BASE_WORLD));
			//EventManager.Notify(() => EventManager.RequestSceneChange(SceneManager.GetActiveScene().name, LoadSceneMode.Additive));
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
