using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Data;
using FiringSquad.Debug;
using FiringSquad.Gameplay;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Collections;
using KeatsLib.State;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Logger = FiringSquad.Debug.Logger;
using Random = UnityEngine.Random;

namespace FiringSquad.Networking
{
	public class NetworkServerGameManager : NetworkBehaviour
	{
		[SerializeField] private List<WeaponPartScript> mStageCaptureParts;
		[SerializeField] private float mMinStageWaitTime;
		[SerializeField] private float mMaxStageWaitTime;
		[SerializeField] private int mRoundTime;
		[SerializeField] private int mLobbyTime;
		[SerializeField] private int mGoalPlayerCount;

		private ServerStateMachine mStateMachine;

		public override void OnStartServer()
		{
			mStateMachine = new ServerStateMachine(this);

			ServiceLocator.Get<IGameConsole>().RegisterCommand("force-start", CONSOLE_ForceStartGame);
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IGameConsole>().UnregisterCommand(CONSOLE_ForceStartGame);
		}

		private void Update()
		{
			mStateMachine.Update();
		}

		private void CONSOLE_ForceStartGame(string[] obj)
		{
			if (obj[0] == "1")
				mStateMachine.ForceStartGameNow(true);
			else if (obj[1] == "2")
				mStateMachine.ForceStartGameNow(false);
			else
				throw new ArgumentException("force-start called with invalid parameters.\nUse \"force-start 1\" to start with lobby, or \"force-start 2\" to start the game.");
		}

		private static Transform ChooseSafestSpawnPosition(CltPlayer[] players, CltPlayer deadPlayer, IList<Transform> targets)
		{
			var scores = new float[targets.Count];
			for (int i = 0; i < scores.Length; i++)
			{
				foreach (CltPlayer p in players)
				{
					if (p == deadPlayer || p == null)
						continue;
					scores[i] += Vector3.Distance(p.transform.position, targets[i].position);
				}
			}

			return targets[Array.IndexOf(scores, scores.Max())];
		}

		/// <summary>
		/// The state machine for the server's game manager
		/// </summary>
		private class ServerStateMachine : BaseStateMachine
		{
			public ServerStateMachine(NetworkServerGameManager script)
			{
				mScript = script;
				mStartPositions = GameObject.FindGameObjectsWithTag("matchspawn").Select(x => x.transform).ToArray();
				TransitionStates(new WaitingForConnectionState(this));
			}

			public void ForceStartGameNow(bool lobby)
			{
				if (lobby)
					TransitionStates(new StartLobbyState(this));
				else
					TransitionStates(new StartGameState(this));
			}

			public new void Update()
			{
				base.Update();
			}

			// Private shared data
			private readonly NetworkServerGameManager mScript;
			private readonly Transform[] mStartPositions;
			private StageCaptureArea[] mCaptureAreas;
			private CltPlayer[] mPlayerList;
			private Dictionary<NetworkInstanceId, PlayerScore> mPlayerScores;

			/// <summary>
			/// The state we hold in until we have the required number of players
			/// </summary>
			private class WaitingForConnectionState : BaseState<ServerStateMachine>
			{
				public WaitingForConnectionState(ServerStateMachine machine) : base(machine) { }

				private bool mReady;

				public override void OnEnter()
				{
					mMachine.mCaptureAreas = FindObjectsOfType<StageCaptureArea>();
					foreach (StageCaptureArea area in mMachine.mCaptureAreas)
						area.Disable();

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
					return !mReady ? this : (IState)new StartLobbyState(mMachine);
				}
			}

			private class StartLobbyState : BaseState<ServerStateMachine>
			{
				public StartLobbyState(ServerStateMachine machine) : base(machine) { }

				private long mEndTime;

				public override void OnEnter()
				{
					mMachine.mPlayerList = FindObjectsOfType<CltPlayer>();
					mEndTime = DateTime.Now.Ticks + mMachine.mScript.mLobbyTime * TimeSpan.TicksPerSecond;

					foreach (CltPlayer player in mMachine.mPlayerList)
						player.RpcStartLobbyCountdown(mEndTime);
				}

				private bool IsWaitingTimeOver()
				{
					return DateTime.Now.Ticks >= mEndTime;
				}

				public override IState GetTransition()
				{
					if (IsWaitingTimeOver())
						return new StartGameState(mMachine);

					return this;
				}
			}

			/// <summary>
			/// The single-frame state called to set up the players for the round to start.
			/// Resets spawn points and ensures our shared data has an accurate list of players.
			/// </summary>
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

			/// <summary>
			/// State that runs the actual game. Ticks the timer for the game
			/// and handles player deaths during the match by sending them to
			/// a spawn point.
			/// 
			/// Also handles the StageCaptureArea enabling and disabling.
			/// </summary>
			private class GameRunningState : BaseState<ServerStateMachine>
			{
				public GameRunningState(ServerStateMachine machine) : base(machine) { }

				private Coroutine mStageEnableRoutine;
				private long mEndTime;
				private bool mFinished;

				public override void OnEnter()
				{
					mEndTime = DateTime.Now.Ticks + mMachine.mScript.mRoundTime * TimeSpan.TicksPerSecond;
					EventManager.Server.OnPlayerHealthHitsZero += OnPlayerHealthHitsZero;
					EventManager.Server.OnPlayerCapturedStage += OnPlayerCapturedStage;
					EventManager.Server.OnStageTimedOut += OnStageTimedOut;

					mMachine.mPlayerScores = mMachine.mPlayerList.Select(x => new PlayerScore(x)).ToDictionary(x => x.playerId);

					mStageEnableRoutine = mMachine.mScript.StartCoroutine(EnableStageArea(mMachine.mCaptureAreas.ChooseRandom()));

					EventManager.Notify(() => EventManager.Server.StartGame(mEndTime));
				}

				private void OnPlayerCapturedStage(StageCaptureArea stage, CltPlayer player)
				{
					stage.Disable();

					SpawnLegendaryPart(stage, player);

					StageCaptureArea nextStage = mMachine.mCaptureAreas.Where(x => x != stage).ChooseRandom();
					mStageEnableRoutine = mMachine.mScript.StartCoroutine(EnableStageArea(nextStage));
				}

				private void SpawnLegendaryPart(StageCaptureArea stage, CltPlayer player)
				{
					WeaponPartCollection currentParts = player.weapon.currentParts;
					var allPossibilities = mMachine.mScript.mStageCaptureParts;
					var possibilities = allPossibilities.Where(x => !currentParts.Contains(x));
					WeaponPartScript choice = possibilities.ChooseRandom();

					GameObject instance = choice.SpawnInWorld();
					instance.transform.position = stage.transform.position + Vector3.up * 45.0f;
					instance.name = choice.partId;

					NetworkServer.Spawn(instance);
					mMachine.mScript.StartCoroutine(Coroutines.InvokeAfterFrames(2, () => { instance.GetComponent<WeaponPickupScript>().RpcInitializePickupView(); }));
				}

				private void OnStageTimedOut(StageCaptureArea stage)
				{
					stage.Disable();

					StageCaptureArea nextStage = mMachine.mCaptureAreas.Where(x => x != stage).ChooseRandom();
					mStageEnableRoutine = mMachine.mScript.StartCoroutine(EnableStageArea(nextStage));
				}

				public override void Update()
				{
					if (!mFinished && DateTime.Now.Ticks >= mEndTime)
						mFinished = true;
				}

				private IEnumerator EnableStageArea(StageCaptureArea stage)
				{
					yield return new WaitForSeconds(Random.Range(mMachine.mScript.mMinStageWaitTime, mMachine.mScript.mMaxStageWaitTime));
					stage.Enable();
				}

				public override void OnExit()
				{
					EventManager.Server.OnPlayerHealthHitsZero -= OnPlayerHealthHitsZero;
					EventManager.Server.OnPlayerCapturedStage -= OnPlayerCapturedStage;
					EventManager.Server.OnStageTimedOut -= OnStageTimedOut;

					if (mStageEnableRoutine != null)
						mMachine.mScript.StopCoroutine(mStageEnableRoutine);
				}

				private void OnPlayerHealthHitsZero(CltPlayer dead, IDamageSource damage)
				{
					Transform newPosition = ChooseSafestSpawnPosition(mMachine.mPlayerList, dead, mMachine.mStartPositions);

					if (damage.source is CltPlayer)
					{
						PlayerScore killerScore = mMachine.mPlayerScores[damage.source.netId];
						mMachine.mPlayerScores[damage.source.netId] = new PlayerScore(killerScore.playerId, killerScore.kills + 1, killerScore.deaths);
					}

					PlayerScore deadScore = mMachine.mPlayerScores[dead.netId];
					mMachine.mPlayerScores[dead.netId] = new PlayerScore(deadScore.playerId, deadScore.kills, deadScore.deaths + 1);

					EventManager.Notify(() => EventManager.Server.PlayerDied(dead, damage.source, newPosition));
				}

				public override IState GetTransition()
				{
					return mFinished ? (IState)new GameFinishedState(mMachine) : this;
				}
			}

			/// <summary>
			/// State to hold in after the game timer has completed and
			/// everyone is able to disconnect.
			/// </summary>
			private class GameFinishedState : BaseState<ServerStateMachine>
			{
				public GameFinishedState(ServerStateMachine machine) : base(machine) { }

				public override void OnEnter()
				{
					EventManager.Server.FinishGame(mMachine.mPlayerScores.Values.ToArray());
				}

				public override IState GetTransition()
				{
					return this;
				}
			}
		}
	}
}
