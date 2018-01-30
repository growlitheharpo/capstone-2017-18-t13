using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using FiringSquad.Gameplay.Weapons;
using UnityEngine.UI;


namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class that manages player elimination text and kill indicator
	/// </summary>
	public class EnemyEliminationText : MonoBehaviour
	{
		// Private variables
		private UnityEngine.UI.Text mUIText;
		private Image mKillIndicator;
		private Color mOriginalColor;
		private Color mFadedColor;

		// Base string for the elimination text
		const string mBaseString = "You Eliminated: ";
		//Time it takes for elimination text to fade away
		const float mTextTime = 2.0f;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mUIText = GetComponent<UnityEngine.UI.Text>();

			// Get the kill indicator and its color
			mKillIndicator = GetComponentInChildren<Image>();
			mOriginalColor = mKillIndicator.color;

			// Getting the color after fade and storing it
			mFadedColor = Color.clear;

			mKillIndicator.color = mFadedColor;

			EventManager.Server.OnPlayerDied += OnPlayerDied;
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Server.OnPlayerDied -= OnPlayerDied;
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnPlayerDied
		/// </summary>
		[EventHandler]
		private void OnPlayerDied(CltPlayer deadPlayer, ICharacter killer, Transform spawnPos)
		{
			// Compare the killer with this player
			if (killer.isCurrentPlayer)
			{
				// Update to the dead players name
				UpdateText(deadPlayer.playerName);
				StartCoroutine(RemoveText(mTextTime));
				StartCoroutine(FadeKillIndicator(mTextTime));
			}
		}

		/// <summary>
		/// Update the UI text to the 
		/// </summary>
		private void UpdateText(string text)
		{
			mUIText.text = mBaseString + text;
		}

		/// <summary>
		/// Remove the text from the screen
		/// </summary>
		private IEnumerator RemoveText(float time)
		{
			float currentTime = 0.0f;
			while (currentTime < time)
			{
				currentTime += Time.deltaTime;
				yield return null;
			}

			mUIText.text = "";
		}

		/// <summary>
		/// Fade the kill indicator away
		/// </summary>
		private IEnumerator FadeKillIndicator(float time)
		{
			float currentTime = 0.0f;
			while (currentTime < time)
			{
				
				mKillIndicator.color = Color.Lerp(mOriginalColor, mFadedColor, currentTime / time);

				currentTime += Time.deltaTime;
				yield return null;
			}

			mKillIndicator.color = mFadedColor;
		}
	}
}


