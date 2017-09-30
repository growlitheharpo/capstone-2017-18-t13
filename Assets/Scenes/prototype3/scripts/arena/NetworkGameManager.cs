using FiringSquad.Gameplay;
using UnityEngine.Networking;

public class NetworkGameManager : NetworkManager
{
	private int mPlayerCount;

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		base.OnServerAddPlayer(conn, playerControllerId);
		mPlayerCount += 1;

		if (mPlayerCount == 2)
			FindObjectOfType<NetworkServerGameManager>().NotifyStartGame();
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
	{
		base.OnServerRemovePlayer(conn, player);
		mPlayerCount -= 1;
	}
}
