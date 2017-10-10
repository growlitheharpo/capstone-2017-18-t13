using UnityEngine.Networking;

public class CltPlayer : NetworkBehaviour
{
	public override void OnStartServer()
	{
		// register for server events
	}

	public override void OnStartClient()
	{
		// register for local events that should effect all players (might not be any?)

		if (isLocalPlayer)
		{
			// instantiate the local player stuff

			// register for local-player only client events
		}
	}
}
