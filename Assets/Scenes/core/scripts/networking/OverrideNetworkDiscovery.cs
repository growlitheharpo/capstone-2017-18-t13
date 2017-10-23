using UnityEngine.Networking;

namespace FiringSquad.Networking
{
	public class OverrideNetworkDiscovery : NetworkDiscovery
	{
		private void Awake()
		{
			EventManager.Local.OnReceiveStartEvent += OnReceiveStartEvent;
		}

		private void OnDestroy()
		{
			EventManager.Local.OnReceiveStartEvent -= OnReceiveStartEvent;
		}

		private void OnReceiveStartEvent(long time)
		{
			Destroy(gameObject);
		}

		public override void OnReceivedBroadcast(string fromAddress, string data)
		{
			NetworkManager manager = FindObjectOfType<NetworkManager>();
			manager.networkAddress = fromAddress;
			manager.StartClient();

			StopBroadcast();
		}
	}
}
