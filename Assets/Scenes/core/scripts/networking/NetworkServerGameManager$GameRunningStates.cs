using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Weapons;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using FiringSquad.Gameplay.Weapons;
using JetBrains.Annotations;
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
		public const float PLAYER_RESPAWN_TIME = 5f;

		public const int STANDARD_KILL_POINTS = 100;
		public const int HEADSHOT_KILL_POINTS = 25;
		public const int MULTI_KILL_POINTS = 50;
		public const int KILLSTREAK_POINTS = 75;
		public const int KINGSLAYER_POINTS = 75;
		public const int REVENGE_KILL_POINTS = 25;
		public const int STAGE_CAPTURE_POINTS = 100;
		public const int LEGENDARY_PART_POINTS = 50;
		public const int CHEATING_PENALTY_POINTS = 100;

		/// <summary>
		/// Returns the score amount for the flags that have been determined.
		/// </summary>
		/// <param name="killInfoFlags">The relevant flags.</param>
		public static int GetScoreForKillFlags(KillFlags killInfoFlags)
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
				/// Utility struct for saving kill information through the course of each match.
				/// </summary>
				private class PlayerKillServerInfo
				{
					/// <summary>
					/// The time at which this kill occurred.
					/// </summary>
					public float time { get; set; }

					/// <summary>
					/// The length of time since this kill occurred.
					/// </summary>
					public float age { get { return Time.time - time; } }
				}

				/// <summary>
				/// The log of each player's kills.
				/// </summary>
				private class PlayerKillLog
				{
					/// <summary>
					/// A list of the player's kills up to now.
					/// </summary>
					public List<PlayerKillServerInfo> killHistory { get; set; }

					/// <summary>
					/// A running total of this player's kills since their last death.
					/// </summary>
					public int killStreak { get; set; }

					/// <summary>
					/// The exact time of the player's last death
					/// </summary>
					public float lastDeath { get; set; }

					/// <summary>
					/// How much time has elapsed since the player's last death.
					/// </summary>
					public float timeSinceLastDeath { get { return Time.time - lastDeath; } }

					/// <summary>
					/// The log of each player's kills.
					/// </summary>
					public PlayerKillLog()
					{
						killHistory = new List<PlayerKillServerInfo>();
						killStreak = 0;
						lastDeath = 0.0f;
					}
				}

				/// <summary>
				/// State that runs the actual game. Ticks the timer for the game
				/// and handles player deaths during the match by sending them to
				/// a spawn point.
				/// 
				/// Also handles the StageCaptureArea enabling and disabling.
				/// </summary>
				public GameRunningState(ServerStateMachine machine) : base(machine) { }

				private Dictionary<CltPlayer, PlayerKillLog> mKillLogs;
				private List<WeaponPartScript> mCachedLegendaryList;
				private Coroutine mStageEnableRoutine;
				private long mEndTime;
				private bool mFinished;

				/// <inheritdoc />
				public override void OnEnter()
				{
					// Set up everything we need for kill logging.
					mMachine.mPlayerList = FindObjectsOfType<CltPlayer>();

					mKillLogs = new Dictionary<CltPlayer, PlayerKillLog>();
					foreach (CltPlayer player in mMachine.mPlayerList)
						mKillLogs.Add(player, new PlayerKillLog());

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

					StageCaptureArea nextStage = mMachine.mCaptureAreas.Length > 1
						? mMachine.mCaptureAreas.Where(x => x != stage).ChooseRandom() 
						: stage;
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
				private void SpawnLegendaryPart(StageCaptureArea stage, IWeaponBearer player)
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

					StageCaptureArea nextStage = mMachine.mCaptureAreas.Length > 1
						? mMachine.mCaptureAreas.Where(x => x != stage).ChooseRandom()
						: stage;
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

					// Make sure the player isn't still dead from last time
					if (mKillLogs[dead].timeSinceLastDeath < PLAYER_RESPAWN_TIME - 0.5f)
					{
						dead.HealDamage(dead.defaultData.defaultHealth);
						return;
					}

					Transform newPosition = ChooseSafestSpawnPosition(mMachine.mPlayerList, dead, spawnList);

					mMachine.mPlayerScores[dead.netId].deaths++;

					PlayerKill killInfo = new PlayerKill
					{
						killer = damage.source,
						spawnPosition = newPosition,
						mFlags = KillFlags.None
					};

					CltPlayer killer = damage.source as CltPlayer;
					if (killer != null)
					{
						mMachine.mPlayerScores[damage.source.netId].kills++;
						killInfo.mFlags = CalculateFlagsForKill(wasHeadshot, dead, killer);
						mMachine.mPlayerScores[damage.source.netId].score += GetScoreForKillFlags(killInfo.mFlags);
					}

					LogKill(dead, killer);
					EventManager.Server.PlayerDied(dead, killInfo);
				}

				/// <summary>
				/// Calculate the relevant kill flags for what just happened.
				/// </summary>
				/// <param name="wasHeadshot">True if the player reported this to us as a headshot.</param>
				/// <param name="victim">The victim of the kill (the player whose health just reached 0.</param>
				/// <param name="killer">The killer of the kill.</param>
				/// <returns>Appropriate KillFlags based on recent kill history.</returns>
				private KillFlags CalculateFlagsForKill(bool wasHeadshot, CltPlayer victim, [NotNull] CltPlayer killer)
				{
					KillFlags result = KillFlags.None;
					if (wasHeadshot)
						result |= KillFlags.Headshot;

					// Grab all the relevant kill history that we need.
					PlayerKillLog killerLog = mKillLogs[killer];

					// Check if the killer has had more than 3 kills in a 5 second timespan
					if (killerLog.killHistory.Count(x => x.age < 5.0f) > 3)
						result |= KillFlags.Multikill;

					// Check if the killer has had more than 4 kills since last death (+1 because this kill hasn't been logged yet)
					if (killerLog.killStreak + 1 >= 4)
						result |= KillFlags.Killstreak;

					// Check if the killer's last death was less than the respawn timer (i.e., they are currently dead)
					if (killerLog.timeSinceLastDeath < PLAYER_RESPAWN_TIME)
						result |= KillFlags.Revenge;

					// Check if the dead player had a kill streak happening
					if (mKillLogs[victim].killStreak >= 4)
						result |= KillFlags.Kingslayer;

					return result;
				}

				/// <summary>
				/// Log a kill into the history log.
				/// </summary>
				private void LogKill(CltPlayer victim, CltPlayer killer)
				{
					PlayerKillServerInfo serverInfo = new PlayerKillServerInfo { time = Time.time };

					if (killer != null)
					{
						mKillLogs[killer].killHistory.Add(serverInfo);
						mKillLogs[killer].killStreak++;
					}

					if (victim != null)
					{
						mKillLogs[victim].killStreak = 0;
						mKillLogs[victim].killHistory.Clear();
						mKillLogs[victim].lastDeath = Time.time;
					}
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
