using System;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public class NetworkServerGameManager : NetworkBehaviour
	{
		[SerializeField] private int mRoundTime;

		public void NotifyStartGame()
		{
			long now = DateTime.UtcNow.Ticks;
			long lengthTicks = mRoundTime * TimeSpan.TicksPerSecond;
			long endTime = now + lengthTicks;

			var clients = FindObjectsOfType<NetworkClientGameManager>();
			foreach (NetworkClientGameManager c in clients)
				c.RpcNotifyStartGame(endTime);
		}
	}
}
