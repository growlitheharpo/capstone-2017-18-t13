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
		[SerializeField] private ActionProvider mGunGlossaryButton;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			mTwoPlayerButton.OnClick += LaunchOldLevel;
			mFourPlayerButton.OnClick += LaunchNewLevel;
			mQuitButton.OnClick += ClickQuit;
			mGunGlossaryButton.OnClick += LaunchGlossary;

		}

		/// <summary>
		/// Launch the two player game.
		/// </summary>
		private void LaunchOldLevel()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.FOURPLAYER_GAMEPLAY);
		}

		/// <summary>
		/// Launch the four/five/six/whatever player game.
		/// </summary>
		private void LaunchNewLevel()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.DRAFT_GAMEPLAY);
		}

		/// <summary>
		/// Handle the player clicking the quit button.
		/// </summary>
		private void ClickQuit()
		{
			ServiceLocator.Get<IGamestateManager>()
				.RequestShutdown();
		}

		/// <summary>
		/// Launch the gun glossary scene
		/// </summary>
		private void LaunchGlossary()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.GUN_GLOSSARY);
		}
	}
}
