using KeatsLib.State;
using UnityEngine;
using Input = KeatsLib.Unity.Input;

public partial class GamestateManager
{
	/// <summary>
	/// State used when the game is in the game scene state.
	/// </summary>
	/// <inheritdoc cref="IGameState" />
	private partial class GameSceneState : BaseStateMachine, IGameState
	{
		public bool safeToTransition { get { return true; } }
		private bool mIsPaused;
		
		/// <inheritdoc />
		public void OnEnter()
		{
			EventManager.OnInputLevelChanged += HandleInputChange;
			EventManager.OnTogglePauseState += HandlePauseToggle;
			
			ServiceLocator.Get<IInput>().SetInputLevelState(Input.InputLevel.Gameplay | Input.InputLevel.PauseMenu, true);
			SetCursorState(true);
			
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
			EventManager.OnTogglePauseState -= HandlePauseToggle;
			SetCursorState(false);
		}

		public IState GetTransition()
		{
			return this; //we never explicitly leave
		}

		private class PausedGameState : BaseState<GameSceneState>
		{
			public PausedGameState(GameSceneState m) : base(m)
			{
			}

			private bool mOriginalGameplayState;

			public override void OnEnter()
			{
				EventManager.Notify(() => EventManager.ShowPausePanel(true));

				IInput input = ServiceLocator.Get<IInput>();
				mOriginalGameplayState = input.IsInputEnabled(Input.InputLevel.Gameplay);
				input.DisableInputLevel(Input.InputLevel.Gameplay);
			}

			public override void OnExit()
			{
				EventManager.Notify(() => EventManager.ShowPausePanel(false));
				ServiceLocator.Get<IInput>().SetInputLevelState(Input.InputLevel.Gameplay, mOriginalGameplayState);
			}

			public override IState GetTransition()
			{
				return this;
			}
		}
	}
}
