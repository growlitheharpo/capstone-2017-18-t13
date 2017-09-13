using UnityEngine;
using Input = KeatsLib.Unity.Input;

public partial class GamestateManager
{
	/// <summary>
	/// State used when the game is in the game scene state.
	/// </summary>
	/// <inheritdoc />
	private class Prototype2State : BaseGameState
	{
		/// <inheritdoc />
		public override void OnEnter()
		{
			EventManager.OnInputLevelChanged += HandleInputChange;

			bool state = ServiceLocator.Get<IInput>().IsInputEnabled(Input.InputLevel.Gameplay);
			SetCursorState(state);
		}

		private void HandleInputChange(Input.InputLevel input, bool state)
		{
			if (input != Input.InputLevel.Gameplay)
				return;

			SetCursorState(state);
		}

		private void SetCursorState(bool playing)
		{
			Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !playing;
		}

		public override void OnExit()
		{
			EventManager.OnInputLevelChanged -= HandleInputChange;
			SetCursorState(false);
		}
	}
}
