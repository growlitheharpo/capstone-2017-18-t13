using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Weapons;
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
		public const float PLAYER_RESPAWN_TIME = 4.5f;

		public const int STANDARD_KILL_POINTS = 100;
		public const int HEADSHOT_KILL_POINTS = 25;
		public const int MULTI_KILL_POINTS = 50;
		public const int KILLSTREAK_POINTS = 75;
		public const int KINGSLAYER_POINTS = 75;
		public const int REVENGE_KILL_POINTS = 25;
		public const int STAGE_CAPTURE_POINTS = 100;
		public const int LEGENDARY_PART_POINTS = 50;
		public const int CHEATING_PENALTY_POINTS = 100;

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

				private List<WeaponPartScript> mCachedLegendaryList;
				private Coroutine mStageEnableRoutine;
				private long mEndTime;
				private bool mFinished;

				/// <inheritdoc />
				public override void OnEnter()
				{
					mCachedLegendaryList = ServiceLocator.Get<IWeaponPartManager>()
						.GetAllPrefabScripts(false)
						.Select(x => x.Value)
						.Where(x => x.isLegendary)
						.ToList();

					mMachine.mPlayerScores = mMachine.mPlayerList.Select(x => new PlayerScore(x)).ToDictionary(x => x.playerId);

					EventManager.Server.OnPlayerHealthHitsZero += OnPlayerHealthHitsZero;
					EventManager.Server.OnPlayerCapturedStage += OnPlayerCapturedStage;
					EventManager.Server.OnPlayerAttachedPart += OnPlayerAttachedPart;
					EventManager.Server.OnPlayerCheated += OnPlayerCheated;
					EventManager.Server.OnStageTimedOut += OnStageTimedOut;

					int roundLength = mMachine.data.roundTime;
					if (!mMachine.mForceSkipIntro)
						roundLength += (int)INTRO_BUFFER_FOR_SPAWN;

					mEndTime = DateTime.Now.Ticks + roundLength * TimeSpan.TicksPerSecond;

					mStageEnableRoutine = mMachine.mScript.StartCoroutine(EnableStageArea(mMachine.mCaptureAreas.ChooseRandom(), mMachine.data.initialStageWait));

					EventManager.Notify(() => EventManager.Server.StartGame(mEndTime));
				}

				/// <inheritdoc />
				public override void OnExit()
				{
					EventManager.Server.OnPlayerHealthHitsZero -= OnPlayerHealthHitsZero;
					EventManager.Server.OnPlayerCapturedStage -= OnPlayerCapturedStage;
					EventManager.Server.OnPlayerAttachedPart -= OnPlayerAttachedPart;
					EventManager.Server.OnPlayerCheated -= OnPlayerCheated;
					EventManager.Server.OnStageTimedOut -= OnStageTimedOut;

					if (mStageEnableRoutine != null)
						mMachine.mScript.StopCoroutine(mStageEnableRoutine);
				}

				/// <summary>
				/// EVENT HANDLER: Server.OnPlayerCapturedStage
				/// </summary>
				private void OnPlayerCapturedStage(StageCaptureArea stage, IList<CltPlayer> players)
				{
					stage.Disable();

					CltPlayer firstPlayer = players[players.Count - 1];
					SpawnLegendaryPart(stage, firstPlayer);

					foreach (CltPlayer p in players)
						mMachine.mPlayerScores[p.netId].score += STAGE_CAPTURE_POINTS;

					StageCaptureArea nextStage = mMachine.mCaptureAreas.Where(x => x != stage).ChooseRandom();
					mStageEnableRoutine = mMachine.mScript.StartCoroutine(EnableStageArea(nextStage));
				}

				/// <summary>
				/// EVENT HANDLER: Server.OnPlayerAttachedPart
				/// </summary>
				private void OnPlayerAttachedPart(BaseWeaponScript weapon, WeaponPartScript partInstance)
				{
					if (partInstance.isLegendary)
						mMachine.mPlayerScores[weapon.bearer.netId].score += LEGENDARY_PART_POINTS;
				}

				/// <summary>
				/// EVENT HANDLER: Server.OnPlayerCheated
				/// </summary>
				/// <param name="obj"></param>
				private void OnPlayerCheated(CltPlayer obj)
				{
					mMachine.mPlayerScores[obj.netId].score -= CHEATING_PENALTY_POINTS;
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
						var allPossibilities = mCachedLegendaryList;
						var possibilities = allPossibilities.Where(x => !currentParts.Contains(x));
						choice = possibilities.ChooseRandom();
					}
					else
						choice = mCachedLegendaryList.ChooseRandom();

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
				private IEnumerator EnableStageArea(StageCaptureArea stage, float time = -1.0f)
				{
					time = time > 0.0f ? time : Random.Range(mMachine.data.minStageWaitTime, mMachine.data.maxStageWaitTime);
					yield return new WaitForSeconds(time);
					stage.Enable();
				}

				/// <summary>
				/// EVENT HANDLER: Server.OnPlayerHealthHitsZero
				/// </summary>
				private void OnPlayerHealthHitsZero(CltPlayer dead, IDamageSource damage, bool wasHeadshot)
				{
					var spawnList = mMachine.data.currentType == GameData.MatchType.Deathmatch
						? PlayerSpawnPosition.GetAll().Select(x => x.transform).ToList()
						: PlayerSpawnPosition.GetAll(dead.playerTeam).Select(x => x.transform).ToList();

					Transform newPosition = ChooseSafestSpawnPosition(mMachine.mPlayerList, dead, spawnList);


					mMachine.mPlayerScores[dead.netId].deaths++;

					PlayerKill killInfo = new PlayerKill
					{
						killer = damage.source,
						spawnPosition = newPosition,
						mFlags = KillFlags.None
					};

					if (damage.source is CltPlayer)
					{
						mMachine.mPlayerScores[damage.source.netId].kills++;
						killInfo.mFlags = CalculateFlagsForKill(killInfo, wasHeadshot);
						mMachine.mPlayerScores[damage.source.netId].score += GetScoreForFlags(killInfo.mFlags);
					}

					LogKill(killInfo);

					EventManager.Server.PlayerDied(dead, killInfo);
				}

				private KillFlags CalculateFlagsForKill(PlayerKill killInfo, bool wasHeadshot)
				{
					KillFlags result = KillFlags.None;
					if (wasHeadshot)
						result |= KillFlags.Headshot;
						
					return result;
				}
				
				private int GetScoreForFlags(KillFlags killInfoFlags)
				{
					int score = STANDARD_KILL_POINTS;
					if ((killInfoFlags & KillFlags.Kingslayer) > 0)
						score += KINGSLAYER_POINTS;
					if ((killInfoFlags & KillFlags.Multikill) > 0)
						score += MULTI_KILL_POINTS;
					if ((killInfoFlags & KillFlags.Headshot) > 0)
						score += HEADSHOT_KILL_POINTS;
					if ((killInfoFlags & KillFlags.Killstreak) > 0)
						score += KILLSTREAK_POINTS;
					if ((killInfoFlags & KillFlags.Revenge) > 0)
						score += REVENGE_KILL_POINTS;

					return score;
				}

				private void LogKill(PlayerKill killInfo)
				{
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
