using UnityEngine.Networking;

namespace FiringSquad.Networking
{
	/// <summary>
	/// Utility class given for customizing the networkdiscovery class.
	/// </summary>
	public class OverrideNetworkDiscovery : NetworkDiscovery
	{
		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			EventManager.Local.OnReceiveStartEvent += OnReceiveStartEvent;
		}

		/// <summary>
		/// Cleanup all listeners and handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnReceiveStartEvent -= OnReceiveStartEvent;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnReceiveStartEvent
		/// </summary>
		private void OnReceiveStartEvent(long time)
		{
			Destroy(gameObject);
		}

		/// <summary>
		/// Handles a response to our broadcast listen.
		/// </summary>
		public override void OnReceivedBroadcast(string fromAddress, string data)
		{
			NetworkManager manager = FindObjectOfType<NetworkManager>();
			manager.networkAddress = fromAddress;
			manager.StartClient();

			StopBroadcast();
		}
	}
}
