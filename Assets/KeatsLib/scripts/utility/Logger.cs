using System;
using System.Collections.Generic;
using KeatsLib.Collections;
using UnityEngine;

/// <summary>
/// Unity Debug.Log wrapper.
/// Allows easy colorization and toggling.
/// </summary>
public static class Logger
{
	private static readonly Dictionary<System, string> COLORS = new Dictionary<System, string>
	{
		{ System.State, "teal" },
		{ System.Audio, "blue" },
		{ System.Event, "purple" },
		{ System.Services, "olive" },
		{ System.Input, "orange" },
		{ System.Generic, "grey" },
	};

	[Flags]
	public enum System
	{
		State = 0x1,
		Audio = 0x2,
		Event = 0x4,
		Services = 0x8,
		Input = 0x10,
		Generic = 0x10000,
	}

	/// <summary>
	/// Stand-in for Debug.Log().
	/// Prints information to the Unity console.
	/// </summary>
	/// <param name="message">The message to be printed.</param>
	/// <param name="system">The system of origin for the log, if applicable. Will affect color and header.</param>
	public static void Info(string message, System system = System.Generic)
	{
		if (!CheckLevel(system))
			return;

		var colorPair = GetColorPair(system);
		string label = system == System.Generic ? "Info" : system.ToString();
		Debug.Log(colorPair.first + label + ": " + message + colorPair.second);
	}
	
	/// <summary>
	/// Stand-in for Debug.LogWarning().
	/// Prints information to the Unity console.
	/// </summary>
	/// <param name="message">The message to be printed.</param>
	/// <param name="system">The system of origin for the log, if applicable. Will affect color and header.</param>
	public static void Warn(string message, System system = System.Generic)
	{
		if (!CheckLevel(system))
			return;

		var colorPair = GetColorPair(system);
		string label = system == System.Generic ? "Info" : system.ToString();
		Debug.LogWarning(colorPair.first + label + ": " + message + colorPair.second);
	}
	
	/// <summary>
	/// Stand-in for Debug.LogError().
	/// Prints information to the Unity console.
	/// </summary>
	/// <param name="message">The message to be printed.</param>
	/// <param name="system">The system of origin for the log, if applicable. Will affect color and header.</param>
	public static void Error(string message, System system = System.Generic)
	{
		if (!CheckLevel(system))
			return;

		var colorPair = GetColorPair(system);
		string label = system == System.Generic ? "Info" : system.ToString();
		Debug.LogError(colorPair.first + label + ": " + message + colorPair.second);
	}

	private static bool CheckLevel(System system)
	{
		System level = ServiceLocator.Get<IGameConsole>().enabledLogLevels;
		return (level & system) == system;
	}

	private static SystemExtensions.Types.Pair<string, string> GetColorPair(System system)
	{
		string color = COLORS[system];
		return new SystemExtensions.Types.Pair<string, string>("<color=" + color + ">", "</color>");
	}
}
