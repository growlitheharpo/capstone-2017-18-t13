using UnityEngine;
using UnityEngine.Networking;

public class OverrideNetworkDiscovery : NetworkDiscovery
{
	private void Start()
	{
		Initialize();
	}

	private void OnGUI()
	{
		Rect rect = new Rect(0.0f, Screen.height - 200.0f, 200.0f, 200.0f);
		GUILayout.BeginArea(rect);

		if (GUILayout.Button("Start As Server"))
			StartAsServer();
		if (GUILayout.Button("Start As Client"))
			StartAsClient();
		GUILayout.EndArea();
	}

	public override void OnReceivedBroadcast(string fromAddress, string data)
	{
		NetworkManager manager = FindObjectOfType<NetworkManager>();
		manager.networkAddress = fromAddress;
		manager.StartClient();

		StopBroadcast();
	}
}
