using FiringSquad.Core;
using FiringSquad.Core.State;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Main menu UI manager.
	/// </summary>
	public class MainMenuManager : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private GameObject mMainElementHolder;
		[SerializeField] private ActionProvider mTwoPlayerButton;
		[SerializeField] private ActionProvider mFourPlayerButton;
		[SerializeField] private ActionProvider mQuitButton;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			mTwoPlayerButton.OnClick += LaunchTwoPlayer;
			mFourPlayerButton.OnClick += LaunchFourPlayer;
			mQuitButton.OnClick += ClickQuit;
		}

		/// <summary>
		/// Launch the two player game.
		/// </summary>
		private void LaunchTwoPlayer()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.TWOPLAYER_WORLD)
				.RequestSceneChange(GamestateManager.TWOPLAYER_GAMEPLAY, LoadSceneMode.Additive);
		}

		/// <summary>
		/// Launch the four/five/six/whatever player game.
		/// </summary>
		private void LaunchFourPlayer()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.FOURPLAYER_GAMEPLAY);
		}

		/// <summary>
		/// Handle the player clicking the quit button.
		/// </summary>
		private void ClickQuit()
		{
			ServiceLocator.Get<IGamestateManager>()
				.RequestShutdown();
		}
	}
}
