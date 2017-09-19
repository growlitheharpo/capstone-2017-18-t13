
/// <summary>
/// Base interface for the Gamestate Manager service.
/// Handles the state of the game.
/// </summary>
public interface IGamestateManager
{
	/// <summary>
	/// Whether or not an instance of the GameState Manager exists.
	/// Only the null service will set this to false.
	/// </summary>
	bool isAlive { get; }

	/// <summary>
	/// Request a safe shutdown of the application.
	/// Will do things like save and release resources before
	/// killing the application.
	/// </summary>
	void RequestShutdown();

	/// <summary>
	/// Returns whether or not a feature is currently enabled. For prototyping only.
	/// </summary>
	bool IsFeatureEnabled(GamestateManager.Feature feat);
}
