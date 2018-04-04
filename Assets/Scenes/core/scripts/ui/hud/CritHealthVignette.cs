using System.Collections;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.UI;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class that enables and disables the critical health vignette as necessary
	/// </summary>
	public class CritHealthVignette : MonoBehaviour
	{
		// Inspector variable for what is considered critical health
		[SerializeField] private float mCritHealthNumber;

		private CltPlayer mPlayerRef;
		private UIImage mCritImage;

		private Color mVisibleColor;
		private Color mHiddenColor;

		private const float FADE_OUT_TIME = 0.35f;

		private bool mIsEnabled = false;


		// Use this for initialization
		void Start()
		{
			mCritImage = GetComponent<UIImage>();

			// Set the colors for the image
			mVisibleColor = mHiddenColor = mCritImage.color;
			mHiddenColor.a = 0.0f;

			mCritImage.color = mHiddenColor;
		}

		// Update is called once per frame
		void Update()
		{
			SearchForPlayer();

			// Ensure we have a player 
			if (mPlayerRef == null)
				return;

			// Check if the player has health
			if (CheckPlayerHealth())
			{
				// Check if the vignette is enabled
				if (mIsEnabled)
				{
					// Fade out if it is
					FadeVignette();
					mIsEnabled = false;
				}
			}
			else
			{
				if (!mIsEnabled)
				{
					// Check if 
					EnableVignette();
					mIsEnabled = true;
				}	   
			}
		}

		/// <summary>
		/// Attempts to grab a reference to the local player. Runs every frame until success.
		/// </summary>
		private void SearchForPlayer()
		{
			if (mPlayerRef != null)
				return;

			mPlayerRef = CltPlayer.localPlayerReference;
		}

		/// <summary>
		/// Check if the player's health is above a critical level
		/// </summary>
		private bool CheckPlayerHealth()
		{
			if (mPlayerRef.currentHealth <= mCritHealthNumber)
			{

				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// Enables the screen vignette
		/// </summary>
		private void EnableVignette()
		{
			mCritImage.color = mVisibleColor;
		}

		private void FadeVignette()
		{
			StartCoroutine(FadeOutColor(mCritImage, mVisibleColor, mHiddenColor, FADE_OUT_TIME, false));
		}

		/// <summary>
		/// Fade out the color of a UI element.
		/// </summary>
		/// <param name="image">The </param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="time"></param>
		/// <param name="returnToPool"></param>
		/// <returns></returns>
		private IEnumerator FadeOutColor(Graphic image, Color a, Color b, float time, bool returnToPool)
		{
			image.color = a;
			yield return Coroutines.LerpUIColor(image, b, time);
		}
	}
}


