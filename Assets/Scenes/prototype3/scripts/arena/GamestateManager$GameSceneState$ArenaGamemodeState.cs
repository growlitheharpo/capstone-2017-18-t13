using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Gameplay;
using KeatsLib.Collections;
using KeatsLib.State;
using KeatsLib.Unity;
using UnityEngine;

public partial class GamestateManager
{
	private partial class GameSceneState
	{
		private class ArenaGamemodeState : BaseStateMachine, IState
		{
			private Gamemode.ArenaSettings mSettings;
			public Gamemode.ArenaSettings settings { get { return mSettings; } }

			private BoundProperty<int> mPlayer1Score;
			private BoundProperty<int> mPlayer2Score;
			private BoundProperty<float> mRemainingTime;
			private PlayerScript[] mPlayerList;
			private Transform[] mSpawnPoints;

			public ArenaGamemodeState(GameSceneState m)
			{
				mSettings = FindObjectOfType<Gamemode>().arenaSettings;
			}

			public void OnEnter()
			{
				mPlayer1Score = new BoundProperty<int>(0, GameplayUIManager.PLAYER1_SCORE);
				mPlayer2Score = new BoundProperty<int>(0, GameplayUIManager.PLAYER2_SCORE);
				mRemainingTime = new BoundProperty<float>(mSettings.roundTime, GameplayUIManager.ARENA_ROUND_TIME);

				TransitionStates(new StartGameState(this));
			}

			public new void Update()
			{
				base.Update();
			}

			public void OnExit()
			{
				currentState.OnExit();

				mPlayer1Score.Cleanup();
				mPlayer2Score.Cleanup();
				mRemainingTime.Cleanup();
			}

			public IState GetTransition()
			{
				return this;
			}

			#region States

			private class StartGameState : BaseState<ArenaGamemodeState>
			{
				public StartGameState(ArenaGamemodeState machine) : base(machine) { }

				public override void OnEnter()
				{
					mMachine.mRemainingTime.value = mMachine.settings.roundTime;
					mMachine.mPlayer1Score.value = 0;
					mMachine.mPlayer2Score.value = 0;

					GeneratePlayerList();
					GenerateSpawnList();

					MovePlayersToSpawn();
				}

				private void GeneratePlayerList()
				{
					mMachine.mPlayerList = FindObjectsOfType<PlayerScript>().OrderBy(x => x.name).ToArray();
				}

				private void GenerateSpawnList()
				{
					mMachine.mSpawnPoints = mMachine.mSettings.spawnPoints;
				}

				private void MovePlayersToSpawn()
				{
					var spawnsCopy = new List<Transform>(mMachine.mSpawnPoints);
					foreach (PlayerScript player in mMachine.mPlayerList)
					{
						Transform t = spawnsCopy.ChooseRandom();
						spawnsCopy.Remove(t);

						player.transform.position = t.position;
						player.transform.rotation = t.rotation;
					}
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
					mMachine.mRemainingTime.value -= Time.deltaTime;
				}

				private void HandlePlayerDeath(ICharacter obj)
				{
					if (ReferenceEquals(obj, mMachine.mPlayerList[0]))
						mMachine.mPlayer2Score.value += 1;
					else if (ReferenceEquals(obj, mMachine.mPlayerList[1]))
						mMachine.mPlayer1Score.value += 1;
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

					player.ResetArenaPlayer();
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

				public override IState GetTransition()
				{
					return this;
				}
			}
			#endregion
		}
	}
}
