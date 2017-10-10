using UnityEngine;
using UnityEngine.Networking;

public class NetworkClientGameManager : NetworkBehaviour
{
	[ClientRpc]
	public void RpcNotifyStartGame(long endTime)
	{
		EventManager.Notify(() => EventManager.AllPlayersReady(endTime));
	}

	[Command]
	public void CmdNotifyPlayerDied(NetworkInstanceId pId, Vector3 position)
	{
		NetworkServer.FindLocalObject(pId).GetComponent<PlayerScript>().RpcHandleRemoteDeath(position);
	}
}
