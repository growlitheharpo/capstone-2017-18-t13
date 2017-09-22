using UnityEngine;
using UnityEngine.SceneManagement;
using UIText = UnityEngine.UI.Text;

public class ArenaGameOverPanel : MonoBehaviour
{
	[SerializeField] private UIText mWhoWinsText;
	[SerializeField] private ActionProvider mRestartButton;
	[SerializeField] private ActionProvider mQuitButton;

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
		EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.BASE_WORLD));
		EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.PROTOTYPE3_SCENE, LoadSceneMode.Additive));
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
