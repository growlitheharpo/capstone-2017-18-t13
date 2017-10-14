using KeatsLib.State;
using UnityEngine;
using Input = KeatsLib.Unity.Input;

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
			
			ServiceLocator.Get<IInput>().SetInputLevelState(Input.InputLevel.Gameplay | Input.InputLevel.PauseMenu, true);
			SetCursorState(true);

			TransitionStates(new InGameState(this));
			
			mIsPaused = false;
		}

		private void HandlePauseToggle()
		{
			if (mIsPaused)
				PopState();
			else
				PushState(new PausedGameState(this));

			mIsPaused = !mIsPaused;
		}

		private void HandleInputChange(Input.InputLevel input, bool state)
		{
			if (input != Input.InputLevel.Gameplay)
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
			SetCursorState(false);
		}

		public IState GetTransition()
		{
			return this; //we never explicitly leave
		}

		private class InGameState : BaseState<GameSceneState>
		{
			public InGameState(GameSceneState machine) : base(machine) { }

			private float mRoundEndTime;
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

			private void OnReceiveStartEvent(float obj)
			{
				mRoundEndTime = obj;
				float remainingTime = Mathf.Clamp(mRoundEndTime - (float)Network.time, 0.0f, float.MaxValue);
				mRemainingTime = new BoundProperty<float>(remainingTime, GameplayUIManager.ARENA_ROUND_TIME);
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
				ServiceLocator.Get<IInput>().DisableInputLevel(Input.InputLevel.Gameplay);
			}

			public override void Update()
			{
				if (mRemainingTime == null)
					return;

				float remainingTime = mRoundEndTime - (float)Network.time;
				mRemainingTime.value = remainingTime;
			}

			public override IState GetTransition()
			{
				return this;
			}
		}

		private class PausedGameState : BaseState<GameSceneState>
		{
			public PausedGameState(GameSceneState m) : base(m) {}

			private bool mOriginalGameplayState;

			public override void OnEnter()
			{
				EventManager.Notify(() => EventManager.LocalGUI.TogglePauseMenu(true));

				IInput input = ServiceLocator.Get<IInput>();
				mOriginalGameplayState = input.IsInputEnabled(Input.InputLevel.Gameplay);
				input.DisableInputLevel(Input.InputLevel.Gameplay);
			}

			public override void OnExit()
			{
				EventManager.Notify(() => EventManager.LocalGUI.TogglePauseMenu(false));
				ServiceLocator.Get<IInput>().SetInputLevelState(Input.InputLevel.Gameplay, mOriginalGameplayState);
			}

			public override IState GetTransition()
			{
				return this;
			}
		}
	}
}
