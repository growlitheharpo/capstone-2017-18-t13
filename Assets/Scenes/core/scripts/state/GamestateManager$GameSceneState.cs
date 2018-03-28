using System;
using System.Collections.Generic;
using FiringSquad.Core.Audio;
using FiringSquad.Core.Input;
using FiringSquad.Core.UI;
using FiringSquad.Data;
using FiringSquad.Gameplay;
using FiringSquad.Gameplay.UI;
using KeatsLib.State;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Core.State
{
	public partial class GamestateManager
	{
		/// <summary>
		/// State used when the game is in a gameplay scene.
		/// </summary>
		/// <inheritdoc cref="IGameState" />
		private class GameSceneState : BaseStateMachine, IGameState
		{
			/// Private variables
			private bool mIsPaused;

			/// <inheritdoc />
			public bool safeToTransition { get { return true; } }

			/// <inheritdoc />
			public void OnEnter()
			{
				EventManager.Local.OnInputLevelChanged += OnInputLevelChanged;
				EventManager.Local.OnTogglePause += OnTogglePause;
				EventManager.Local.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
				EventManager.Local.OnIntroBegin += OnIntroBegin;
				mIsPaused = false;
			}

			/// <summary>
			/// EVENT HANDLER: Local.OnLocalPlayerSpawned
			/// Save a reference to the local player.
			/// </summary>
			private void OnLocalPlayerSpawned(CltPlayer player)
			{
				ServiceLocator.Get<IInput>()
					.EnableInputLevel(InputLevel.Gameplay)
					.EnableInputLevel(InputLevel.PauseMenu);
				SetCursorState(true);

				EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
				TransitionStates(new InGameState(this));
			}

			/// <summary>
			/// EVENT HANDLER: Local.OnTogglePause
			/// Pushes a new state onto the state machine stack.
			/// </summary>
			private void OnTogglePause()
			{
				if (mIsPaused)
					PopState();
				else
					PushState(new PausedGameState(this));

				mIsPaused = !mIsPaused;
			}

			/// <summary>
			/// EVENT HANDLER: Local.OnIntroBegin
			/// </summary>
			private void OnIntroBegin()
			{
				if (mIsPaused)
					PopState();
			}

			/// <summary>
			/// EVENT HANDLER: Local.OnInputLevelChanged
			/// Update the cursor state if this input affects it.
			/// </summary>
			private void OnInputLevelChanged(InputLevel input, bool state)
			{
				if (input != InputLevel.HideCursor)
					return;

				SetCursorState(state);
			}

			/// <summary>
			/// Update whether or not Unity should hide the cursor.
			/// </summary>
			/// <param name="hidden">Whether or not to hide the cursor.</param>
			private void SetCursorState(bool hidden)
			{
				Cursor.lockState = hidden ? CursorLockMode.Locked : CursorLockMode.None;
				Cursor.visible = !hidden;
			}

			/// <inheritdoc />
			public new void Update()
			{
				base.Update();
			}

			/// <inheritdoc />
			public void OnExit()
			{
				TransitionStates(new NullState());

				EventManager.Local.OnInputLevelChanged -= OnInputLevelChanged;
				EventManager.Local.OnTogglePause -= OnTogglePause;
				EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
				EventManager.Local.OnIntroBegin -= OnIntroBegin;
				SetCursorState(false);
			}

			/// <inheritdoc />
			public IState GetTransition()
			{
				return this; //we never explicitly leave
			}

			/// <summary>
			/// The inner state to run when we are not paused.
			/// </summary>
			private class InGameState : BaseState<GameSceneState>
			{
				/// <summary>
				/// The inner state to run when we are not paused.
				/// </summary>
				public InGameState(GameSceneState machine) : base(machine) { }

				/// Private variables
				private bool mPlayedSoundTimeWarning;
				private bool mPlayedSoundSponsor;
				private bool mInRealGame;
				private long mRoundEndTime;
				private BoundProperty<float> mRemainingTime;

				/// <inheritdoc />
				public override void OnEnter()
				{
					EventManager.Local.OnReceiveLobbyEndTime += OnReceiveLobbyEndTime;
					EventManager.Local.OnReceiveGameEndTime += OnReceiveGameEndTime;
					EventManager.Local.OnReceiveFinishEvent += OnReceiveFinishEvent;
					EventManager.Local.OnConfirmQuitGame += OnConfirmQuitGame;
					EventManager.Local.OnTeamVictoryScreen += OnTeamVictoryScreen;

				}

				/// <inheritdoc />
				public override void OnExit()
				{
					EventManager.Local.OnReceiveLobbyEndTime -= OnReceiveLobbyEndTime;
					EventManager.Local.OnReceiveGameEndTime -= OnReceiveGameEndTime;
					EventManager.Local.OnReceiveFinishEvent -= OnReceiveFinishEvent;
					EventManager.Local.OnConfirmQuitGame -= OnConfirmQuitGame;
					EventManager.Local.OnTeamVictoryScreen -= OnTeamVictoryScreen;

				}

				/// <summary>
				/// EVENT HANDLER: Local.OnReceiveLobbyEndTime
				/// Update the UI accordingly.
				/// </summary>
				private void OnReceiveLobbyEndTime(CltPlayer player, long time)
				{
					mInRealGame = false;
					OnReceiveGameEndTime(time);
				}

				/// <summary>
				/// EVENT HANDLER: Local.OnReceiveGameEndTime
				/// Update the UI accordingly.
				/// </summary>
				private void OnReceiveGameEndTime(long time)
				{
					mInRealGame = true;
					mRoundEndTime = time;

					if (mRemainingTime == null)
						mRemainingTime = new BoundProperty<float>(CalculateRemainingTime(), UIManager.ARENA_ROUND_TIME);
					else
						mRemainingTime.value = CalculateRemainingTime();
				}

				/// <summary>
				/// 
				/// </summary>
				private void OnTeamVictoryScreen(IList<PlayerScore> scores)
				{
					IScreenPanel panel = ServiceLocator.Get<IUIManager>()
						.PushNewPanel(ScreenPanelTypes.TeamVictory);
					((TeamVictoryPanel)panel).TallyScores(scores);

				}

				/// <summary>
				/// EVENT HANDLER: Local.OnReceiveFinishEvent
				/// Update the UI accordingly.
				/// </summary>
				private void OnReceiveFinishEvent(IList<PlayerScore> scores)
				{
					IScreenPanel panel = ServiceLocator.Get<IUIManager>()
						.PushNewPanel(ScreenPanelTypes.GameOver);
					((GameOverPanel)panel).SetDisplayScores(scores);
				}

				/// <summary>
				/// EVENT HANDLER: Local.OnConfirmQuitGame
				/// Shutdown any network processes and go back to the menu.
				/// </summary>
				private void OnConfirmQuitGame()
				{
					NetworkManager.singleton.StopHost();
					NetworkManager.Shutdown();

					ServiceLocator.Get<IGamestateManager>()
						.RequestSceneChange(MENU_SCENE);
				}

				/// <inheritdoc />
				public override void Update()
				{
					if (mRemainingTime == null)
						return;

					mRemainingTime.value = Mathf.Clamp(CalculateRemainingTime(), 0.0f, float.MaxValue);

					if (mInRealGame && mRemainingTime.value < 120 & mPlayedSoundSponsor == false)
					{
						ServiceLocator.Get<IAudioManager>()
							.CreateSound(AudioEvent.AnnouncerSponsor, null);
						mPlayedSoundSponsor = true;
					}

					if (mInRealGame && mRemainingTime.value < 30 & mPlayedSoundTimeWarning == false)
					{
						ServiceLocator.Get<IAudioManager>()
							.CreateSound(AudioEvent.AnnouncerTimeWarning, null);
						mPlayedSoundTimeWarning = true;
					}
				}

				/// <summary>
				/// Calculate the remaining time in seconds based on our current Tick data.
				/// </summary>
				/// <returns>The remaining time in seconds.</returns>
				private float CalculateRemainingTime()
				{
					long currentTime = DateTime.Now.Ticks;
					long remainingTicks = mRoundEndTime - currentTime;

					return (float)remainingTicks / TimeSpan.TicksPerSecond;
				}

				/// <inheritdoc />
				public override IState GetTransition()
				{
					return this;
				}
			}

			/// <summary>
			/// The inner state to run when the game is paused.
			/// Disables input and enables UI on enter, flips these on exit.
			/// </summary>
			private class PausedGameState : BaseState<GameSceneState>
			{
				/// <summary>
				/// The inner state to run when the game is paused.
				/// Disables input and enables UI on enter, flips these on exit.
				/// </summary>
				public PausedGameState(GameSceneState m) : base(m) { }

				/// <inheritdoc />
				public override void OnEnter()
				{
					ServiceLocator.Get<IUIManager>().PushNewPanel(ScreenPanelTypes.Pause);
				}

				/// <inheritdoc />
				public override void OnExit()
				{
					ServiceLocator.Get<IUIManager>().PopPanel(ScreenPanelTypes.Pause);
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
