using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input = KeatsLib.Unity.Input;

public partial class GamestateManager
{
	/// <summary>
	/// State used when the game is in the game scene state.
	/// </summary>
	private class Prototype2State : BaseGameState
	{
		private IInput mInputRef;

		/// <inheritdoc />
		public override void OnEnter()
		{
			mInputRef = ServiceLocator.Get<IInput>();
			mInputRef.EnableInputLevel(Input.InputLevel.Gameplay);
		}

		public override void Update()
		{
			bool playing = mInputRef.IsInputEnabled(Input.InputLevel.Gameplay);
			Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !playing;
		}
	}
}
