using System.Collections;
using System.Collections.Generic;
using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI script that displays hype text at the top of the screen when cool shit happens.
	/// Does almost the same thing as PlayerScorePopupPanel.
	/// </summary>
	public class PlayerHypeTextPanel : MonoBehaviour
	{
		[SerializeField] private GameObject mTextPrefab;
		[SerializeField] private float mTextLifetime = 4.0f;
		
		[SerializeField] private Graphic mPersistentColorReferenceGraphic;
		[SerializeField] private Shadow mPersistentColorReferenceShadow;

		private const string MULTIKILL = "DOMINATOR";
		private const string KILLSTREAK = "UNSTOPPABLE";
		private const string REVENGE = "BEYOND THE GRAVE";
		private const string KINGSLAYER = "DREAM CRUSHER";
		private const string HEADSHOT = "MARKSMAN";
		private const string LEGENDARY = "DESTROY YOUR ENEMIES";
		private const string STAGE_CAPTURE = "LEGENDARY PART INBOUND";

		/// <summary>
		/// Unity's Start function
		/// </summary>
		private void Start()
		{
			EventManager.Local.OnLocalPlayerCapturedStage += OnLocalPlayerCapturedStage;
			EventManager.Local.OnLocalPlayerAttachedPart += OnLocalPlayerAttachedPart;
			EventManager.Local.OnLocalPlayerGotKill += OnLocalPlayerGotKill;
		}

		/// <summary>
		/// Unity's OnDestroy function
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerCapturedStage -= OnLocalPlayerCapturedStage;
			EventManager.Local.OnLocalPlayerAttachedPart -= OnLocalPlayerAttachedPart;
			EventManager.Local.OnLocalPlayerGotKill -= OnLocalPlayerGotKill;
		}
		
		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerGotKill
		/// Handle showing the relevant kill message (if any)
		/// </summary>
		private void OnLocalPlayerGotKill(CltPlayer deadPlayer, IWeapon currentWeapon, KillFlags killFlags)
		{
			if ((killFlags & KillFlags.Multikill) != KillFlags.None)
				DisplayNewMessage(MULTIKILL);
			else if ((killFlags & KillFlags.Killstreak) != KillFlags.None)
				DisplayNewMessage(KILLSTREAK);
			else if ((killFlags & KillFlags.Revenge) != KillFlags.None)
				DisplayNewMessage(REVENGE);
			else if ((killFlags & KillFlags.Kingslayer) != KillFlags.None)
				DisplayNewMessage(KINGSLAYER);
			else if ((killFlags & KillFlags.Headshot) != KillFlags.None)
				DisplayNewMessage(HEADSHOT);
		}
		
		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerAttachedPart
		/// Handle showing if the player got a legendary part
		/// </summary>
		private void OnLocalPlayerAttachedPart(BaseWeaponScript weapon, WeaponPartScript partInstance)
		{
			if (partInstance.isLegendary)
				DisplayNewMessage(LEGENDARY);
		}
		
		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerCapturedStage
		/// Handle showing that the player captured a stage.
		/// </summary>
		private void OnLocalPlayerCapturedStage()
		{
			DisplayNewMessage(STAGE_CAPTURE);
		}

		/// <summary>
		/// Display a new hype text message on the screen.
		/// </summary>
		/// <param name="message">The fully-formatted message to display.</param>
		private Text DisplayNewMessage(string message)
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
			StartCoroutine(Coroutines.InvokeAfterSeconds(mTextLifetime, () => KillInstance(anim)));

			return text;
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
