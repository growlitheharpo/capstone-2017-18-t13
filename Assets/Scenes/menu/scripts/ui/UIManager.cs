using FiringSquad.Core;
using FiringSquad.Core.State;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Main menu UI manager.
	/// </summary>
	public class UIManager : MonoBehaviour
	{
		[SerializeField] private GameObject mMainElementHolder;
		[SerializeField] private ActionProvider mTwoPlayerButton;
		[SerializeField] private ActionProvider mFourPlayerButton;
		[SerializeField] private ActionProvider mQuitButton;

		private void Start()
		{
			mTwoPlayerButton.OnClick += LaunchTwoPlayer;
			mFourPlayerButton.OnClick += LaunchFourPlayer;
			mQuitButton.OnClick += ClickQuit;
		}

		private void LaunchTwoPlayer()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.TWOPLAYER_WORLD)
				.RequestSceneChange(GamestateManager.TWOPLAYER_GAMEPLAY, LoadSceneMode.Additive);
		}

		private void LaunchFourPlayer()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.FOURPLAYER_GAMEPLAY);
		}

		private void ClickQuit()
		{
			ServiceLocator.Get<IGamestateManager>()
				.RequestShutdown();
		}
	}
}
