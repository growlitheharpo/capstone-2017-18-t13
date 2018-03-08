using FiringSquad.Data;
using UnityEngine;
using FiringSquad.Gameplay.Weapons;
using UnityEngine.UI;
using KeatsLib.Unity;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class that manages player elimination text and kill indicator
	/// </summary>
	public class EnemyEliminationText : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private float mFadeImageTime;

		// Private variables
		private Image mKillIndicator;
		private Color mOriginalColor;
		private Color mFadedColor;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			// Get the kill indicator and its color
			mKillIndicator = GetComponentInChildren<Image>();
			mOriginalColor = mKillIndicator.color;

			// Getting the color after fade and storing it
			mFadedColor = Color.clear;

			mKillIndicator.color = mFadedColor;

			EventManager.Local.OnLocalPlayerGotKill += OnLocalPlayerGotKill;
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerGotKill -= OnLocalPlayerGotKill;
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnLocalPlayerGotKill
		/// </summary>
		[EventHandler]
		private void OnLocalPlayerGotKill(CltPlayer deadPlayer, IWeapon currentWeapon, KillFlags flags)
		{
			StopAllCoroutines();

			// Show the image
			mKillIndicator.color = mOriginalColor;

			StartCoroutine(Coroutines.InvokeEveryTick(currentTime =>
			{
				if (currentTime < mFadeImageTime)
				{
					mKillIndicator.color = Color.Lerp(mOriginalColor, mFadedColor, currentTime / mFadeImageTime);
					return true;
				}

				mKillIndicator.color = mFadedColor;
				return false;
			}));
		}
	}
}
