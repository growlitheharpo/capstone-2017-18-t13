using FiringSquad.Core;
using FiringSquad.Core.State;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIButton = UnityEngine.UI.Button;
using UnityEngine.UI;


namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// How to play menu slide manager
	/// </summary>
	public class HowToPlayManager : MonoBehaviour
	{
		[SerializeField] private GameObject mMainElementHolder;

		[SerializeField] int mNumSlides = 5;

		[SerializeField] Sprite mSlide1;
		[SerializeField] Sprite mSlide2;
		[SerializeField] Sprite mSlide3;
		[SerializeField] Sprite mSlide4;
		[SerializeField] Sprite mSlide5;

		[SerializeField] GameObject mSlideObject;

		// Buttons for changing slide, going to the main menu
		[SerializeField] Button mNextSlide;
		[SerializeField] Button mPrevSlide;
		[SerializeField] Button mMainMenu;

		int mCurrentSlide = 1;

		// Use this for initialization
		void Start()
		{
			mCurrentSlide = 1;
			mPrevSlide.gameObject.SetActive(false);

			mMainMenu.onClick.AddListener(BackToMainMenu);
			mPrevSlide.onClick.AddListener(PrevSlide);
			mNextSlide.onClick.AddListener(NextSlide);
		}

		/// <summary>
		/// Return the user to the main menu
		/// </summary>
		private void BackToMainMenu()
		{
			mMainElementHolder.SetActive(false);

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.MENU_SCENE);
		}

		/// <summary>
		/// Goes to the next slide
		/// </summary>
		private void NextSlide()
		{
			if (mCurrentSlide < mNumSlides)
			{
				mCurrentSlide++;
				SwitchSlide();

				// If you are at the last slide
				if (mCurrentSlide == mNumSlides)
				{
					mNextSlide.gameObject.SetActive(false);
				}

				if (mCurrentSlide > 1)
				{
					mPrevSlide.gameObject.SetActive(true);
				}
			}
		}

		/// <summary>
		/// Goes to the previous slide
		/// </summary>
		private void PrevSlide()
		{
			if (mCurrentSlide > 1)
			{
				mCurrentSlide--;
				SwitchSlide();

				// If you are not at the last slide
				if (mCurrentSlide < mNumSlides)
				{
					mNextSlide.gameObject.SetActive(true);
				}

				if (mCurrentSlide == 1)
				{
					mPrevSlide.gameObject.SetActive(false);
				}
			}
		}

		/// <summary>
		/// Switches the slide based on the current slide
		/// </summary>
		private void SwitchSlide()
		{
			if (mCurrentSlide == 1)
			{
				mSlideObject.GetComponent<Image>().sprite = mSlide1;
			}
			else if (mCurrentSlide == 2)
			{
				mSlideObject.GetComponent<Image>().sprite = mSlide2;
			}
			else if (mCurrentSlide == 3)
			{
				mSlideObject.GetComponent<Image>().sprite = mSlide3;
			}
			else if (mCurrentSlide == 4)
			{
				mSlideObject.GetComponent<Image>().sprite = mSlide4;
			}
			else if (mCurrentSlide == 5)
			{
				mSlideObject.GetComponent<Image>().sprite = mSlide5;
			}
		}
	}
}

