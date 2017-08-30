using System;
using System.Collections.Generic;
using UnityEngine;

public class ImmediateModeConsoleView : BaseGameConsoleView
{
	private struct LogEntryHolder
	{
		public string timestamp { get; set; }
		public string message { get; set; }
		public LogType type { get; set; }
	}

	private const int MAX_LOGS = 25;
	private string mCurrentCommand = "";
	private Queue<LogEntryHolder> mEntries;

	private Action<string[]> mEntryHandler;
	private bool mViewEnabled;
	private Vector2 mViewScrollPosition;

	private void Awake()
	{
		// ReSharper disable once InconsistentlySynchronizedField  (null object cannot be locked)
		mEntries = new Queue<LogEntryHolder>(MAX_LOGS);
	}

	private void Start()
	{
		ServiceLocator.Get<IInput>()
			.RegisterInput(Input.GetKeyDown, KeyCode.BackQuote, INPUT_ToggleConsole, KeatsLib.Unity.Input.InputLevel.None);
	}

	private void INPUT_ToggleConsole()
	{
		ToggleConsole();
	}

	public override void ClearLogs()
	{
		lock (mEntries)
			mEntries.Clear();
	}

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

	public override void RegisterCommandHandler(Action<string[]> handler)
	{
		mEntryHandler = handler;
	}

	public override void ToggleConsole()
	{
		mViewEnabled = !mViewEnabled;
		ServiceLocator.Get<IInput>().SetInputLevelState(KeatsLib.Unity.Input.InputLevel.DevConsole, mViewEnabled);

		ForceScrollToBottom();

		//TODO: Issue pause toggle here.
	}

	private void SendCommand()
	{
		var command = mCurrentCommand.Split(' ');
		mCurrentCommand = "";
		mEntryHandler(command);
	}

	private void ForceScrollToBottom()
	{
		mViewScrollPosition = Vector2.positiveInfinity;
	}
	
	private void OnGUI()
	{
		if (!mViewEnabled)
			return;

		lock (mEntries)
		{
			float baseX = Screen.width, baseY = Screen.height;
			DrawLogs(baseX, baseY);
			DrawEntryBox(baseX, baseY);
		}
	}

	private void DrawEntryBox(float baseX, float baseY)
	{
		if (Event.current.type == EventType.keyDown)
		{
			if (Event.current.keyCode == KeyCode.Return)
				SendCommand();
			else if (Event.current.keyCode == KeyCode.BackQuote)
				ToggleConsole();
		}

		Rect entryRect = new Rect(10.0f, baseY * 0.95f, baseX - 20.0f, baseY * 0.045f);
		GUI.SetNextControlName("ConsoleEntry");
		mCurrentCommand = GUI.TextField(entryRect, mCurrentCommand);
		GUI.FocusControl("ConsoleEntry");
	}

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
}
