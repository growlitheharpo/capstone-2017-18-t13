using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Gameplay;
using KeatsLib.Collections;
using KeatsLib.State;
using KeatsLib.Unity;
using UnityEngine;
using Input = KeatsLib.Unity.Input;

public partial class GamestateManager
{
	private partial class GameSceneState
	{
		private class ArenaGamemodeState : BaseStateMachine, IState
		{
			private Gamemode.ArenaSettings mSettings;
			public Gamemode.ArenaSettings settings { get { return mSettings; } }

			private BoundProperty<int> mPlayerKills;
			private BoundProperty<int> mPlayerDeaths;
			private BoundProperty<float> mRemainingTime;
			private Transform[] mSpawnPoints;

			private PlayerScript mLocalPlayer;
			private long mRoundEndTime;

			public ArenaGamemodeState(GameSceneState m)
			{
				mSettings = FindObjectOfType<Gamemode>().arenaSettings;
			}

			public void OnEnter()
			{
				mPlayerKills = new BoundProperty<int>(0, GameplayUIManager.PLAYER_KILLS);
				mPlayerDeaths = new BoundProperty<int>(0, GameplayUIManager.PLAYER_DEATHS);
				mRemainingTime = new BoundProperty<float>(mSettings.roundTime, GameplayUIManager.ARENA_ROUND_TIME);

				TransitionStates(new WaitingForNetworkState(this));
			}

			public new void Update()
			{
				base.Update();
			}

			public void OnExit()
			{
				if (currentState != null)
					currentState.OnExit();

				mPlayerKills.Cleanup();
				mPlayerDeaths.Cleanup();
				mRemainingTime.Cleanup();
			}

			public IState GetTransition()
			{
				return this;
			}

			private float CalculateTimer()
			{
				long now = DateTime.UtcNow.Ticks;
				long span = mRoundEndTime - now;

				return (float)span / TimeSpan.TicksPerSecond;
			}

			#region States

			private class WaitingForNetworkState : BaseState<ArenaGamemodeState>
			{
				public WaitingForNetworkState(ArenaGamemodeState machine) : base(machine) { }

				private bool mReady;

				public override void OnEnter()
				{
					mReady = false;
					EventManager.OnAllPlayersReady += HandleAllPlayersReady;
				}

				private void HandleAllPlayersReady(long time)
				{
					mReady = true;
					mMachine.mRoundEndTime = time;
				}

				public override void OnExit()
				{
					EventManager.OnAllPlayersReady -= HandleAllPlayersReady;
				}

				public override IState GetTransition()
				{
					if (mReady)
						return new StartGameState(mMachine);

					return this;
				}
			}

			private class StartGameState : BaseState<ArenaGamemodeState>
			{
				public StartGameState(ArenaGamemodeState machine) : base(machine) { }

				public override void OnEnter()
				{
					mMachine.mRemainingTime.value = mMachine.CalculateTimer();
					mMachine.mPlayerKills.value = 0;
					mMachine.mPlayerDeaths.value = 0;

					GeneratePlayerList();
					GenerateSpawnList();

					MovePlayersToSpawn();
				}

				private void GeneratePlayerList()
				{
					//mMachine.mPlayerList = FindObjectsOfType<PlayerScript>().OrderBy(x => x.name).ToArray();

					var players = FindObjectsOfType<PlayerScript>();
					mMachine.mLocalPlayer = players.First(x => x.isLocalPlayer);
				}

				private void GenerateSpawnList()
				{
					mMachine.mSpawnPoints = mMachine.mSettings.spawnPoints;
				}

				private void MovePlayersToSpawn()
				{
					/*var spawnsCopy = new List<Transform>(mMachine.mSpawnPoints);
					foreach (PlayerScript player in mMachine.mPlayerList)
					{
						Transform t = spawnsCopy.ChooseRandom();
						spawnsCopy.Remove(t);

						player.transform.position = t.position;
						player.transform.rotation = t.rotation;
					}*/

					// TODO: Make this decided by the server to make sure players don't start in the same place!
					mMachine.mLocalPlayer.transform.position = mMachine.mSpawnPoints.ChooseRandom().position;
				}

				public override IState GetTransition()
				{
					return new PlayingState(mMachine);
				}
			}

			private class PlayingState : BaseState<ArenaGamemodeState>
			{
				public PlayingState(ArenaGamemodeState machine) : base(machine) { }

				public override void OnEnter()
				{
					EventManager.OnPlayerDied += HandlePlayerDeath;
				}

				public override void Update()
				{
					mMachine.mRemainingTime.value = mMachine.CalculateTimer();
				}

				private void HandlePlayerDeath(ICharacter obj)
				{
					/*if (ReferenceEquals(obj, mMachine.mPlayerList[0]))
						mMachine.mPlayerDeaths.value += 1;
					else if (ReferenceEquals(obj, mMachine.mPlayerList[1]))
						mMachine.mPlayerKills.value += 1;
					else
						throw new ArgumentException("We got an invalid player from OnPlayerDied!");

					PlayerScript player = (PlayerScript)obj;
					Transform t = mMachine.mSpawnPoints.ChooseRandom();

					if (mMachine.settings.deathParticles != null)
					{
						ParticleSystem ps = Instantiate(mMachine.settings.deathParticles, player.transform.position, Quaternion.identity)
							.GetComponent<ParticleSystem>();
						ps.Play();
						instance.StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(ps));
					}

					player.transform.position = t.position;
					player.transform.rotation = t.rotation;

					player.ResetArenaPlayer();*/
					if (!ReferenceEquals(obj, mMachine.mLocalPlayer))
					{
						mMachine.mPlayerKills.value += 1;
						return;
					}

					Transform s;
					do
					{
						s = mMachine.mSpawnPoints.ChooseRandom();
					} while (Vector3.Distance(s.position, mMachine.mLocalPlayer.transform.position) < 5.0f);


					mMachine.mLocalPlayer.transform.position = s.position;
					mMachine.mLocalPlayer.transform.rotation = s.rotation;

					mMachine.mLocalPlayer.ResetArenaPlayer();
					mMachine.mPlayerDeaths.value += 1;
				}

				public override IState GetTransition()
				{
					return (mMachine.mRemainingTime.value <= 0.0f) ? (IState)new EndMatchState(mMachine) : this;
				}

				public override void OnExit()
				{
					EventManager.OnPlayerDied -= HandlePlayerDeath;
				}
			}

			private class EndMatchState : BaseState<ArenaGamemodeState>
			{
				public EndMatchState(ArenaGamemodeState machine) : base(machine) { }

				public override void OnEnter()
				{
					string resultText;
					if (mMachine.mPlayerKills.value > mMachine.mPlayerDeaths.value)
						resultText = "You win!";
					else if (mMachine.mPlayerKills.value < mMachine.mPlayerDeaths.value)
						resultText = "You lost.";
					else
						resultText = "It's a tie!";

					EventManager.Notify(() => EventManager.ShowGameoverPanel(resultText));
					ServiceLocator.Get<IInput>().DisableInputLevel(Input.InputLevel.Gameplay);
				}

				public override IState GetTransition()
				{
					return this;
				}
			}
			#endregion
		}
	}
}
