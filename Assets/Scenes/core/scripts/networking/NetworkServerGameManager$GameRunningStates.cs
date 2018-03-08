using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Collections;
using KeatsLib.State;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace FiringSquad.Networking
{
	public partial class NetworkServerGameManager 
	{
		private partial class ServerStateMachine 
		{
			// Private data only used in this file:
			private Dictionary<NetworkInstanceId, PlayerScore> mPlayerScores;

			/// <summary>
			/// State that runs the actual game. Ticks the timer for the game
			/// and handles player deaths during the match by sending them to
			/// a spawn point.
			/// 
			/// Also handles the StageCaptureArea enabling and disabling.
			/// </summary>
			private partial class GameRunningState
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
					var spawnList = mMachine.data.currentType == GameData.MatchType.Deathmatch
						? PlayerSpawnPosition.GetAll().Select(x => x.transform).ToList()
						: PlayerSpawnPosition.GetAll(dead.playerTeam).Select(x => x.transform).ToList();

					Transform newPosition = ChooseSafestSpawnPosition(mMachine.mPlayerList, dead, spawnList);

					if (damage.source is CltPlayer)
					{
						PlayerScore killerScore = mMachine.mPlayerScores[damage.source.netId];
						mMachine.mPlayerScores[damage.source.netId] = new PlayerScore(killerScore.playerId, killerScore.kills + 1, killerScore.deaths);
					}

					PlayerScore deadScore = mMachine.mPlayerScores[dead.netId];
					mMachine.mPlayerScores[dead.netId] = new PlayerScore(deadScore.playerId, deadScore.kills, deadScore.deaths + 1);
					
					PlayerKill killInfo = new PlayerKill
					{
						killer = damage.source,
						spawnPosition = newPosition,
						mFlags = KillFlags.None
					};
					EventManager.Server.PlayerDied(dead, killInfo);
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
			private partial class GameFinishedState
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
