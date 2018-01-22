using System;
using FiringSquad.Core;

namespace FiringSquad.Debug
{
	/// <summary>
	/// The base interface for the Game Console service.
	/// </summary>
	public interface IGameConsole : IGlobalService
	{
		/// <summary>
		/// Immediately throws an error if cheats are not enabled.
		/// </summary>
		void AssertCheatsEnabled();

		/// <summary>
		/// Register a command to the console.
		/// </summary>
		/// <param name="command">The name of the command.</param>
		/// <param name="handle">The action to call with the command parameters.</param>
		IGameConsole RegisterCommand(string command, Action<string[]> handle);

		/// <summary>
		/// Remove a command from the console.
		/// </summary>
		/// <param name="command">The name of the command.</param>
		IGameConsole UnregisterCommand(string command);

		/// <summary>
		/// Remove a command from the console.
		/// </summary>
		/// <param name="handle">The action to call with the command parameters.</param>
		IGameConsole UnregisterCommand(Action<string[]> handle);

		/// <summary>
		/// Returns the current systems that are allowed to log.
		/// </summary>
		Logger.System enabledLogLevels { get; }
	}
}
