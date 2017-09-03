using KeatsLib.Unity;

public partial class GamestateManager
{
	/// <summary>
	/// State used when the game is in the game scene state.
	/// </summary>
	private class GameSceneState : BaseGameState
	{
		/// <inheritdoc />
		public override void OnEnter()
		{
			ServiceLocator.Get<IInput>()
				.EnableInputLevel(Input.InputLevel.Gameplay);
		}
	}
}
