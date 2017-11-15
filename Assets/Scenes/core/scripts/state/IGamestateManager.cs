using UnityEngine.SceneManagement;

namespace FiringSquad.Core.State
{
	/// <summary>
	/// Base interface for the Gamestate Manager service.
	/// Handles the state of the game.
	/// </summary>
	public interface IGamestateManager : IGlobalService
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
		/// Make a request to change the current scene through the state system.
		/// </summary>
		/// <param name="sceneName"></param>
		/// <param name="mode"></param>
		IGamestateManager RequestSceneChange(string sceneName, LoadSceneMode mode = LoadSceneMode.Single);
	}
}
