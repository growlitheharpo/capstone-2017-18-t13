using System.Collections.Generic;
using System.Linq;
using KeatsLib.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public class NetworkServerGameManager : NetworkBehaviour
	{
		[SerializeField] private int mRoundTime;
		[SerializeField] private int mGoalPlayerCount;

		private NetworkStartPosition[] mStartPositions;
		private CltPlayer[] mPlayerList;

		private void Awake()
		{
			EventManager.Server.OnPlayerJoined += OnPlayerJoined;
		}

		public override void OnStartServer()
		{
			mStartPositions = FindObjectsOfType<NetworkStartPosition>();
		}

		private void OnPlayerJoined(int newCount)
		{
			if (newCount == mGoalPlayerCount)
				StartGame();
			else if (newCount > mGoalPlayerCount)
				Logger.Warn("We have too many players in this session!", Logger.System.Network);
		}

		private void StartGame()
		{
			mPlayerList = FindObjectsOfType<CltPlayer>();
			if (mPlayerList.Length > mStartPositions.Length)
				Logger.Warn("We have too many players for the number of spawn positions!", Logger.System.Network);

			var spawnCopy = mStartPositions.Select(x => x.transform).ToList();
			spawnCopy.Shuffle();

			foreach (CltPlayer player in mPlayerList)
			{
				Transform target = spawnCopy[spawnCopy.Count - 1];
				spawnCopy.RemoveAt(spawnCopy.Count - 1);

				player.MoveToStartPosition(target.position, target.rotation);
			}
		}
	}
}
