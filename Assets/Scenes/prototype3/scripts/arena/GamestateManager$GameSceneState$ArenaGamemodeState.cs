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

				EventManager.OnPlayerDied += HandlePlayerDeath;
				GeneratePlayerList();
				GenerateSpawnList();
				MovePlayersToSpawn();
			}

			private void GeneratePlayerList()
			{
				mPlayerList = FindObjectsOfType<PlayerScript>().OrderBy(x => x.name).ToArray();
			}

			private void GenerateSpawnList()
			{
				mSpawnPoints = mSettings.spawnPoints;
			}

			private void MovePlayersToSpawn()
			{
				var spawnsCopy = new List<Transform>(mSpawnPoints);
				foreach (PlayerScript player in mPlayerList)
				{
					Transform t = spawnsCopy.ChooseRandom();
					spawnsCopy.Remove(t);

					player.transform.position = t.position;
					player.transform.rotation = t.rotation;
				}
			}

			public new void Update()
			{
				base.Update();
				mRemainingTime.value -= Time.deltaTime;

				if (mRemainingTime.value <= 0.0f)
					HandleMatchEnd();
			}

			private void HandleMatchEnd()
			{
				ServiceLocator.Get<IInput>()
					.DisableInputLevel(Input.InputLevel.Gameplay);
			}

			private void HandlePlayerDeath(ICharacter obj)
			{
				if (ReferenceEquals(obj, mPlayerList[0]))
					mPlayer2Score.value += 1;
				else if (ReferenceEquals(obj, mPlayerList[1]))
					mPlayer1Score.value += 1;
				else
					throw new ArgumentException("We got an invalid player from OnPlayerDied!");

				PlayerScript player = (PlayerScript)obj;
				Transform t = mSpawnPoints.ChooseRandom();

				if (settings.deathParticles != null)
				{
					ParticleSystem ps = Instantiate(settings.deathParticles, player.transform.position, Quaternion.identity)
						.GetComponent<ParticleSystem>();
					ps.Play();
					instance.StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(ps));
				}

				player.transform.position = t.position;
				player.transform.rotation = t.rotation;

				player.ResetArenaPlayer();
			}

			public void OnExit()
			{
				EventManager.OnPlayerDied -= HandlePlayerDeath;
			}

			public IState GetTransition()
			{
				return this;
			}

			private class PlayingState : BaseState<ArenaGamemodeState>
			{
				public PlayingState(ArenaGamemodeState machine) : base(machine) { }

				public override IState GetTransition()
				{
					return this;
				}
			}
		}
	}
}
