using System;
using UnityEngine;

/// <summary>
/// A base class for a console view. Allows switching the console between
/// modern Unity UI and immediate mode UI for instance.
/// </summary>
public abstract class BaseGameConsoleView : MonoBehaviour
{
	public abstract void ClearLogs();
	public abstract void AddMessage(string message, LogType messageType);
	public abstract void RegisterCommandHandler(Action<string[]> handler);

	public abstract void ToggleConsole();
}
