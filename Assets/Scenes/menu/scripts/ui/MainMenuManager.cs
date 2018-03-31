using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Core.State;
using KeatsLib.Unity;
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
		[SerializeField] private UIButton mGoToCreditsButton;
		[SerializeField] private UIButton mCreditsReturnButton;
		[SerializeField] private int mKioskTimerSeconds = 10;
		[SerializeField] private Animator mMainMenuAnimator;
		[SerializeField] private Animator mCreditsAnimator;

		/// Private variables
		private int mKioskTimerTicks;
		private IAudioReference mMenuMusic;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			mFourPlayerButton.onClick.AddListener(LaunchNewLevel);
			mQuitButton.onClick.AddListener(ClickQuit);
			mGunGlossaryButton.onClick.AddListener(LaunchGlossary);
			mHowToPlayButton.onClick.AddListener(LaunchHowToPlay);
			mGoToCreditsButton.onClick.AddListener(LaunchCredits);
			mCreditsReturnButton.onClick.AddListener(ReturnToMenu);

			// Ensure everything is enabled
			mMainMenuAnimator.gameObject.SetActive(true);
			mCreditsAnimator.gameObject.SetActive(true);


			StartCoroutine(Coroutines.InvokeAfterSeconds(0.1f, () =>
			{
				mMainMenuAnimator.SetTrigger("Enter");
				mMainMenuAnimator.transform.SetAsLastSibling();
			}));

			mKioskTimerTicks = mKioskTimerSeconds * 100;

			// play music
			IAudioManager audioService = ServiceLocator.Get<IAudioManager>();
			mMenuMusic = audioService.CheckReferenceAlive(ref mMenuMusic);

			if (mMenuMusic == null)
			{
				mMenuMusic = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.MenuMusic, gameObject.transform, false);
				mMenuMusic.Start();
			}
		}

		/// <summary>
		/// Launch the four/five/six/whatever player game.
		/// </summary>
		private void LaunchNewLevel()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.DRAFT_GAMEPLAY);

			mMenuMusic.Kill(true);
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
		/// Hide the menu buttons & display the credits
		/// </summary>
		private void LaunchCredits()
		{
			mMainMenuAnimator.SetTrigger("Exit");
			mCreditsAnimator.SetTrigger("Enter");
			mCreditsAnimator.transform.SetAsLastSibling();
		}

		/// <summary>
		/// Hide all other menu assets and return to the main menu
		/// </summary>
		private void ReturnToMenu()
		{
			mCreditsAnimator.SetTrigger("Exit");
			mMainMenuAnimator.SetTrigger("Enter");
			mMainMenuAnimator.transform.SetAsLastSibling();
		}

		/// <summary>
		/// Launches kiosk mode
		/// </summary>
		private void LaunchKiosk()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.KIOSK_SCENE);

			mMenuMusic.Kill(false);
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
