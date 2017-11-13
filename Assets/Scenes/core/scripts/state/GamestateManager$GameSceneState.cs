using System;
using FiringSquad.Core.Input;
using FiringSquad.Core.UI;
using FiringSquad.Data;
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
				ServiceLocator.Get<IInput>()
					.EnableInputLevel(InputLevel.Gameplay)
					.EnableInputLevel(InputLevel.PauseMenu);
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
				if (input != InputLevel.HideCursor)
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

				private CltPlayer mLocalPlayerRef;
				private bool mOpenNameMenu;
				private long mRoundEndTime;
				private BoundProperty<float> mRemainingTime;

				public override void OnEnter()
				{
					EventManager.Local.OnReceiveLobbyEndTime += OnReceiveLobbyEndTime;
					EventManager.Local.OnReceiveStartEvent += OnReceiveStartEvent;
					EventManager.Local.OnReceiveFinishEvent += OnReceiveFinishEvent;
				}

				public override void OnExit()
				{
					EventManager.Local.OnReceiveLobbyEndTime -= OnReceiveLobbyEndTime;
					EventManager.Local.OnReceiveStartEvent -= OnReceiveStartEvent;
					EventManager.Local.OnReceiveFinishEvent -= OnReceiveFinishEvent;
				}

				private void OnReceiveLobbyEndTime(CltPlayer player, long time)
				{
					OnReceiveStartEvent(time);
					mLocalPlayerRef = player;
					mOpenNameMenu = true;
				}

				private void OnReceiveStartEvent(long time)
				{
					mRoundEndTime = time;

					if (mRemainingTime == null)
						mRemainingTime = new BoundProperty<float>(CalculateRemainingTime(), GameplayUIManager.ARENA_ROUND_TIME);
					else
						mRemainingTime.value = CalculateRemainingTime();
				}

				private void OnReceiveFinishEvent(PlayerScore[] scores)
				{
					EventManager.Notify(() => EventManager.LocalGUI.ShowGameoverPanel(scores));
					ServiceLocator.Get<IInput>()
						.DisableInputLevel(InputLevel.Gameplay)
						.DisableInputLevel(InputLevel.HideCursor);
				}

				public override void Update()
				{
					if (mOpenNameMenu)
						OpenNameMenu();

					if (mRemainingTime == null)
						return;

					mRemainingTime.value = Mathf.Clamp(CalculateRemainingTime(), 0.0f, float.MaxValue);
				}

				private void OpenNameMenu()
				{
					IInput input = ServiceLocator.Get<IInput>();
					if (!input.IsInputEnabled(InputLevel.Gameplay))
						return;

					mOpenNameMenu = false;
					EventManager.Notify(() => EventManager.LocalGUI.RequestNameChange(mLocalPlayerRef));
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
					input.DisableInputLevel(InputLevel.Gameplay)
						.DisableInputLevel(InputLevel.HideCursor);
				}

				public override void OnExit()
				{
					EventManager.Notify(() => EventManager.LocalGUI.TogglePauseMenu(false));
					ServiceLocator.Get<IInput>()
						.SetInputLevelState(InputLevel.Gameplay, mOriginalGameplayState)
						.SetInputLevelState(InputLevel.HideCursor, mOriginalGameplayState);
				}

				public override IState GetTransition()
				{
					return this;
				}
			}
		}
	}
}
