using System;
using FiringSquad.Core.Input;
using FiringSquad.Core.UI;
using FiringSquad.Gameplay;
using KeatsLib.State;
using UnityEngine;

namespace FiringSquad.Core.State
{
	public partial class GamestateManager
	{
		/// <summary>
		/// State used when the game is in the game scene state.
		/// </summary>
		/// <inheritdoc cref="IGameState" />
		private class GameSceneState : BaseStateMachine, IGameState
		{
			public bool safeToTransition { get { return true; } }
			private bool mIsPaused;

			/// <inheritdoc />
			public void OnEnter()
			{
				EventManager.OnInputLevelChanged += HandleInputChange;
				EventManager.Local.OnTogglePause += HandlePauseToggle;

				EventManager.Local.OnLocalPlayerSpawned += HandlePlayerCreated;
				mIsPaused = false;
			}

			private void HandlePlayerCreated(CltPlayer obj)
			{
				ServiceLocator.Get<IInput>().SetInputLevelState(InputLevel.Gameplay | InputLevel.PauseMenu, true);
				SetCursorState(true);

				EventManager.Local.OnLocalPlayerSpawned -= HandlePlayerCreated;
				TransitionStates(new InGameState(this));
			}

			private void HandlePauseToggle()
			{
				if (mIsPaused)
					PopState();
				else
					PushState(new PausedGameState(this));

				mIsPaused = !mIsPaused;
			}

			private void HandleInputChange(InputLevel input, bool state)
			{
				if (input != InputLevel.Gameplay)
					return;

				SetCursorState(state);
			}

			private void SetCursorState(bool hidden)
			{
				Cursor.lockState = hidden ? CursorLockMode.Locked : CursorLockMode.None;
				Cursor.visible = !hidden;
			}

			public new void Update()
			{
				base.Update();
			}

			public void OnExit()
			{
				TransitionStates(new NullState());

				EventManager.OnInputLevelChanged -= HandleInputChange;
				EventManager.Local.OnTogglePause -= HandlePauseToggle;
				EventManager.Local.OnLocalPlayerSpawned -= HandlePlayerCreated;
				SetCursorState(false);
			}

			public IState GetTransition()
			{
				return this; //we never explicitly leave
			}

			private class InGameState : BaseState<GameSceneState>
			{
				public InGameState(GameSceneState machine) : base(machine) { }

				private long mRoundEndTime;
				private BoundProperty<float> mRemainingTime;

				public override void OnEnter()
				{
					EventManager.Local.OnReceiveStartEvent += OnReceiveStartEvent;
					EventManager.Local.OnReceiveFinishEvent += OnReceiveFinishEvent;
				}

				public override void OnExit()
				{
					EventManager.Local.OnReceiveStartEvent -= OnReceiveStartEvent;
					EventManager.Local.OnReceiveFinishEvent -= OnReceiveFinishEvent;
				}

				private void OnReceiveStartEvent(long time)
				{
					mRoundEndTime = time;
					mRemainingTime = new BoundProperty<float>(CalculateRemainingTime(), GameplayUIManager.ARENA_ROUND_TIME);
				}

				private void OnReceiveFinishEvent()
				{
					int myScore = ServiceLocator.Get<IGameplayUIManager>().GetProperty<int>(GameplayUIManager.PLAYER_KILLS).value;
					int myDeaths = ServiceLocator.Get<IGameplayUIManager>().GetProperty<int>(GameplayUIManager.PLAYER_DEATHS).value;

					string resultText;
					if (myScore > myDeaths)
						resultText = "You win!";
					else if (myScore < myDeaths)
						resultText = "You lost.";
					else
						resultText = "It's a tie!";

					EventManager.Notify(() => EventManager.LocalGUI.ShowGameoverPanel(resultText));
					ServiceLocator.Get<IInput>().DisableInputLevel(InputLevel.Gameplay);
				}

				public override void Update()
				{
					if (mRemainingTime == null)
						return;

					mRemainingTime.value = Mathf.Clamp(CalculateRemainingTime(), 0.0f, float.MaxValue);
				}

				private float CalculateRemainingTime()
				{
					long currentTime = DateTime.Now.Ticks;
					long remainingTicks = mRoundEndTime - currentTime;

					return (float)remainingTicks / TimeSpan.TicksPerSecond;
				}

				public override IState GetTransition()
				{
					return this;
				}
			}

			private class PausedGameState : BaseState<GameSceneState>
			{
				public PausedGameState(GameSceneState m) : base(m) { }

				private bool mOriginalGameplayState;

				public override void OnEnter()
				{
					EventManager.Notify(() => EventManager.LocalGUI.TogglePauseMenu(true));

					IInput input = ServiceLocator.Get<IInput>();
					mOriginalGameplayState = input.IsInputEnabled(InputLevel.Gameplay);
					input.DisableInputLevel(InputLevel.Gameplay);
				}

				public override void OnExit()
				{
					EventManager.Notify(() => EventManager.LocalGUI.TogglePauseMenu(false));
					ServiceLocator.Get<IInput>().SetInputLevelState(InputLevel.Gameplay, mOriginalGameplayState);
				}

				public override IState GetTransition()
				{
					return this;
				}
			}
		}
	}
}
