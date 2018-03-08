﻿using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI script that handles displaying score-related information on the HUD
	/// </summary>
	public class PlayerScorePopupPanel : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private GameObject mTextPrefab;
		[SerializeField] private float mPreFadeOutTime = 0.35f;
		[SerializeField] private float mFadeOutLength = 0.55f;

		[SerializeField] private Graphic mPersistentColorReferenceGraphic;
		[SerializeField] private Shadow mPersistentColorReferenceShadow;

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			EventManager.Local.OnLocalPlayerDied += OnLocalPlayerDied;
			EventManager.Local.OnLocalPlayerGotKill += OnLocalPlayerGotKill;
		}

		/// <summary>
		/// Unity's OnDestroy function
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerDied -= OnLocalPlayerDied;
			EventManager.Local.OnLocalPlayerGotKill -= OnLocalPlayerGotKill;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerGotKill
		/// Handle showing the dead player's name
		/// </summary>
		private void OnLocalPlayerGotKill(CltPlayer deadPlayer, IWeapon currentWeapon)
		{
			string displayText = string.Format("Eliminated: {0}		+{1}",
				deadPlayer.playerName,
				"100" // TODO: Fetch a real score here.
			);

			DisplayNewMessage(displayText);
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerDied
		/// Handle showing the killing player's name
		/// </summary>
		private void OnLocalPlayerDied(Vector3 spawnPosition, Quaternion spawnRotation, ICharacter killer)
		{
			// Check if the player was an actual player
			CltPlayer player = killer as CltPlayer;

			string displayText = string.Format("Eliminated by: {0}	   -{1}",
				player != null ? player.playerName : "yourself", 
				"75" // TODO: Fetch a real score here.
			);

			DisplayNewMessage(displayText);
		}

		/// <summary>
		/// Display a new pop-up message on the screen.
		/// </summary>
		/// <param name="message">The fully-formatted message to display.</param>
		private void DisplayNewMessage(string message)
		{
			GameObject instance = Instantiate(mTextPrefab, transform);
			instance.transform.SetAsFirstSibling();

			// Setup our team color
			Color textColor = mPersistentColorReferenceGraphic.color;
			textColor.a = 0.0f;

			// Place the message and set the color
			Text text = instance.GetComponent<Text>();
			text.text = message;
			text.color = textColor;

			// Set the shadow if we have one
			Shadow shadow = instance.GetComponent<Shadow>();
			if (shadow != null)
				shadow.effectColor = mPersistentColorReferenceShadow.effectColor;

			// Prep the animation and fade out time
			Animator anim = instance.GetComponent<Animator>();
			StartCoroutine(Coroutines.InvokeAfterSeconds(mPreFadeOutTime, () => StartFadeout(anim)));
		}

		/// <summary>
		/// Start fading out an instance of the text by calling the "Exit" trigger on its animator.
		/// </summary>
		private void StartFadeout(Animator instance)
		{
			if (instance == null)
				return;

			instance.SetTrigger("Exit");
			StartCoroutine(Coroutines.InvokeAfterSeconds(mFadeOutLength, () => KillInstance(instance)));
		}

		/// <summary>
		/// Officially kill (Destroy) an instance.
		/// </summary>
		private void KillInstance(Animator instance)
		{
			if (instance == null)
				return;

			Destroy(instance.gameObject);
		}
	}
}
