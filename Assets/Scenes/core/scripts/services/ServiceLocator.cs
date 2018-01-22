using System;
using System.Collections.Generic;
using FiringSquad.Core.Audio;
using FiringSquad.Core.Input;
using FiringSquad.Core.State;
using FiringSquad.Core.UI;
using FiringSquad.Core.Weapons;
using FiringSquad.Debug;

namespace FiringSquad.Core
{
	/// <summary>
	/// Gets instances of the main game services at runtime.
	/// Preferable to using the singleton instance directly because
	/// it will prevent crashes and null references at runtime
	/// when a service does not exist.
	/// </summary>
	public class ServiceLocator : MonoSingleton<ServiceLocator>
	{
		/// Private variables
		private Dictionary<Type, IGlobalService> mInterfaceMap;

		/// <summary>
		/// Unity's Awake function.
		/// The ServiceLocator is last in the script execution order, ensuring that all instances are instantiated first.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			mInterfaceMap = new Dictionary<Type, IGlobalService>
			{
				{ typeof(IInput), TryFind<IInput>(Input.Input.instance) },
				{ typeof(IGameConsole), TryFind<IGameConsole>(GameConsole.instance) },
				{ typeof(IAudioManager), TryFind<IAudioManager>(AudioManager.instance) },
				{ typeof(IGamestateManager), TryFind<IGamestateManager>(GamestateManager.instance) },
				{ typeof(IUIManager), TryFind<IUIManager>(UIManager.instance) },
				{ typeof(IWeaponPartManager), TryFind<IWeaponPartManager>(WeaponPartManager.instance) },
			};
		}

		/// <summary>
		/// Get a global game service.
		/// </summary>
		/// <typeparam name="T">The type of service to fetch.</typeparam>
		/// <returns>A fully functional service of the provided interface.</returns>
		public static T Get<T>() where T : class, IGlobalService
		{
#if DEBUG || DEVELOPMENT_BUILD
			IGlobalService result;
			if (instance.mInterfaceMap.TryGetValue(typeof(T), out result))
				return result as T;

			throw new KeyNotFoundException("Type " + typeof(T).Name + " is not accessible through the service locator.");
#else
		return instance.mInterfaceMap[typeof(T)] as T;
#endif
		}

		/// <summary>
		/// Checks if the provided instance exists. If not, creates a NullService representation.
		/// </summary>
		/// <typeparam name="T">The service interface we are searching for.</typeparam>
		/// <param name="inst">The potential concrete implementation of this service to check.</param>
		/// <returns>A concrete implementation of this service, which may or may not be the provided parameter.</returns>
		private static T TryFind<T>(object inst) where T : class
		{
			return inst as T ?? NullServices.Create<T>();
		}
	}
}
