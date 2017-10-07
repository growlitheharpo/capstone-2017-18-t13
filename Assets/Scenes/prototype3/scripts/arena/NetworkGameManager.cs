using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkGameManager : NetworkManager, INetworkManager
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

	public bool isGameClient { get { return Network.isServer; } }
	public bool isGameServer { get { return Network.isClient; } }
	public bool isGameHost { get { return isGameClient && isGameServer; } }
}
