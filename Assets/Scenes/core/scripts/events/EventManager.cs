using FiringSquad.Gameplay;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;

#if !DEBUG && !DEVELOPMENT_BUILD
using UnityEngine;
#endif

/// <summary>
/// A list of game events for this project.
/// </summary>
public partial class EventManager
{
	public static event Action<PlayerScript> OnTogglePauseState = p => { LogEvent(); };

	public static void TogglePauseState(PlayerScript p)
	{
		OnTogglePauseState(p);
	}

	public static event Action<IOptionsData> OnApplyOptionsData = e => { LogEvent(); };

	public static void ApplyOptionsData(IOptionsData data)
	{
		OnApplyOptionsData(data);
	}

	public static event Action OnInitialPersistenceLoadComplete = () => { LogEvent(); };

	public static void InitialPersistenceLoadComplete()
	{
		OnInitialPersistenceLoadComplete();
	}

	public static event Action OnInitialAudioLoadComplete = () => { LogEvent(); };

	public static void InitialAudioLoadComplete()
	{
		OnInitialAudioLoadComplete();
	}

	public static event Action<string, LoadSceneMode> OnRequestSceneChange = (e, m) => { LogEvent(); };

	public static void RequestSceneChange(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
	{
		OnRequestSceneChange(sceneName, mode);
	}

	// PROTOTYPE 2
	public static event Action OnUIToggle = () => { LogEvent(); };

	public static void UIToggle()
	{
		OnUIToggle();
	}

	public static event Action<FiringSquad.Gameplay.WeaponPartScript> OnConfirmPartAttach = e => { LogEvent(); };

	public static void ConfirmPartAttach(FiringSquad.Gameplay.WeaponPartScript newPart)
	{
		OnConfirmPartAttach(newPart);
	}

	public static event Action<BoundProperty, int> OnBoundPropertyCreated = (p, c) => { LogEvent(); };

	public static void BoundPropertyCreated(BoundProperty property, int propertyCode)
	{
		OnBoundPropertyCreated(property, propertyCode);
	}

	public static event Action<BoundProperty> OnBoundPropertyDestroyed = (p) => { LogEvent(); };

	public static void BoundPropertyDestroyed(BoundProperty boundProperty)
	{
		OnBoundPropertyDestroyed(boundProperty);
	}

	public static event Action<ICharacter> OnPlayerDied = p => { LogEvent(); };

	public static void PlayerDied(ICharacter player)
	{
		OnPlayerDied(player);
	}

	public static event Action OnResetLevel = () => { LogEvent(); };

	public static void ResetLevel()
	{
		OnResetLevel();
	}

	public static event Action<ICharacter> OnPlayerKilledEnemy = e => { LogEvent(); };

	public static void PlayerKilledEnemy(ICharacter enemy)
	{
		OnPlayerKilledEnemy(enemy);
	}

	public static event Action<string> OnShowGameoverPanel = e => { LogEvent(); };

	public static void ShowGameoverPanel(string resultText)
	{
		OnShowGameoverPanel(resultText);
	}

	public static event Action<bool, PlayerScript> OnShowPausePanel = (b, r) => { LogEvent(); };

	public static void ShowPausePanel(bool show, PlayerScript requester)
	{
		OnShowPausePanel(show, requester);
	}

	//!PROTOTYPE 2

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
		Debug.LogError("Firing events is not supported in a non-development build.");
#endif
	}
}
