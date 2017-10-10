using UnityEngine.Networking;

public class CltPlayer : NetworkBehaviour
{
	public override void OnStartServer()
	{
		// register for server events
	}

	public override void OnStartClient()
	{
		// register for client events

		if (isLocalPlayer)
		{
			// instantiate the local player stuff
		}
	}
}
