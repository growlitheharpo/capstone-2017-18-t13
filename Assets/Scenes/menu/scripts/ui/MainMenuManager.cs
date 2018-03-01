using FiringSquad.Core;
using FiringSquad.Core.State;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIButton = UnityEngine.UI.Button;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Main menu UI manager.
	/// </summary>
	public class MainMenuManager : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private GameObject mMainElementHolder;
		[SerializeField] private UIButton mFourPlayerButton;
		[SerializeField] private UIButton mQuitButton;
		[SerializeField] private UIButton mGunGlossaryButton;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			mFourPlayerButton.onClick.AddListener(LaunchNewLevel);
			mQuitButton.onClick.AddListener(ClickQuit);
			mGunGlossaryButton.onClick.AddListener(LaunchGlossary);
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
