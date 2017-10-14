using System;
using System.Collections.Generic;
using System.Linq;
using KeatsLib.Collections;
using KeatsLib.State;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public class NetworkServerGameManager : NetworkBehaviour
	{
		[SerializeField] private int mRoundTime;
		[SerializeField] private int mGoalPlayerCount;

		private ServerStateMachine mStateMachine;

		public override void OnStartServer()
		{
			mStateMachine = new ServerStateMachine(this);
		}

		private void Update()
		{
			mStateMachine.Update();
		}

		private static Transform ChooseSafestSpawnPosition(CltPlayer[] players, CltPlayer deadPlayer, IList<Transform> targets)
		{
			var scores = new float[targets.Count];
			for (int i = 0; i < scores.Length; i++)
			{
				foreach (CltPlayer p in players)
				{
					if (p == deadPlayer)
						continue;
					scores[i] += Vector3.Distance(p.transform.position, targets[i].position);
				}
			}

			return targets[Array.IndexOf(scores, scores.Max())];
		}

		private class ServerStateMachine : BaseStateMachine
		{
			public ServerStateMachine(NetworkServerGameManager script)
			{
				mScript = script;
				TransitionStates(new WaitingForConnectionState(this));
				mStartPositions = FindObjectsOfType<NetworkStartPosition>().Select(x => x.transform).ToArray();
			}

			public new void Update()
			{
				base.Update();
			}

			private readonly NetworkServerGameManager mScript;

			private Transform[] mStartPositions;
			private CltPlayer[] mPlayerList;

			private class WaitingForConnectionState : BaseState<ServerStateMachine>
			{
				public WaitingForConnectionState(ServerStateMachine machine) : base(machine) { }

				private bool mReady;

				public override void OnEnter()
				{
					EventManager.Server.OnPlayerJoined += OnPlayerJoined;
					EventManager.Server.OnPlayerHealthHitsZero += OnPlayerHealthHitsZero;
				}

				public override void OnExit()
				{
					EventManager.Server.OnPlayerJoined -= OnPlayerJoined;
					EventManager.Server.OnPlayerHealthHitsZero -= OnPlayerHealthHitsZero;
				}

				private void OnPlayerJoined(int newCount)
				{
					if (newCount > mMachine.mScript.mGoalPlayerCount)
						Logger.Warn("We have too many players in this session!", Logger.System.Network);

					mReady = newCount >= mMachine.mScript.mGoalPlayerCount;
				}

				private void OnPlayerHealthHitsZero(CltPlayer deadPlayer, IDamageSource source)
				{
					EventManager.Server.PlayerDied(deadPlayer, null, mMachine.mStartPositions.ChooseRandom());
				}

				public override IState GetTransition()
				{
					return !mReady ? this : (IState)new StartGameState(mMachine);
				}
			}

			private class StartGameState : BaseState<ServerStateMachine>
			{
				public StartGameState(ServerStateMachine machine) : base(machine) { }

				public override void OnEnter()
				{
					mMachine.mPlayerList = FindObjectsOfType<CltPlayer>();

					if (mMachine.mPlayerList.Length > mMachine.mStartPositions.Length)
						Logger.Warn("We have too many players for the number of spawn positions!", Logger.System.Network);

					var spawnCopy = mMachine.mStartPositions.Select(x => x.transform).ToList();
					spawnCopy.Shuffle();

					foreach (CltPlayer player in mMachine.mPlayerList)
					{
						Transform target = spawnCopy[spawnCopy.Count - 1];
						spawnCopy.RemoveAt(spawnCopy.Count - 1);

						player.MoveToStartPosition(target.position, target.rotation);
					}
				}

				public override IState GetTransition()
				{
					return new GameRunningState(mMachine);
				}
			}

			private class GameRunningState : BaseState<ServerStateMachine>
			{
				public GameRunningState(ServerStateMachine machine) : base(machine) { }

				public override void OnEnter()
				{
					EventManager.Server.OnPlayerHealthHitsZero += OnPlayerHealthHitsZero;
				}

				public override void OnExit()
				{
					EventManager.Server.OnPlayerHealthHitsZero -= OnPlayerHealthHitsZero;
				}

				private void OnPlayerHealthHitsZero(CltPlayer dead, IDamageSource damage)
				{
					CltPlayer realSource = damage.source as CltPlayer;

					Transform newPosition = ChooseSafestSpawnPosition(mMachine.mPlayerList, dead, mMachine.mStartPositions);

					EventManager.Notify(() => EventManager.Server.PlayerDied(dead, realSource, newPosition));
				}

				public override IState GetTransition()
				{
					return this;
				}
			}
		}
	}
}
