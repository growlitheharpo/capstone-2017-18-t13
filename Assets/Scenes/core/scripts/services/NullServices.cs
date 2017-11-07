﻿using System;
using System.Collections.Generic;
using FiringSquad.Core.Audio;
using FiringSquad.Core.Input;
using FiringSquad.Core.SaveLoad;
using FiringSquad.Core.State;
using FiringSquad.Core.UI;
using FiringSquad.Core.Weapons;
using FiringSquad.Debug;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Core
{
	/// <summary>
	/// Generates null services for the ServiceLocator when instances are not found.
	/// </summary>
	public class NullServices
	{
		/// <summary>
		/// Create an instance of a service from the provided interface type.
		/// </summary>
		/// <typeparam name="T">The type of service to be created.</typeparam>
		public static T Create<T>() where T : class
		{
			if (typeof(T) == typeof(IGameConsole))
				return new NullConsole() as T;
			if (typeof(T) == typeof(IInput))
				return new NullInput() as T;
			if (typeof(T) == typeof(ISaveLoadManager))
				return new NullSaveLoadManager() as T;
			if (typeof(T) == typeof(IAudioManager))
				return new NullAudioManager() as T;
			if (typeof(T) == typeof(IGamestateManager))
				return new NullGamestateManager() as T;
			if (typeof(T) == typeof(IGameplayUIManager))
				return new NullGameplayUIManager() as T;
			if (typeof(T) == typeof(IWeaponPartManager))
				return new NullWeaponPartManager() as T;

			return null;
		}

		public class NullWeaponPartManager : IWeaponPartManager
		{
			public WeaponPartScript GetPrefabScript(string id)
			{
				Logger.Info("NULL SERVICE: NullWeaponPartManager.GetPrefabScript()", Logger.System.Services);
				return null;
			}

			public GameObject GetPartPrefab(string id)
			{
				Logger.Info("NULL SERVICE: NullWeaponPartManager.GetPartPrefab()", Logger.System.Services);
				return null;
			}

			public GameObject this[string index]
			{
				get
				{
					Logger.Info("NULL SERVICE: NullWeaponPartManager[partId]", Logger.System.Services);
					return null;
				}
			}

			public Dictionary<string, GameObject> GetAllPrefabs(bool includeDebug)
			{
				Logger.Info("NULL SERVICE: NullWeaponPartManager.GetAllPrefabs()", Logger.System.Services);
				return new Dictionary<string, GameObject>();
			}

			public Dictionary<string, WeaponPartScript> GetAllPrefabScripts(bool includeDebug)
			{
				Logger.Info("NULL SERVICE: NullWeaponPartManager.GetAllPrefabScripts()", Logger.System.Services);
				return new Dictionary<string, WeaponPartScript>();
			}
		}

		private class NullGamestateManager : IGamestateManager
		{
			public bool isAlive { get { return false; } }

			public void RequestShutdown()
			{
				Logger.Info("NULL SERVICE: NullGamestateManager.RequestShutdown()", Logger.System.Services);
			}

			public IGamestateManager RequestSceneChange(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
			{
				Logger.Info("NULL SERVICE: NullGamestateManager.RequestSceneChange()", Logger.System.Services);
				return this;
			}
		}

		private class NullAudioManager : IAudioManager
		{
			private class NullAudioReference : IAudioReference
			{
				public IAudioReference Start()
				{
					return this;
				}

				public IAudioReference Kill(bool allowFade = true)
				{
					return this;
				}

				public IAudioReference SetVolume(float vol)
				{
					return this;
				}

				public IAudioReference AttachToRigidbody(Rigidbody rb)
				{
					return this;
				}

				public bool isPlaying { get { return false; } }
				public float playerSpeed { get { return default(float); } set { } }
				public float weaponType { get { return default(float); } set { } }

				public IAudioReference SetParameter(string name, float value)
				{
					return this;
				}

				public float GetParameter(string name)
				{
					return default(float);
				}
			}

			public void InitializeDatabase()
			{
				Logger.Info("NULL SERVICE: IAudioManager.InitializeDatabase()", Logger.System.Services);
				EventManager.Notify(EventManager.InitialAudioLoadComplete);
			}

			public IAudioReference CreateSound(AudioEvent e, Transform location, bool autoPlay = true)
			{
				Logger.Info("NULL SERVICE: IAudioManager.CreateSound()", Logger.System.Services);
				return new NullAudioReference();
			}

			public IAudioReference CreateSound(AudioEvent e, Transform location, Vector3 offset, Space offsetType = Space.Self, bool autoPlay = true)
			{
				Logger.Info("NULL SERVICE: IAudioManager.CreateSound()", Logger.System.Services);
				return new NullAudioReference();
			}

			// ReSharper disable once RedundantAssignment
			public IAudioReference CheckReferenceAlive(ref IAudioReference reference)
			{
				reference = null;
				return null;
			}
		}

		private class NullConsole : IGameConsole
		{
			public void AssertCheatsEnabled()
			{
				Logger.Info("NULL SERVICE: IGameConsole.AssertCheatsEnabled()", Logger.System.Services);
			}

			public IGameConsole RegisterCommand(string command, Action<string[]> handle)
			{
				Logger.Info("NULL SERVICE: IGameConsole.RegisterCommand()", Logger.System.Services);
				return this;
			}

			public IGameConsole UnregisterCommand(string command)
			{
				return this;
			}

			public IGameConsole UnregisterCommand(Action<string[]> handle)
			{
				return this;
			}

			public Logger.System enabledLogLevels
			{
				get
				{
					Logger.Info("NULL SERVICE: IGameConsole.enabledLogLevels", Logger.System.Services);
					return (Logger.System)int.MaxValue;
				}
			}
		}

		private class NullInput : IInput
		{
			public IInput RegisterInput<T>(Func<T, bool> method, T key, Action command, InputLevel level, bool allowOtherKeys = true)
			{
				Logger.Info("NULL SERVICE: IInput.RegisterInput()", Logger.System.Services);
				return this;
			}

			public IInput UnregisterInput(Action command)
			{
				Logger.Info("NULL SERVICE: IInput.UnregisterInput()", Logger.System.Services);
				return this;
			}

			public IInput RegisterAxis(
				Func<string, float> method, string axis, Action<float> command, InputLevel level, bool allowOtherAxes = true)
			{
				Logger.Info("NULL SERVICE: IInput.RegisterAxis()", Logger.System.Services);
				return this;
			}

			public IInput UnregisterAxis(Action<float> command)
			{
				Logger.Info("NULL SERVICE: IInput.UnregisterAxis()", Logger.System.Services);
				return this;
			}

			public void SetInputLevel(InputLevel level)
			{
				Logger.Info("NULL SERVICE: IInput.SetInputLevel()", Logger.System.Services);
			}

			public void SetInputLevelState(InputLevel level, bool state)
			{
				Logger.Info("NULL SERVICE: IInput.SetInputLevelState()", Logger.System.Services);
			}

			public void EnableInputLevel(InputLevel level)
			{
				Logger.Info("NULL SERVICE: IInput.EnableInputLevel()", Logger.System.Services);
			}

			public void DisableInputLevel(InputLevel level)
			{
				Logger.Info("NULL SERVICE: IInput.DisableInputLevel()", Logger.System.Services);
			}

			public bool IsInputEnabled(InputLevel level)
			{
				Logger.Info("NULL SERVICE: IInput.IsInputEnabled()", Logger.System.Services);
				return false;
			}
		}

		private class NullSaveLoadManager : ISaveLoadManager
		{
			private Persistence mFakePersistence;

			public Persistence persistentData
			{
				get
				{
					Logger.Info("NULL SERVICE: ISaveLoadManager.persistentData", Logger.System.Services);
					return mFakePersistence;
				}
			}

			public void LoadData()
			{
				Logger.Info("NULL SERVICE: ISaveLoadManager.LoadData()", Logger.System.Services);
				mFakePersistence = Persistence.Create("");
				EventManager.Notify(EventManager.InitialPersistenceLoadComplete);
			}
		}

		private class NullGameplayUIManager : IGameplayUIManager
		{
			public BoundProperty<T> GetProperty<T>(int hash)
			{
				Logger.Info("NULL SERVICE: NullGameplayUIManager.GetProperty<T>()", Logger.System.Services);
				return null;
			}

			public void BindProperty(int hash, BoundProperty prop)
			{
				Logger.Info("NULL SERVICE: NullGameplayUIManager.BindProperty()", Logger.System.Services);
			}

			public void UnbindProperty(BoundProperty prop)
			{
				Logger.Info("NULL SERVICE: NullGameplayUIManager.UnbindProperty()", Logger.System.Services);
			}
		}
	}
}
