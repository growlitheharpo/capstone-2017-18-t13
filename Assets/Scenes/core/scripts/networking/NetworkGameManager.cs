using FiringSquad.Core;
using FiringSquad.Debug;
using UnityEngine;
using UnityEngine.Networking;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Networking
{
	/// <summary>
	/// The root NetworkManager for UNet.
	/// </summary>
	public class NetworkGameManager : NetworkManager
	{
		/// Private variables
		private int mPlayerCount;

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		protected void Start()
		{
			ServiceLocator.Get<IGameConsole>()
				.RegisterCommand("getip", CONSOLE_GetIpCommand)
				.RegisterCommand("ipconfig", CONSOLE_GetIpCommand);
		}

		/// <summary>
		/// Unity's OnDestroy signal.
		/// </summary>
		private void OnDestroy()
		{
			ServiceLocator.Get<IGameConsole>()
				.UnregisterCommand(CONSOLE_GetIpCommand);
		}

		/// <summary>
		/// Print the local player's ip address to the console.
		/// </summary>
		/// <param name="obj"></param>
		private void CONSOLE_GetIpCommand(string[] obj)
		{
			Logger.Info("IP: " + Network.player.ipAddress);
		}

		/// <summary>
		/// Unity function called when a new player is added.
		/// </summary>
		public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
		{
			base.OnServerAddPlayer(conn, playerControllerId);
			mPlayerCount += 1;

			EventManager.Notify(() => EventManager.Server.PlayerJoined(mPlayerCount));
		}

		/// <summary>
		/// Unity function called when a player is removed.
		/// </summary>
		public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
		{
			base.OnServerRemovePlayer(conn, player);
			mPlayerCount -= 1;
			EventManager.Notify(() => EventManager.Server.PlayerLeft(mPlayerCount));
		}
	}
}
