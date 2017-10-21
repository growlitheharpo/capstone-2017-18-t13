using UnityEngine.Networking;

namespace FiringSquad.Networking
{
	public class NetworkGameManager : NetworkManager
	{
		private int mPlayerCount;

		public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
		{
			base.OnServerAddPlayer(conn, playerControllerId);
			mPlayerCount += 1;

			EventManager.Notify(() => EventManager.Server.PlayerJoined(mPlayerCount));
		}

		public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
		{
			base.OnServerRemovePlayer(conn, player);
			mPlayerCount -= 1;
			EventManager.Notify(() => EventManager.Server.PlayerLeft(mPlayerCount));
		}
	}
}
