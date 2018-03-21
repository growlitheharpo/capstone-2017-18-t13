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
		[SerializeField] private UIButton mHowToPlayButton;
		[SerializeField] private int mKioskTimerSeconds = 10;
		private int mKioskTimerTicks;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{ 
			mFourPlayerButton.onClick.AddListener(LaunchNewLevel);
			mQuitButton.onClick.AddListener(ClickQuit);
			mGunGlossaryButton.onClick.AddListener(LaunchGlossary);
			mHowToPlayButton.onClick.AddListener(LaunchHowToPlay);

			mKioskTimerTicks = mKioskTimerSeconds * 100;
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
		private void LaunchHowToPlay()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.HOW_TO_PLAY);
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

		/// <summary>
		/// Launches kiosk mode
		/// </summary>
		private void LaunchKiosk()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.KIOSK_SCENE);
		}

		/// <summary>
		/// Unity's fixed update function
		/// </summary>
		private void FixedUpdate()
		{
			// Decrement kiosk timer
			mKioskTimerTicks--;

			// If people press any key, reset the timer
			if (Input.anyKeyDown)
			{
				mKioskTimerTicks = mKioskTimerSeconds * 100;
			}

			// If the timer gets below 0, go to kiosk mode scene
			if (mKioskTimerTicks <= 0)
			{
				LaunchKiosk();
			}
		}
	}
}
