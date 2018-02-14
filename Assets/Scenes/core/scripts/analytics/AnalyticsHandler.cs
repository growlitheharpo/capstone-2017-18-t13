using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiringSquad.Core;
using FiringSquad.Gameplay;
using FiringSquad.Gameplay.Weapons;
using UnityEngine.Networking;

namespace FiringSquad.Data
{
	public class AnalyticsHandler : NetworkBehaviour
	{

		/// <summary>
		/// Unity's awake function
		/// </summary>
		private void Awake()
		{
			EventManager.Server.OnPlayerDied += OnPlayerDied;

			// Start the file with formatting
			string tmp = "Killer,Killed,Killer Location - X, Y, Z,KilledLocation - X, Y, Z,Weapon Parts,,,,\n";

			System.IO.File.WriteAllText("Analytics.csv", tmp);
		}

		/// <summary>
		/// Cleanup event handlers
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Server.OnPlayerDied -= OnPlayerDied;
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnPlayerDied
		/// </summary>
		[Server]
		[EventHandler]
		private void OnPlayerDied(CltPlayer deadPlayer, ICharacter killer, Transform spawnPos)
		{
			// Make sure the player was killed by another player
			if (killer.gameObject.GetComponent<CltPlayer>())
			{
				// Get the killer and kill-e names
				string strOut = killer.gameObject.GetComponent<CltPlayer>().playerName + "," + deadPlayer.playerName + ",";

				// Add locations for the killer and the killed
				string locs = killer.gameObject.transform.position.x.ToString() + "," + killer.gameObject.transform.position.y.ToString()
					+ "," + killer.gameObject.transform.position.z.ToString() + "," + deadPlayer.gameObject.transform.position.x.ToString()
					+ "," + deadPlayer.gameObject.transform.position.y.ToString() + "," + deadPlayer.gameObject.transform.position.z.ToString();

				// Concatenate to the out string
				strOut += locs;

				// String for the weapon parts
				string weaponParts = "";

				// Temporarily getting the weapon part collection
				WeaponPartCollection tmp = killer.gameObject.GetComponent<CltPlayer>().weapon.currentParts;

				foreach (WeaponPartScript part in tmp)
				{
					weaponParts += "," + part.prettyName;
				}

				// Concatenating the weapon parts
				strOut += weaponParts;

				//UnityEngine.Debug.Log(strOut);
				OutputToFile(strOut);
			}

			// Else the player was killed by the environment
			else
			{
			}
		}
		
		/// <summary>
		/// Output the string to the file
		/// </summary>
		/// <param name="outputString"></param>
		private void OutputToFile(string outputString)
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter("Analytics.csv", true))
			{
				file.WriteLine(outputString);
			}
		}
	}
}
