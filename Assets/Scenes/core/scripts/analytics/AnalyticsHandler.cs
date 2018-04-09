using System.Collections.Generic;
using UnityEngine;
using FiringSquad.Gameplay;
using FiringSquad.Gameplay.Weapons;
using UnityEngine.Networking;

namespace FiringSquad.Data
{
	public class AnalyticsHandler : NetworkBehaviour
	{
		[SerializeField] private string mKillAnalyticsFile = "Analytics.csv";
		[SerializeField] private string mHealthAnalyticsFile = "HealthPack.csv";
		[SerializeField] private string mWeaponPadAnalyticsFile = "WeaponPad.csv";

		// List of the healthpacks and the number of times they were used
		private Dictionary<string, int> mPackCount;
		private Dictionary<string, int> mWeaponPadCount;

		/// <summary>
		/// Unity's awake function
		/// </summary>
		public override void OnStartServer()
		{
			EventManager.Server.OnPlayerDied += OnPlayerDied;
			EventManager.Server.OnHealthPickedUp += OnHealthPickedUp;
			EventManager.Server.OnPartPickedUp += OnPartPickedUp;
			EventManager.Server.OnFinishGame += OnFinishGame;

			mPackCount = new Dictionary<string, int>();
			mWeaponPadCount = new Dictionary<string, int>();

			if (!System.IO.File.Exists(mKillAnalyticsFile))
			{
				// Start the file with formatting
				const string tmp = "Killer,Killed,Killer Location - X, Y, Z,KilledLocation - X, Y, Z,Weapon Parts,,,,\n";
				System.IO.File.WriteAllText(mKillAnalyticsFile, tmp);
			}

			if (!System.IO.File.Exists(mHealthAnalyticsFile))
			{
				// Start the file with formatting
				const string tmp = "Health Pack Name, Times Used,\n";
				System.IO.File.WriteAllText(mHealthAnalyticsFile, tmp);
			}

			if (!System.IO.File.Exists(mWeaponPadAnalyticsFile))
			{
				// Start the file with formatting
				const string tmp = "Weapon Pad Name, Times Used,\n";
				System.IO.File.WriteAllText(mWeaponPadAnalyticsFile, tmp);
			}
		}

		/// <summary>
		/// Cleanup event handlers
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Server.OnPlayerDied -= OnPlayerDied;
			EventManager.Server.OnHealthPickedUp -= OnHealthPickedUp;
			EventManager.Server.OnPartPickedUp -= OnPartPickedUp;
			EventManager.Server.OnFinishGame -= OnFinishGame;
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnPlayerDied
		/// </summary>
		[Server]
		[EventHandler]
		private void OnPlayerDied(CltPlayer deadPlayer, PlayerKill killInfo)
		{
			ICharacter killer = killInfo.killer;

			// Make sure the player was killed by another player
			if (killer != null)
			{
				// Get the killer and kill-e names
				string strOut = killer.gameObject.GetComponent<CltPlayer>().playerName + "," + deadPlayer.playerName + ",";

				// Add locations for the killer and the killed
				string locs = killer.gameObject.transform.position.x + "," + killer.gameObject.transform.position.y
					+ "," + killer.gameObject.transform.position.z + "," + deadPlayer.gameObject.transform.position.x
					+ "," + deadPlayer.gameObject.transform.position.y + "," + deadPlayer.gameObject.transform.position.z;

				// Concatenate to the out string
				strOut += locs;

				// String for the weapon parts
				string weaponParts = "";

				// Temporarily getting the weapon part collection
				IWeapon weapon = killer.gameObject.GetComponent<CltPlayer>().weapon;
				if (weapon != null)
				{
					WeaponPartCollection tmp = weapon.currentParts;
					foreach (WeaponPartScript part in tmp)
						weaponParts += "," + part.prettyName;
				}

				// Concatenating the weapon parts
				strOut += weaponParts;

				//UnityEngine.Debug.Log(strOut);
				OutputToFile(strOut, mKillAnalyticsFile);
			}

			// Else the player was killed by the environment
			else
			{
				string strOut = "Environment" + "," + deadPlayer.playerName + ",";
				OutputToFile(strOut, mKillAnalyticsFile);
			}
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnHealthPickedUp
		/// </summary>
		[Server]
		[EventHandler]
		private void OnHealthPickedUp(PlayerHealthPack pack)
		{
			// If the dictionary already contains this health pack
			if (mPackCount.ContainsKey(pack.gameObject.name))
				mPackCount[pack.name] += 1;
			else
				mPackCount.Add(pack.gameObject.name, 1);
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnPartPickedUp
		/// </summary>
		[Server]
		[EventHandler]
		private void OnPartPickedUp(WeaponPartPad pad)
		{
			// If the dictionary already contains this health pack
			if (mWeaponPadCount.ContainsKey(pad.gameObject.name))
				mWeaponPadCount[pad.name] += 1;
			else
				mWeaponPadCount.Add(pad.gameObject.name, 1);
		}

		/// <summary>
		/// EVENT HANDLER: Server.OnFinishGame
		/// </summary>
		[Server]
		[EventHandler]
		private void OnFinishGame(PlayerScore[] scores)
		{
			// Run this when the game ends
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(mHealthAnalyticsFile, true))
			{
				// Write the health pack data to a file
				foreach(var entry in mPackCount)
				{
					string outputString = entry.Key + "," + entry.Value;
					file.WriteLine(outputString);
				}
			}

			using (System.IO.StreamWriter file = new System.IO.StreamWriter(mWeaponPadAnalyticsFile, true))
			{
				// Write the weapon pad data to a file
				foreach (var entry in mWeaponPadCount)
				{
					string outputString = entry.Key + "," + entry.Value;
					file.WriteLine(outputString);
				}
			}
		}

		/// <summary>
		/// Output the string to the file
		/// </summary>
		/// <param name="outputString">The string to write</param>
		/// <param name="fileName">The file to write to.</param>
		private static void OutputToFile(string outputString, string fileName)
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))
				file.WriteLine(outputString);
		}
	}
}
