using FiringSquad.Core;
using FiringSquad.Core.Input;
using FiringSquad.Core.UI;
using FiringSquad.Data;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class that manages player elimination text below the crosshair.
	/// Uses EventManager.LocalGUI.SetHintState to handle the hint stack.
	/// </summary>
	public class PlayerDeathText : MonoBehaviour
	{

		private UnityEngine.UI.Text mUIText;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mUIText = GetComponent<UnityEngine.UI.Text>();
		}

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			EventManager.Local.OnLocalPlayerDied += OnLocalPlayerDied;
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerDied -= OnLocalPlayerDied;
		}


		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerDied
		/// Handle showing the killing player's name
		/// </summary>
		private void OnLocalPlayerDied(Vector3 spawnPosition, Quaternion spawnRotation, ICharacter killer)
		{
			// Set as a player name if they were killed by another player
			if (killer != null)
			{
				string killer_name = killer.gameObject.GetComponent<CltPlayer>().playerName;
				killer_name = "Eliminated by: " + killer_name;
				UpdateText(killer_name);
			}

			// Otherwise show that you were elimated by a turret
			else
			{
				string killer_name = "Eliminated by : Turret";
				UpdateText(killer_name);
			}

			// Similar to coroutine in the PlayerLocal - used to get rid of elimated message after 4 seconds
			StartCoroutine(Coroutines.InvokeEveryTick(time =>
			{
				if (time < 4)
				{
					return true;
				}

				UpdateText("");

				return false;
			}));
		}

		/// <summary>
		/// Update the text to be the most recent hint text pushed.
		/// </summary>
		private void UpdateText(string text)
		{
			mUIText.text = text;
		}
	}
}
