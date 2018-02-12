using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
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
		[SerializeField] private float mFadeTextTime;

		// Private variables
		private Text mUIText;
		private Image mKillIndicator;
		private Color mOriginalColor;
		private Color mFadedColor;

		// Base string for the elimination text
		const string mBaseString = "You Eliminated: ";

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mUIText = GetComponentInChildren<Text>();

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
		private void OnLocalPlayerGotKill(CltPlayer deadPlayer, IWeapon currentWeapon)
		{
			StopAllCoroutines();

			// Update to the dead players name
			UpdateText(deadPlayer.playerName);
			mKillIndicator.color = mOriginalColor;

			StartCoroutine(Coroutines.InvokeAfterSeconds(mFadeTextTime, () =>
			{
				mUIText.text = "";
			}));

			StartCoroutine(Coroutines.InvokeEveryTick((currentTime) =>
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

		/// <summary>
		/// Update the UI text to the 
		/// </summary>
		private void UpdateText(string text)
		{
			mUIText.text = mBaseString + text;
		}
	}
}


