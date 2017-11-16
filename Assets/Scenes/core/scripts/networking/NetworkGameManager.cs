using UnityEngine.Networking;

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
