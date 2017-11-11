using System;
using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.Input;
using UnityEngine;
using Input = UnityEngine.Input;

namespace FiringSquad.Debug
{
	/// <inheritdoc cref="BaseGameConsoleView"/>
	public class ImmediateModeConsoleView : BaseGameConsoleView
	{
		/// <summary>
		/// Utility struct that holds important information for a log.
		/// </summary>
		private struct LogEntryHolder
		{
			public string timestamp { get; set; }
			public string message { get; set; }
			public LogType type { get; set; }
		}

		/// Private variables
		private const KeyCode CONSOLE_TOGGLE = KeyCode.BackQuote;
		private const int MAX_LOGS = 25;
		private string mCurrentCommand = "";
		private Queue<LogEntryHolder> mEntries;

		private Action<string[]> mEntryHandler;
		private bool mViewEnabled;
		private Vector2 mViewScrollPosition;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			// ReSharper disable once InconsistentlySynchronizedField  (null object cannot be locked)
			mEntries = new Queue<LogEntryHolder>(MAX_LOGS);
		}

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetKeyDown, CONSOLE_TOGGLE, INPUT_ToggleConsole, InputLevel.None);
		}

		/// <summary>
		/// INPUT HANDLER: Toggle the debug game console.
		/// </summary>
		private void INPUT_ToggleConsole()
		{
			ToggleConsole();
		}

		/// <inheritdoc />
		public override void ClearLogs()
		{
			lock (mEntries)
				mEntries.Clear();
		}

		/// <inheritdoc />
		public override void AddMessage(string message, LogType messageType)
		{
			lock (mEntries)
			{
				while (mEntries.Count >= MAX_LOGS)
					mEntries.Dequeue();

				mEntries.Enqueue(new LogEntryHolder
				{
					timestamp = DateTime.Now.ToString("h:mm:ss tt"),
					message = message,
					type = messageType
				});

				ForceScrollToBottom();
			}
		}

		/// <inheritdoc />
		public override void RegisterCommandHandler(Action<string[]> handler)
		{
			mEntryHandler = handler;
		}

		/// <inheritdoc />
		public override void ToggleConsole()
		{
			mViewEnabled = !mViewEnabled;
			IInput input = ServiceLocator.Get<IInput>();
			input.SetInputLevelState(InputLevel.DevConsole, mViewEnabled);
			input.SetInputLevelState(InputLevel.Gameplay, !mViewEnabled);
			input.SetInputLevelState(InputLevel.HideCursor, !mViewEnabled);

			ForceScrollToBottom();
		}

		/// <summary>
		/// Send a command to our registered handler.
		/// </summary>
		private void SendCommand()
		{
			var command = mCurrentCommand.Split(' ');
			mCurrentCommand = "";
			mEntryHandler(command);
		}

		/// <summary>
		/// Force our scroll position to the bottom.
		/// </summary>
		private void ForceScrollToBottom()
		{
			mViewScrollPosition = Vector2.positiveInfinity;
		}

		/// <summary>
		/// Unity GUI loop.
		/// </summary>
		private void OnGUI()
		{
			if (!mViewEnabled)
				return;

			float baseX = Screen.width, baseY = Screen.height;
			lock (mEntries)
				DrawLogs(baseX, baseY);

			DrawEntryBox(baseX, baseY);
		}

		/// <summary>
		/// Called from immediate mode OnGUI. Draws the debug console logs.
		/// </summary>
		private void DrawLogs(float baseX, float baseY)
		{
			Rect consoleRect = new Rect(10.0f, baseY * 0.5f, baseX - 20.0f, baseY * 0.45f);
			GUI.Box(consoleRect, "Developer Console");

			Rect layoutRect = new Rect(consoleRect.x - 2.0f, consoleRect.y + 10.0f, consoleRect.width - 4.0f,
				consoleRect.height - 15.0f);
			GUILayout.BeginArea(layoutRect);
			mViewScrollPosition = GUILayout.BeginScrollView(mViewScrollPosition, false, true);

			foreach (LogEntryHolder log in mEntries)
			{
				GUILayout.BeginHorizontal();

				Color defaultColor = GUI.color;
				switch (log.type)
				{
					case LogType.Error:
						GUI.color = Color.red;
						GUILayout.Label("Error " + log.timestamp + ": ");
						break;
					case LogType.Warning:
						GUI.color = Color.yellow;
						GUILayout.Label("Warning " + log.timestamp + ": ");
						break;
					default:
						GUILayout.Label("Info " + log.timestamp + ": ");
						break;
				}

				GUI.color = defaultColor;

				GUILayout.Label(log.message);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		/// <summary>
		/// Called from immediate mode OnGUI. Draws the text entry box.
		/// </summary>
		private void DrawEntryBox(float baseX, float baseY)
		{
			if (Event.current.type == EventType.keyDown)
			{
				if (Event.current.keyCode == KeyCode.Return)
					SendCommand();
				else if (Event.current.keyCode == CONSOLE_TOGGLE)
					ToggleConsole();
			}

			Rect entryRect = new Rect(10.0f, baseY * 0.95f, baseX - 20.0f, baseY * 0.045f);
			GUI.SetNextControlName("ConsoleEntry");
			mCurrentCommand = GUI.TextField(entryRect, mCurrentCommand);
			GUI.FocusControl("ConsoleEntry");
		}
	}
}
