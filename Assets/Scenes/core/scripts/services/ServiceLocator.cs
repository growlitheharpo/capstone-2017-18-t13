using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : MonoSingleton<ServiceLocator>
{
	private Dictionary<Type, object> mInterfaceMap;

	protected override void Awake()
	{
		base.Awake();
		mInterfaceMap = new Dictionary<Type, object>
		{
			{ typeof(IInput), TryFind<IInput>(KeatsLib.Unity.Input.instance) },
			{ typeof(IGameConsole), TryFind<IGameConsole>(GameConsole.instance) },
			{ typeof(ISaveLoadManager), TryFind<ISaveLoadManager>(SaveLoadManager.instance) },
			{ typeof(IAudioManager), TryFind<IAudioManager>(AudioManager.instance) },
			{ typeof(IGamestateManager), TryFind<IGamestateManager>(GamestateManager.instance) }
		};
	}

	public static T Get<T>() where T : class
	{
#if DEBUG || DEVELOPMENT_BUILD
		object result;
		if (instance.mInterfaceMap.TryGetValue(typeof(T), out result))
			return result as T;

		throw new KeyNotFoundException("Type " + typeof(T).Name + " is not accessible through the service locator.");
#else
		return instance.mInterfaceMap[typeof(T)] as T;
#endif
	}

	private static T TryFind<T>(object inst) where T : class
	{
		return inst as T ?? NullServices.Create<T>();
	}
}
