using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// A game console class used to register and issue commands at runtime.
/// </summary>
public class GameConsole : MonoSingleton<GameConsole>, IGameConsole
{
	private bool mCheatsEnabled;

	private Dictionary<string, Action<string[]>> mCommandHandlers;
	[SerializeField] private BaseGameConsoleView mConsoleView;

	[SerializeField] [EnumFlags] private Logger.System mEnabledLogSystems;
	public Logger.System enabledLogLevels { get { return mEnabledLogSystems; } }

	protected override void Awake()
	{
		base.Awake();
		Assert.raiseExceptions = true;

		mCommandHandlers = new Dictionary<string,  Action<string[]>>();
	}

	private void Start()
	{
		Application.logMessageReceived += UnityLogToConsole;

		mConsoleView.RegisterCommandHandler(EnterCommand);
		RegisterCommand("cheats", ToggleCheatsCommand);
		RegisterCommand("help", ListCommandsCommand);
	}

	public void AssertCheatsEnabled()
	{
		Assert.IsTrue(mCheatsEnabled, "Cheats are not enabled at this time.");
	}

	public void RegisterCommand(string command,	 Action<string[]> handle)
	{
		mCommandHandlers.Add(command, handle);
	}

	private void UnityLogToConsole(string message, string stackTrace, LogType type)
	{
		switch (type)
		{
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				mConsoleView.AddMessage(message, LogType.Error);
				break;
			default:
				mConsoleView.AddMessage(message, type);
				break;
		}
	}

	private void EnterCommand(string[] typedData)
	{
		string command = typedData[0].ToLower();
		Action<string[]> handle;

		if (!mCommandHandlers.TryGetValue(command, out handle))
		{
			Debug.LogError("Console command not found: " + command);
			return;
		}

		try
		{
			handle(typedData.Skip(1).ToArray());
			Debug.Log(string.Join(" ", typedData));
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}

	private void ToggleCheatsCommand(string[] data)
	{
		if (data.Length != 1 || data[0] != "0" && data[0] != "1")
			throw new ArgumentException("Invalid parameters for command: cheats");

		switch (data[0])
		{
			case "0":
				mCheatsEnabled = false;
				break;
			case "1":
				mCheatsEnabled = true;
				break;
		}
	}

	private void ListCommandsCommand(string[] data)
	{
		string commands = string.Join("\n", mCommandHandlers.Keys.ToArray());
		Debug.Log(commands);
	}
}
