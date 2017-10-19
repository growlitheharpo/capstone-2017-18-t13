using System;
using UnityEngine;

namespace FiringSquad.Debug
{
	/// <summary>
	/// A base class for a console view. Allows switching the console between
	/// modern Unity UI and immediate mode UI for instance.
	/// </summary>
	public abstract class BaseGameConsoleView : MonoBehaviour
	{
		/// <summary>
		/// Clear all of the saved logs.
		/// </summary>
		public abstract void ClearLogs();

		/// <summary>
		/// Post a message to the console view.
		/// </summary>
		/// <param name="message">The new message/log.</param>
		/// <param name="messageType">The type for this log.</param>
		public abstract void AddMessage(string message, LogType messageType);

		/// <summary>
		/// Register a handler for commands being entered into this console.
		/// </summary>
		public abstract void RegisterCommandHandler(Action<string[]> handler);

		/// <summary>
		/// Enable/disable this console.
		/// </summary>
		public abstract void ToggleConsole();
	}
}
