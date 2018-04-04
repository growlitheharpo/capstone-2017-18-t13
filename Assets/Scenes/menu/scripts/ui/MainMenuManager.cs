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
		[Header("Main Buttons")]
		[SerializeField]
		private UIButton mSubPlayButton;
		[SerializeField] private UIButton mGunGlossaryButton;
		[SerializeField] private UIButton mHowToPlayButton;
		[SerializeField] private UIButton mCreditsButton;
		[SerializeField] private UIButton mQuitButton;

		[Header("Play Game Buttons")]
		[SerializeField]
		private UIButton mArenaBattleButton;
		[SerializeField] private UIButton mDualModeButton;

		[Header("Return Buttons")]
		[SerializeField]
		private UIButton mSubPlayReturnButton;
		[SerializeField] private UIButton mGunGlossaryReturnButton;
		[SerializeField] private UIButton mHowToPlayReturnButton;
		[SerializeField] private UIButton mCreditsReturnButton;

		[Header("Section Animators")]
		[SerializeField]
		private Animator mMainMenuAnimator;
		[SerializeField] private Animator mSubPlayAnimator;
		[SerializeField] private Animator mGunGlossaryAnimator;
		[SerializeField] private Animator mHowToPlayAnimator;
		[SerializeField] private Animator mCreditsAnimator;

		[Header("Other")]
		[SerializeField]
		private float mKioskTimerLength = 30;

		/// Private variables
		private float mKioskTimer;
		private IAudioReference mMenuMusic;

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			mSubPlayButton.onClick.AddListener(SwitchTo_SubPlay);
			mGunGlossaryButton.onClick.AddListener(SwitchTo_GunGlossary);
			mHowToPlayButton.onClick.AddListener(SwitchTo_HowToPlay);
			mCreditsButton.onClick.AddListener(SwitchTo_Credits);
			mQuitButton.onClick.AddListener(ClickQuit);
			mArenaBattleButton.onClick.AddListener(Launch_ArenaBattle);
			mDualModeButton.onClick.AddListener(Launch_DualMode);

			mSubPlayReturnButton.onClick.AddListener(() => ReturnToMainMenu(mSubPlayAnimator));
			mGunGlossaryReturnButton.onClick.AddListener(() =>
			{
				mGunGlossaryAnimator.GetComponent<GlossaryMenuManager>().ResetToDefault(false);
				ReturnToMainMenu(mGunGlossaryAnimator);
			});
			mHowToPlayReturnButton.onClick.AddListener(() => ReturnToMainMenu(mHowToPlayAnimator));
			mCreditsReturnButton.onClick.AddListener(() => ReturnToMainMenu(mCreditsAnimator));

			// Ensure everything is enabled
			mMainMenuAnimator.gameObject.SetActive(true);
			mSubPlayAnimator.gameObject.SetActive(true);
			mGunGlossaryAnimator.gameObject.SetActive(true);
			mHowToPlayAnimator.gameObject.SetActive(true);
			mCreditsAnimator.gameObject.SetActive(true);

			StartCoroutine(Coroutines.InvokeAfterSeconds(0.1f, () =>
			{
				mMainMenuAnimator.SetTrigger("Enter");
				mMainMenuAnimator.transform.SetAsLastSibling();
			}));

			mKioskTimer = mKioskTimerLength;

			// play music
			IAudioManager audioService = ServiceLocator.Get<IAudioManager>();
			mMenuMusic = audioService.CheckReferenceAlive(ref mMenuMusic);

			if (mMenuMusic == null)
			{
				mMenuMusic = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.MenuMusic, gameObject.transform, false);
				mMenuMusic.Start();
			}
		}

		#region Section Swapping

		/// <summary>
		/// Hide the menu buttons & display the sub play menu
		/// </summary>
		private void SwitchTo_SubPlay()
		{
			mMainMenuAnimator.SetTrigger("Exit");
			mSubPlayAnimator.SetTrigger("Enter");
			mSubPlayAnimator.transform.SetAsLastSibling();
		}

		/// <summary>
		/// Hide the menu buttons & display the gun glossary
		/// </summary>
		private void SwitchTo_GunGlossary()
		{
			mMainMenuAnimator.SetTrigger("Exit");
			mGunGlossaryAnimator.GetComponent<GlossaryMenuManager>().ResetToDefault();
			mGunGlossaryAnimator.SetTrigger("Enter");
			mGunGlossaryAnimator.transform.SetAsLastSibling();
		}

		/// <summary>
		/// Hide the menu buttons & display the gun glossary
		/// </summary>
		private void SwitchTo_HowToPlay()
		{
			mMainMenuAnimator.SetTrigger("Exit");
			mHowToPlayAnimator.GetComponent<HowToPlayManager>().ResetEverything();
			mHowToPlayAnimator.SetTrigger("Enter");
			mHowToPlayAnimator.transform.SetAsLastSibling();
		}

		/// <summary>
		/// Hide the menu buttons & display the credits
		/// </summary>
		private void SwitchTo_Credits()
		{
			mMainMenuAnimator.SetTrigger("Exit");
			mCreditsAnimator.SetTrigger("Enter");
			mCreditsAnimator.transform.SetAsLastSibling();
		}

		/// <summary>
		/// Launch the arena mode.
		/// </summary>
		private void Launch_ArenaBattle()
		{
			mSubPlayAnimator.SetTrigger("Exit");
			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.DRAFT_GAMEPLAY);

			KillMusic ();
		}

		/// <summary>
		/// Launch the two player mode.
		/// </summary>
		private void Launch_DualMode()
		{
			mSubPlayAnimator.SetTrigger("Exit");
			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.DRAFT_DUALMODE);

			KillMusic ();
		}

		/// <summary>
		/// Hide all other menu assets and return to the main menu
		/// </summary>
		private void ReturnToMainMenu(Animator from)
		{
			from.SetTrigger("Exit");
			mMainMenuAnimator.SetTrigger("Enter");
			mMainMenuAnimator.transform.SetAsLastSibling();
		}

		#endregion

		/// <summary>
		/// Handle the player clicking the quit button.
		/// </summary>
		private void ClickQuit()
		{
			ServiceLocator.Get<IGamestateManager>()
				.RequestShutdown();
		}

		/// <summary>
		/// Launches kiosk mode
		/// </summary>
		private void LaunchKiosk()
		{
			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.KIOSK_SCENE);

			KillMusic ();
		}

		private void KillMusic()
		{
			mMenuMusic.Kill();
		}

		/// <summary>
		/// Unity's fixed update function
		/// </summary>
		private void FixedUpdate()
		{
			// Decrement kiosk timer
			mKioskTimer -= Time.deltaTime;

			// If people press any key or move the mouse, reset the timer
			if (Input.anyKeyDown || Mathf.Abs(Input.GetAxis("Mouse X")) > 0.0f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.0f)
				mKioskTimer = mKioskTimerLength;

			// If the timer gets below 0, go to kiosk mode scene
			if (mKioskTimer < 0.0f)
				LaunchKiosk();
		}
	}
}
