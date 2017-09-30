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
			
		}
	}
}
