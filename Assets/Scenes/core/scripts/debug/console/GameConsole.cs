using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Assertions;

namespace FiringSquad.Debug
{
	/// <inheritdoc cref="IGameConsole"/>
	public class GameConsole : MonoSingleton<GameConsole>, IGameConsole
	{
		/// Inspector variables
		[SerializeField] private BaseGameConsoleView mConsoleView;
		[SerializeField] [EnumFlags] private Logger.System mEnabledLogSystems;

		/// Private variables
		private Dictionary<string, Action<string[]>> mCommandHandlers;
		private bool mCheatsEnabled;

		/// <inheritdoc />
		public Logger.System enabledLogLevels { get { return mEnabledLogSystems; } }

		/// <inheritdoc/>
		protected override void Awake()
		{
			base.Awake(); // initialize the MonoSingleton.

			Assert.raiseExceptions = true;
			mCommandHandlers = new Dictionary<string, Action<string[]>>();
		}

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			Application.logMessageReceived += UnityLogToConsole;

			mConsoleView.RegisterCommandHandler(EnterCommand);
			RegisterCommand("cheats", ToggleCheatsCommand);
			RegisterCommand("clear", ClearConsoleCmmand);
			RegisterCommand("help", ListCommandsCommand);
		}

		/// <inheritdoc />
		public void AssertCheatsEnabled()
		{
			Assert.IsTrue(mCheatsEnabled, "Cheats are not enabled at this time.");
		}

		/// <inheritdoc />
		public IGameConsole RegisterCommand(string command, Action<string[]> handle)
		{
			mCommandHandlers[command] = handle;
			return this;
		}

		/// <inheritdoc />
		public IGameConsole UnregisterCommand(string command)
		{
			mCommandHandlers.Remove(command);
			return this;
		}

		/// <inheritdoc />
		public IGameConsole UnregisterCommand(Action<string[]> handle)
		{
			var keys = mCommandHandlers.Where(x => x.Value == handle).ToArray();
			foreach (var k in keys)
				mCommandHandlers.Remove(k.Key);

			return this;
		}

		/// <summary>
		/// Event called when Unity receives a Debug.Log call or variant.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="stackTrace"></param>
		/// <param name="type"></param>
		private void UnityLogToConsole(string message, string stackTrace, LogType type)
		{
			switch (type)
			{
				// Treat Assert, and Exception as errors. The rest are handled as themselves.
				case LogType.Assert:
				case LogType.Exception:
					mConsoleView.AddMessage(message, LogType.Error);
					break;
				default:
					mConsoleView.AddMessage(message, type);
					break;
			}
		}

		/// <summary>
		/// Callback for when a command is entered into the console.
		/// </summary>
		private void EnterCommand(string[] typedData)
		{
			string command = typedData[0].ToLower();
			Action<string[]> handle;

			if (!mCommandHandlers.TryGetValue(command, out handle))
			{
				UnityEngine.Debug.LogError("Console command not found: " + command);
				return;
			}

			try
			{
				handle(typedData.Skip(1).ToArray());
				UnityEngine.Debug.Log(string.Join(" ", typedData));
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		/// <summary>
		/// Handler for the "cheats [0/1]" command.
		/// </summary>
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

		/// <summary>
		/// Handler for the "help" command.
		/// </summary>
		private void ListCommandsCommand(string[] data)
		{
			string commands = string.Join("\n", mCommandHandlers.Keys.ToArray());
			UnityEngine.Debug.Log(commands);
		}

		/// <summary>
		/// Handler for the "clear" command.
		/// </summary>
		private void ClearConsoleCmmand(string[] data)
		{
			if (data.Length > 0)
				throw new ArgumentException("Invalid parameters for command: clear");

			mConsoleView.ClearLogs();
		}
	}
}
