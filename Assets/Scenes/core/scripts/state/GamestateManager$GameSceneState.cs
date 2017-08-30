using KeatsLib.Unity;

public partial class GamestateManager
{
	private class GameSceneState : BaseGameState
	{
		public override void OnEnter()
		{
			ServiceLocator.Get<IInput>()
				.EnableInputLevel(Input.InputLevel.Gameplay);
		}
	}
}
