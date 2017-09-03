using UnityEngine;

/// <summary>
/// Main menu UI manager.
/// </summary>
public class UIManager : MonoBehaviour
{
	[SerializeField] private GameObject mMainElementHolder;
	[SerializeField] private ActionProvider mPlayButton;
	[SerializeField] private ActionProvider mOptionsButton;
	[SerializeField] private ActionProvider mQuitButton;

	private void Start()
	{
		mPlayButton.OnClick += ClickPlay;
		mOptionsButton.OnClick += ClickOptions;
		mQuitButton.OnClick += ClickQuit;
	}

	private void ClickPlay()
	{
		mMainElementHolder.SetActive(false);
		EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.GAME_SCENE));
	}

	private void ClickOptions()
	{
		
	}

	private void ClickQuit()
	{
		ServiceLocator.Get<IGamestateManager>()
			.RequestShutdown();
	}
}
