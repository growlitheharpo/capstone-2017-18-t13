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
	/// <summary>
	/// The main game manager on the server. Handles the current game state
	/// and routes different signals that inform clients of the game state.
	/// </summary>
	public class NetworkServerGameManager : NetworkBehaviour
	{
		/// Inspector variables
		[SerializeField] private ServerGameDefaultData mData;

		/// Private variables
		private ServerStateMachine mStateMachine;

		/// <summary>
		/// Unity function. Called when this class begins on the server.
		/// </summary>
		public override void OnStartServer()
		{
			mStateMachine = new ServerStateMachine(this);
			ServiceLocator.Get<IGameConsole>().RegisterCommand("force-start", CONSOLE_ForceStartGame);
		}

		/// <summary>
		/// Cleanup listeners and handlers.
		/// </summary>
		private void OnDestroy()
		{
			ServiceLocator.Get<IGameConsole>().UnregisterCommand(CONSOLE_ForceStartGame);
		}

		/// <summary>
		/// Server only. Unity's update function.
		/// </summary>
		private void Update()
		{
			mStateMachine.Update();
		}

		/// <summary>
		/// CONSOLE HANDLER: Force start the game from the server console.
		/// </summary>
		private void CONSOLE_ForceStartGame(string[] obj)
		{
			if (obj.Length == 1 && obj[0] == "1")
				mStateMachine.ForceStartGameNow(true);
			else if (obj.Length == 1 && obj[0] == "2")
				mStateMachine.ForceStartGameNow(false);
			else
				throw new ArgumentException("force-start called with invalid parameters.\nUse \"force-start 1\" to start with lobby, or \"force-start 2\" to start the match directly.");
		}

		/// <summary>
		/// Static method that finds the spawn point that is furthest away from all other players.
		/// TODO: Also account for the player's death position.
		/// </summary>
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
			/// <summary>
			/// The state machine for the server's game manager
			/// </summary>
			public ServerStateMachine(NetworkServerGameManager script)
			{
				mScript = script;
				mInGameStartPositions = GameObject.FindGameObjectsWithTag("matchspawn").Select(x => x.transform).ToArray();
				mLobbyStartPositions = FindObjectsOfType<NetworkStartPosition>().Select(x => x.transform).ToArray();
				TransitionStates(new WaitingForConnectionState(this));
			}

			// Private shared data
			private readonly NetworkServerGameManager mScript;
			private readonly Transform[] mInGameStartPositions, mLobbyStartPositions;
			private StageCaptureArea[] mCaptureAreas;
			private CltPlayer[] mPlayerList;
			private Dictionary<NetworkInstanceId, PlayerScore> mPlayerScores;

			// Shortcut to the script's data
			private ServerGameDefaultData data { get { return mScript.mData; } }

			/// <summary>
			/// Force an immediate transition into a non-waiting state.
			/// </summary>
			/// <param name="lobby">True to enter the lobby, false to immediately enter the match.</param>
			public void ForceStartGameNow(bool lobby)
			{
				if (lobby)
					TransitionStates(new StartLobbyState(this));
				else
					TransitionStates(new StartGameState(this));
			}

			/// <summary>
			/// Tick our state machine.
			/// </summary>
			public new void Update()
			{
				base.Update();
			}

			/// <summary>
			/// The state we hold in until we have the required number of players
			/// </summary>
			private class WaitingForConnectionState : BaseState<ServerStateMachine>
			{
				/// <summary>
				/// The state we hold in until we have the required number of players
				/// </summary>
				public WaitingForConnectionState(ServerStateMachine machine) : base(machine) { }

				private GameData.PlayerTeam mPreviousTeam;
				private bool mReady;

				/// <inheritdoc />
				public override void OnEnter()
				{
					mMachine.mCaptureAreas = FindObjectsOfType<StageCaptureArea>();
					foreach (StageCaptureArea area in mMachine.mCaptureAreas)
						area.Disable();

					EventManager.Server.OnPlayerJoined += OnPlayerJoined;
					EventManager.Server.OnPlayerHealthHitsZero += OnPlayerHealthHitsZero;

					mPreviousTeam = GameData.PlayerTeam.Orange;
				}

				/// <inheritdoc />
				public override void OnExit()
				{
					EventManager.Server.OnPlayerJoined -= OnPlayerJoined;
					EventManager.Server.OnPlayerHealthHitsZero -= OnPlayerHealthHitsZero;
				}

				/// <summary>
				/// EVENT HANDLER: Server.OnPlayerJoined
				/// </summary>
				private void OnPlayerJoined(int newCount, CltPlayer newPlayer)
				{
					if (newCount > mMachine.data.goalPlayerCount)
						Logger.Warn("We have too many players in this session!", Logger.System.Network);

					mReady = newCount >= mMachine.data.goalPlayerCount;

					if (mMachine.data.currentType == GameData.MatchType.TeamDeathmatch)
					{
						mPreviousTeam = mPreviousTeam == GameData.PlayerTeam.Orange ? GameData.PlayerTeam.Blue : GameData.PlayerTeam.Orange;
						newPlayer.AssignPlayerTeam(mPreviousTeam);
					}
				}

				/// <summary>
				/// EVENT HANDLER: Server.OnPlayerHealthHitsZero
				/// </summary>
				private void OnPlayerHealthHitsZero(CltPlayer deadPlayer, IDamageSource source)
				{
					Transform localSpawn = mMachine.mLobbyStartPositions
						.OrderBy(x => Vector3.Distance(x.transform.position, deadPlayer.transform.position))
						.FirstOrDefault();

					EventManager.Server.PlayerDied(deadPlayer, null, localSpawn != null ? localSpawn.transform : deadPlayer.transform);
				}

				/// <inheritdoc />
				public override IState GetTransition()
				{
					return !mReady ? this : (IState)new StartLobbyState(mMachine);
				}
			}

			/// <summary>
			/// The state we hold in while waiting for the lobby to begin.
			/// </summary>
			private class StartLobbyState : BaseState<ServerStateMachine>
			{
				/// <summary>
				/// The state we hold in while waiting for the lobby to begin.
				/// </summary>
				public StartLobbyState(ServerStateMachine machine) : base(machine) { }

				private long mEndTime;

				/// <inheritdoc />
				public override void OnEnter()
				{
					mMachine.mPlayerList = FindObjectsOfType<CltPlayer>();
					mEndTime = DateTime.Now.Ticks + mMachine.data.lobbyTime * TimeSpan.TicksPerSecond;

					foreach (CltPlayer player in mMachine.mPlayerList)
						player.TargetStartLobbyCountdown(player.connectionToClient, mEndTime);

					EventManager.Server.OnPlayerHealthHitsZero += OnPlayerHealthHitsZero;
				}

				/// <summary>
				/// EVENT HANDLER: Server.OnPlayerHealthHitsZero
				/// </summary>
				private void OnPlayerHealthHitsZero(CltPlayer deadPlayer, IDamageSource source)
				{
					Transform localSpawn = mMachine.mLobbyStartPositions
						.OrderBy(x => Vector3.Distance(x.transform.position, deadPlayer.transform.position))
						.FirstOrDefault();

					EventManager.Server.PlayerDied(deadPlayer, null, localSpawn != null ? localSpawn.transform : deadPlayer.transform);
				}

				/// <inheritdoc />
				public override void OnExit()
				{
					EventManager.Server.OnPlayerHealthHitsZero -= OnPlayerHealthHitsZero;
				}

				/// <summary>
				/// Returns true if the waiting period for the lobby is over.
				/// </summary>
				private bool IsWaitingTimeOver()
				{
					return DateTime.Now.Ticks >= mEndTime;
				}

				/// <inheritdoc />
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
				/// <summary>
				/// The single-frame state called to set up the players for the round to start.
				/// Resets spawn points and ensures our shared data has an accurate list of players.
				/// </summary>
				public StartGameState(ServerStateMachine machine) : base(machine) { }

				/// <inheritdoc />
				public override void OnEnter()
				{
					mMachine.mPlayerList = FindObjectsOfType<CltPlayer>();

					if (mMachine.mPlayerList.Length > mMachine.mInGameStartPositions.Length)
						Logger.Warn("We have too many players for the number of spawn positions!", Logger.System.Network);

					var spawnCopy = mMachine.mInGameStartPositions.Select(x => x.transform).ToList();
					spawnCopy.Shuffle();

					foreach (CltPlayer player in mMachine.mPlayerList)
					{
						Transform target = spawnCopy[spawnCopy.Count - 1];
						spawnCopy.RemoveAt(spawnCopy.Count - 1);

						player.MoveToStartPosition(target.position, target.rotation);
					}
				}

				/// <inheritdoc />
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
				/// <summary>
				/// State that runs the actual game. Ticks the timer for the game
				/// and handles player deaths during the match by sending them to
				/// a spawn point.
				/// 
				/// Also handles the StageCaptureArea enabling and disabling.
				/// </summary>
				public GameRunningState(ServerStateMachine machine) : base(machine) { }

				private Coroutine mStageEnableRoutine;
				private long mEndTime;
				private bool mFinished;

				/// <inheritdoc />
				public override void OnEnter()
				{
					mEndTime = DateTime.Now.Ticks + mMachine.data.roundTime * TimeSpan.TicksPerSecond;
					EventManager.Server.OnPlayerHealthHitsZero += OnPlayerHealthHitsZero;
					EventManager.Server.OnPlayerCapturedStage += OnPlayerCapturedStage;
					EventManager.Server.OnStageTimedOut += OnStageTimedOut;

					mMachine.mPlayerScores = mMachine.mPlayerList.Select(x => new PlayerScore(x)).ToDictionary(x => x.playerId);

					mStageEnableRoutine = mMachine.mScript.StartCoroutine(EnableStageArea(mMachine.mCaptureAreas.ChooseRandom()));

					EventManager.Notify(() => EventManager.Server.StartGame(mEndTime));
				}

				/// <summary>
				/// EVENT HANDLER: Server.OnPlayerCapturedStage
				/// </summary>
				private void OnPlayerCapturedStage(StageCaptureArea stage, CltPlayer player)
				{
					stage.Disable();

					SpawnLegendaryPart(stage, player);

					StageCaptureArea nextStage = mMachine.mCaptureAreas.Where(x => x != stage).ChooseRandom();
					mStageEnableRoutine = mMachine.mScript.StartCoroutine(EnableStageArea(nextStage));
				}

				/// <summary>
				/// Instantiate a legendary part because a stage has been captured.
				/// </summary>
				private void SpawnLegendaryPart(StageCaptureArea stage, CltPlayer player)
				{
					WeaponPartScript choice;
					if (player.weapon != null)
					{
						WeaponPartCollection currentParts = player.weapon.currentParts;
						var allPossibilities = mMachine.data.stageCaptureParts;
						var possibilities = allPossibilities.Where(x => !currentParts.Contains(x));
						choice = possibilities.ChooseRandom();
					}
					else
						choice = mMachine.data.stageCaptureParts.ChooseRandom();

					GameObject instance = choice.SpawnInWorld();
					instance.transform.position = stage.transform.position + Vector3.up * 45.0f;
					instance.name = choice.name;

					NetworkServer.Spawn(instance);
					mMachine.mScript.StartCoroutine(Coroutines.InvokeAfterFrames(2, () => { instance.GetComponent<WeaponPickupScript>().RpcInitializePickupView(); }));
				}

				/// <summary>
				/// EVENT HANDLER: Server.OnStageTimedOut
				/// </summary>
				private void OnStageTimedOut(StageCaptureArea stage)
				{
					stage.Disable();

					StageCaptureArea nextStage = mMachine.mCaptureAreas.Where(x => x != stage).ChooseRandom();
					mStageEnableRoutine = mMachine.mScript.StartCoroutine(EnableStageArea(nextStage));
				}

				/// <inheritdoc />
				public override void Update()
				{
					if (!mFinished && DateTime.Now.Ticks >= mEndTime)
						mFinished = true;
				}

				/// <summary>
				/// Wait within a random range of seconds, then enable a stage.
				/// </summary>
				private IEnumerator EnableStageArea(StageCaptureArea stage)
				{
					yield return new WaitForSeconds(Random.Range(mMachine.data.minStageWaitTime, mMachine.data.maxStageWaitTime));
					stage.Enable();
				}

				/// <summary>
				/// EVENT HANDLER: Server.OnPlayerHealthHitsZero
				/// </summary>
				private void OnPlayerHealthHitsZero(CltPlayer dead, IDamageSource damage)
				{
					Transform newPosition = ChooseSafestSpawnPosition(mMachine.mPlayerList, dead, mMachine.mInGameStartPositions);

					if (damage.source is CltPlayer)
					{
						PlayerScore killerScore = mMachine.mPlayerScores[damage.source.netId];
						mMachine.mPlayerScores[damage.source.netId] = new PlayerScore(killerScore.playerId, killerScore.kills + 1, killerScore.deaths);
					}

					PlayerScore deadScore = mMachine.mPlayerScores[dead.netId];
					mMachine.mPlayerScores[dead.netId] = new PlayerScore(deadScore.playerId, deadScore.kills, deadScore.deaths + 1);

					EventManager.Notify(() => EventManager.Server.PlayerDied(dead, damage.source, newPosition));
				}

				/// <inheritdoc />
				public override void OnExit()
				{
					EventManager.Server.OnPlayerHealthHitsZero -= OnPlayerHealthHitsZero;
					EventManager.Server.OnPlayerCapturedStage -= OnPlayerCapturedStage;
					EventManager.Server.OnStageTimedOut -= OnStageTimedOut;

					if (mStageEnableRoutine != null)
						mMachine.mScript.StopCoroutine(mStageEnableRoutine);
				}

				/// <inheritdoc />
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
				/// <summary>
				/// State to hold in after the game timer has completed and
				/// everyone is able to disconnect.
				/// </summary>
				public GameFinishedState(ServerStateMachine machine) : base(machine) { }

				/// <inheritdoc />
				public override void OnEnter()
				{
					EventManager.Server.FinishGame(mMachine.mPlayerScores.Values.ToArray());
				}

				/// <inheritdoc />
				public override IState GetTransition()
				{
					return this;
				}
			}
		}
	}
}
