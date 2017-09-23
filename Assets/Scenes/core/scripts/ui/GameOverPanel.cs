using UnityEngine;
using UnityEngine.SceneManagement;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay
{
	public class GameOverPanel : MonoBehaviour
	{
		[SerializeField] private UIText mWhoWinsText;
		[SerializeField] private ActionProvider mRestartButton;
		[SerializeField] private ActionProvider mQuitButton;
		[SerializeField] private string mOverrideRestartSceneName = "";

		private void Start()
		{
			EventManager.OnShowGameoverPanel += HandleGameover;

			mRestartButton.OnClick += HandleRestart;
			mQuitButton.OnClick += HandleQuit;

			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			EventManager.OnShowGameoverPanel -= HandleGameover;
			mRestartButton.OnClick -= HandleRestart;
			mQuitButton.OnClick -= HandleQuit;
		}

		private void HandleRestart()
		{
			if (string.IsNullOrEmpty(mOverrideRestartSceneName))
			{
				EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.BASE_WORLD));
				EventManager.Notify(() => EventManager.RequestSceneChange(SceneManager.GetActiveScene().name, LoadSceneMode.Additive));
			}
			else
			{
				EventManager.Notify(() => EventManager.RequestSceneChange(mOverrideRestartSceneName));
			}
		}

		private void HandleQuit()
		{
			EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.MENU_SCENE));
		}

		private void HandleGameover(string whoWins)
		{
			gameObject.SetActive(true);
			mWhoWinsText.text = whoWins;
		}
	}
}
