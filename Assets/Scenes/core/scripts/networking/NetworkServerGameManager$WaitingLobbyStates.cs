using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using KeatsLib.Collections;
using KeatsLib.State;
using UnityEngine;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Networking
{
	public partial class NetworkServerGameManager 
	{
		private partial class ServerStateMachine 
		{
			/// <summary>
			/// The state we hold in until we have the required number of players
			/// </summary>
			private partial class WaitingForConnectionState
			{
				/// <summary>
				/// The state we hold in until we have the required number of players
				/// </summary>
				public WaitingForConnectionState(ServerStateMachine machine) : base(machine) { }

				private bool mReady;

				/// <inheritdoc />
				public override void OnEnter()
				{
					mMachine.mCaptureAreas = FindObjectsOfType<StageCaptureArea>();
					foreach (StageCaptureArea area in mMachine.mCaptureAreas)
						area.Disable();

					EventManager.Server.OnPlayerJoined += OnPlayerJoined;
					EventManager.Server.OnPlayerHealthHitsZero += OnPlayerHealthHitsZero;
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
						//mPreviousTeam = mPreviousTeam == GameData.PlayerTeam.Orange ? GameData.PlayerTeam.Blue : GameData.PlayerTeam.Orange;
						GameData.PlayerTeam team = newCount > mMachine.data.goalPlayerCount / 2 ? GameData.PlayerTeam.Blue : GameData.PlayerTeam.Orange;
						newPlayer.AssignPlayerTeam(team);
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

					PlayerKill killInfo = new PlayerKill
					{
						killer = null,
						spawnPosition = localSpawn != null ? localSpawn.transform : deadPlayer.transform,
						mFlags = KillFlags.None
					};

					EventManager.Server.PlayerDied(deadPlayer, killInfo);
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
			private partial class StartLobbyState
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
					
					PlayerKill killInfo = new PlayerKill
					{
						killer = null,
						spawnPosition = localSpawn != null ? localSpawn.transform : deadPlayer.transform,
						mFlags = KillFlags.None
					};
					EventManager.Server.PlayerDied(deadPlayer, killInfo);
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
			private partial class StartGameState
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

					if (mMachine.mPlayerList.Length > PlayerSpawnPosition.GetAll().Count)
						Logger.Warn("We have too many players for the number of spawn positions!", Logger.System.Network);

					if (mMachine.data.currentType == GameData.MatchType.Deathmatch)
						SpawnPlayersDeathmatch();
					else
					{
						ForceUpdatePlayerTeams();
						SpawnPlayersTeam();
					}
				}

				/// <summary>
				/// Force Unity's networking to re-send the player's team so that it is reflected
				/// on all clients. Do so by forcing the dirty-bit to be set by using a tmp trash value.
				/// </summary>
				private void ForceUpdatePlayerTeams()
				{
					foreach (CltPlayer player in mMachine.mPlayerList)
					{
						GameData.PlayerTeam team = player.playerTeam;
						player.AssignPlayerTeam(GameData.PlayerTeam.Deathmatch);
						player.AssignPlayerTeam(team);
					}
				}

				/// <summary>
				/// Spawn all players in random positions on the map.
				/// </summary>
				private void SpawnPlayersDeathmatch()
				{
					var spawnCopy = PlayerSpawnPosition.GetAll().Select(x => x.transform).ToList();
					spawnCopy.Shuffle();

					foreach (CltPlayer player in mMachine.mPlayerList)
					{
						Transform target = spawnCopy[spawnCopy.Count - 1];
						spawnCopy.RemoveAt(spawnCopy.Count - 1);

						player.MoveToStartPosition(target.position, target.rotation);
					}
				}

				/// <summary>
				/// Spawn all players in locations appropriate for their assigned team.
				/// </summary>
				private void SpawnPlayersTeam()
				{
					var blue = new Queue<PlayerSpawnPosition>(PlayerSpawnPosition.GetAll(GameData.PlayerTeam.Blue).Shuffle());
					var orange = new Queue<PlayerSpawnPosition>(PlayerSpawnPosition.GetAll(GameData.PlayerTeam.Orange).Shuffle());

					foreach (CltPlayer player in mMachine.mPlayerList)
					{
						var list = player.playerTeam == GameData.PlayerTeam.Blue ? blue : orange;
						Transform target = list.Dequeue().transform;

						player.MoveToStartPosition(target.position, target.rotation);
					}
				}

				/// <inheritdoc />
				public override IState GetTransition()
				{
					return new GameRunningState(mMachine);
				}
			}
		}
	}
}
