using System;
using System.Collections.Generic;
using System.Diagnostics;
using FiringSquad.Core;
using KeatsLib.Collections;

namespace FiringSquad.Debug
{
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
			{ System.Network, "maroon" },
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
			Network = 0x20,
			Generic = 0x10000,
		}

		/// <summary>
		/// Stand-in for Debug.Log().
		/// Prints information to the Unity console.
		/// </summary>
		/// <param name="message">The message to be printed.</param>
		/// <param name="system">The system of origin for the log, if applicable. Will affect color and header.</param>
		[Conditional("INCLUDE_ALL_LOGS")]
		public static void Info(string message, System system = System.Generic)
		{
#if INCLUDE_ALL_LOGS
			if (!CheckLevel(system))
				return;

			var colorPair = GetColorPair(system);
			string label = system == System.Generic ? "Info" : system.ToString();
			UnityEngine.Debug.Log(colorPair.first + label + ": " + message + colorPair.second);
#endif
		}

		/// <summary>
		/// Stand-in for Debug.LogWarning().
		/// Prints information to the Unity console.
		/// </summary>
		/// <param name="message">The message to be printed.</param>
		/// <param name="system">The system of origin for the log, if applicable. Will affect color and header.</param>
		[Conditional("INCLUDE_ALL_LOGS")]
		public static void Warn(string message, System system = System.Generic)
		{
#if INCLUDE_ALL_LOGS
			if (!CheckLevel(system))
				return;

			var colorPair = GetColorPair(system);
			string label = system == System.Generic ? "Info" : system.ToString();
			UnityEngine.Debug.LogWarning(colorPair.first + label + ": " + message + colorPair.second);
#endif
		}

		/// <summary>
		/// Stand-in for Debug.LogError().
		/// Prints information to the Unity console.
		/// </summary>
		/// <param name="message">The message to be printed.</param>
		/// <param name="system">The system of origin for the log, if applicable. Will affect color and header.</param>
		[Conditional("INCLUDE_ALL_LOGS")]
		public static void Error(string message, System system = System.Generic)
		{
#if INCLUDE_ALL_LOGS
			if (!CheckLevel(system))
				return;

			var colorPair = GetColorPair(system);
			string label = system == System.Generic ? "Info" : system.ToString();
			UnityEngine.Debug.LogError(colorPair.first + label + ": " + message + colorPair.second);
#endif
		}
		
		/// <summary>
		/// Returns true if the given system should print to the console based on current settings.
		/// </summary>
		private static bool CheckLevel(System system)
		{
			try
			{
				System level = ServiceLocator.Get<IGameConsole>().enabledLogLevels;
				return (level & system) == system;
			}
			catch (Exception)
			{
				return true;
			}
		}

		private static SystemExtensions.Types.Pair<string, string> GetColorPair(System system)
		{
			string color = COLORS[system];
			return new SystemExtensions.Types.Pair<string, string>("<color=" + color + ">", "</color>");
		}
	}
}
