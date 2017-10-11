﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if !DEBUG && !DEVELOPMENT_BUILD
using UnityEngine;
#endif

/// <summary>
/// A list of game events for this project.
/// </summary>
public partial class EventManager
{
	#region Local-side Events

	public static class Local
	{
		public static event Action<IOptionsData> OnApplyOptionsData = e => { LogEvent(); };

		public static void ApplyOptionsData(IOptionsData data)
		{
			OnApplyOptionsData(data);
		}

		public static event Action OnTogglePause = () => { LogEvent(); };

		public static void TogglePause()
		{
			OnTogglePause();
		}
	}

	public static class LocalGUI
	{
		public static event Action<bool> OnTogglePauseMenu = b => { LogEvent(); };

		public static void TogglePauseMenu(bool visible)
		{
			OnTogglePauseMenu(visible);
		}

		public static event Action<string> OnShowGameoverPanel = s => { LogEvent(); };

		public static void ShowGameoverPanel(string whoWins)
		{
			OnShowGameoverPanel(whoWins);
		}
	}

	#endregion

	#region Server-side Events

	public static class Server
	{
		public static event Action<CltPlayer, List<Ray>> OnPlayerFiredWeapon = (p, s) => { LogEvent(); };

		public static void PlayerFiredWeapon(CltPlayer bearer, List<Ray> shotsFired)
		{
			OnPlayerFiredWeapon(bearer, shotsFired);
		}
	}

	#endregion

	#region Agnostic Events

	public static event Action OnInitialAudioLoadComplete = () => { LogEvent(); };

	public static void InitialAudioLoadComplete()
	{
		OnInitialAudioLoadComplete();
	}

	public static event Action OnInitialPersistenceLoadComplete = () => { LogEvent(); };

	public static void InitialPersistenceLoadComplete()
	{
		OnInitialPersistenceLoadComplete();
	}

	#endregion
	

	public void Start()
	{
		ServiceLocator.Get<IGameConsole>().RegisterCommand("event_fire", FireEventFromConsole);
	}

	private void FireEventFromConsole(string[] args)
	{
#if DEBUG || DEVELOPMENT_BUILD
		string method = args[0];
		MethodInfo info = GetType().GetMethod(method);
		var parameters = info.GetParameters();

		var realParameters = new List<object>();
		for (int i = 0; i < parameters.Length; i++)
		{
			Type type = parameters[i].ParameterType;
			MethodInfo parseMethod = type.GetMethod("Parse",
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

			if (type == typeof(string))
				realParameters.Add(args[i + 1]);
			else if (parseMethod != null)
				realParameters.Add(parseMethod.Invoke(null, new object[] { args[i + 1] }));
			else if (type.IsValueType)
				realParameters.Add(Activator.CreateInstance(type));
			else
				realParameters.Add(null);
		}

		info.Invoke(this, realParameters.ToArray());
#else
		Logger.Error("Firing events is not supported in a non-development build.", Logger.System.Event);
#endif
	}
}
