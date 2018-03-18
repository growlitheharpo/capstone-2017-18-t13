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
			EventManager.Local.OnReceiveGameEndTime += OnReceiveGameEndTime;
		}

		/// <summary>
		/// Cleanup all listeners and handlers.
		/// </summary>
		private void OnDestroy()
		{
			if (running)
				StopBroadcast();

			EventManager.Local.OnReceiveGameEndTime -= OnReceiveGameEndTime;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnReceiveGameEndTime
		/// </summary>
		private void OnReceiveGameEndTime(long time)
		{
			if (running)
				StopBroadcast();

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
