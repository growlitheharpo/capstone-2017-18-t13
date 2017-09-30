using UnityEngine.Networking;

public class OverrideNetworkDiscovery : NetworkDiscovery
{
	public override void OnReceivedBroadcast(string fromAddress, string data)
	{
		NetworkManager manager = FindObjectOfType<NetworkManager>();
		manager.networkAddress = fromAddress;
		manager.StartClient();

		StopBroadcast();
	}
}
