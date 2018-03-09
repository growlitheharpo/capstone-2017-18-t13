using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using FiringSquad.Networking;
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

		private const string BASE_MESSAGE_FORMAT = "{0}		+{1}";

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			EventManager.Local.OnLocalPlayerDied += OnLocalPlayerDied;
			EventManager.Local.OnLocalPlayerCapturedStage += OnLocalPlayerCapturedStage;
			EventManager.Local.OnLocalPlayerAttachedPart += OnLocalPlayerAttachedPart;
			EventManager.Local.OnLocalPlayerGotKill += OnLocalPlayerGotKill;
		}

		/// <summary>
		/// Unity's OnDestroy function
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerDied -= OnLocalPlayerDied;
			EventManager.Local.OnLocalPlayerCapturedStage -= OnLocalPlayerCapturedStage;
			EventManager.Local.OnLocalPlayerAttachedPart -= OnLocalPlayerAttachedPart;
			EventManager.Local.OnLocalPlayerGotKill -= OnLocalPlayerGotKill;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerGotKill
		/// Handle showing the dead player's name
		/// </summary>
		private void OnLocalPlayerGotKill(CltPlayer deadPlayer, IWeapon currentWeapon, KillFlags killFlags)
		{
			if ((killFlags & KillFlags.Kingslayer) > 0)
				DisplayNewMessage(string.Format(BASE_MESSAGE_FORMAT, "KINGSLAYER!", NetworkServerGameManager.KINGSLAYER_POINTS));
			if ((killFlags & KillFlags.Killstreak) > 0)
				DisplayNewMessage(string.Format(BASE_MESSAGE_FORMAT, "KILLSTREAK!", NetworkServerGameManager.KILLSTREAK_POINTS));
			if ((killFlags & KillFlags.Revenge) > 0)
				DisplayNewMessage(string.Format(BASE_MESSAGE_FORMAT, "REVENGE KILL!", NetworkServerGameManager.REVENGE_KILL_POINTS));
			if ((killFlags & KillFlags.Multikill) > 0)
				DisplayNewMessage(string.Format(BASE_MESSAGE_FORMAT, "M-M-M-MULTI-KILL!", NetworkServerGameManager.MULTI_KILL_POINTS));
			if ((killFlags & KillFlags.Headshot) > 0)
				DisplayNewMessage(string.Format(BASE_MESSAGE_FORMAT, "HEADSHOT!", NetworkServerGameManager.HEADSHOT_KILL_POINTS));

			DisplayNewMessage(string.Format("ELIMINATED: {0}	 +{1}",
				deadPlayer.playerName,
				NetworkServerGameManager.STANDARD_KILL_POINTS
			));
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerDied
		/// Handle showing the killing player's name
		/// </summary>
		private void OnLocalPlayerDied(Vector3 spawnPosition, Quaternion spawnRotation, ICharacter killer)
		{
			// Check if the player was an actual player
			CltPlayer player = killer as CltPlayer;

			DisplayNewMessage(string.Format("ELIMINATED BY: {0}", player != null ? player.playerName : "YOURSELF"));
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerCapturedStage
		/// Handle showing that the player captured a stage.
		/// </summary>
		private void OnLocalPlayerCapturedStage()
		{
			DisplayNewMessage(string.Format(BASE_MESSAGE_FORMAT, "STAGE CAPTURED!", NetworkServerGameManager.STAGE_CAPTURE_POINTS));
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerAttachedPart
		/// Handle showing if the player got a legendary part
		/// </summary>
		private void OnLocalPlayerAttachedPart(BaseWeaponScript weapon, WeaponPartScript partInstance)
		{
			if (partInstance.isLegendary)
				DisplayNewMessage(string.Format(BASE_MESSAGE_FORMAT, "LEGENDARY PART!", NetworkServerGameManager.LEGENDARY_PART_POINTS));
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
