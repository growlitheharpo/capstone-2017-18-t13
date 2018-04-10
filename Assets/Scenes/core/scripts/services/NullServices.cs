using System;
using System.Collections.Generic;
using FiringSquad.Core.Audio;
using FiringSquad.Core.Input;
using FiringSquad.Core.State;
using FiringSquad.Core.UI;
using FiringSquad.Core.Weapons;
using FiringSquad.Debug;
using FiringSquad.Gameplay.Weapons;
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
			if (typeof(T) == typeof(IAudioManager))
				return new NullAudioManager() as T;
			if (typeof(T) == typeof(IGamestateManager))
				return new NullGamestateManager() as T;
			if (typeof(T) == typeof(IUIManager))
				return new NullUIManager() as T;
			if (typeof(T) == typeof(IWeaponPartManager))
				return new NullWeaponPartManager() as T;

			return null;
		}

		/// <inheritdoc />
		public class NullWeaponPartManager : IWeaponPartManager
		{
			/// <inheritdoc />
			public WeaponPartScript GetPrefabScript(byte id)
			{
				Logger.Info("NULL SERVICE: NullWeaponPartManager.GetPrefabScript()", Logger.System.Services);
				return null;
			}

			/// <inheritdoc />
			public GameObject GetPartPrefab(byte id)
			{
				Logger.Info("NULL SERVICE: NullWeaponPartManager.GetPartPrefab()", Logger.System.Services);
				return null;
			}

			/// <inheritdoc />
			public GameObject this[byte index]
			{
				get
				{
					Logger.Info("NULL SERVICE: NullWeaponPartManager[partId]", Logger.System.Services);
					return null;
				}
			}

			/// <inheritdoc />
			public Dictionary<byte, GameObject> GetAllPrefabs(bool includeDebug)
			{
				Logger.Info("NULL SERVICE: NullWeaponPartManager.GetAllPrefabs()", Logger.System.Services);
				return new Dictionary<byte, GameObject>();
			}

			/// <inheritdoc />
			public Dictionary<byte, WeaponPartScript> GetAllPrefabScripts(bool includeDebug)
			{
				Logger.Info("NULL SERVICE: NullWeaponPartManager.GetAllPrefabScripts()", Logger.System.Services);
				return new Dictionary<byte, WeaponPartScript>();
			}
		}

		/// <inheritdoc />
		private class NullGamestateManager : IGamestateManager
		{
			/// <inheritdoc />
			public bool isAlive { get { return false; } }

			/// <inheritdoc />
			public string currentUserName { get; set; }

			/// <inheritdoc />
			public void RequestShutdown()
			{
				Logger.Info("NULL SERVICE: NullGamestateManager.RequestShutdown()", Logger.System.Services);
			}

			/// <inheritdoc />
			public IGamestateManager RequestSceneChange(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
			{
				Logger.Info("NULL SERVICE: NullGamestateManager.RequestSceneChange()", Logger.System.Services);
				return this;
			}
		}

		/// <inheritdoc />
		private class NullAudioManager : IAudioManager
		{
			/// <inheritdoc />
			private class NullAudioReference : IAudioReference
			{
				/// <inheritdoc />
				public IAudioReference Start()
				{
					return this;
				}

				/// <inheritdoc />
				public IAudioReference Kill(bool allowFade = true)
				{
					return this;
				}

				/// <inheritdoc />
				public IAudioReference SetVolume(float vol)
				{
					return this;
				}

				/// <inheritdoc />
				public IAudioReference AttachToRigidbody(Rigidbody rb)
				{
					return this;
				}

				/// <inheritdoc />
				public bool isPlaying { get { return false; } }

				/// <inheritdoc />
				public float isSprinting { get { return default(float); } set { } }

				/// <inheritdoc />
				public float playerSpeed { get { return default(float); } set { } }

				/// <inheritdoc />
				public float weaponType { get { return default(float); } set { } }

				/// <inheritdoc />
				public float barrelType { get { return default(float); } set { } }

				/// <inheritdoc />
				public float isCurrentPlayer { get { return default(float); } set { } }

				/// <inheritdoc />
				public float isPlayButton { get { return default(float); } set { } }

				/// <inheritdoc />
				public float healthGained { get { return default(float); } set { } }

				/// <inheritdoc />
				public float crowdHypeLevel { get { return default(float); } set { } }

				/// <inheritdoc />
				public IAudioReference SetParameter(string name, float value)
				{
					return this;
				}

				/// <inheritdoc />
				public float GetParameter(string name)
				{
					return default(float);
				}
			}

			/// <inheritdoc />
			public void InitializeDatabase()
			{
				Logger.Info("NULL SERVICE: IAudioManager.InitializeDatabase()", Logger.System.Services);
				EventManager.Notify(EventManager.Local.InitialAudioLoadComplete);
			}

			/// <inheritdoc />
			public IAudioReference CreateSound(AudioEvent e, Transform location, bool autoPlay = true)
			{
				Logger.Info("NULL SERVICE: IAudioManager.CreateSound()", Logger.System.Services);
				return new NullAudioReference();
			}

			/// <inheritdoc />
			public IAudioReference CreateSound(AudioEvent e, Transform location, Vector3 offset, Space offsetType = Space.Self, bool autoPlay = true)
			{
				Logger.Info("NULL SERVICE: IAudioManager.CreateSound()", Logger.System.Services);
				return new NullAudioReference();
			}

			/// <inheritdoc />
			// ReSharper disable once RedundantAssignment
			public IAudioReference CheckReferenceAlive(ref IAudioReference reference)
			{
				reference = null;
				return null;
			}
		}

		/// <inheritdoc />
		private class NullConsole : IGameConsole
		{
			/// <inheritdoc />
			public void AssertCheatsEnabled()
			{
				Logger.Info("NULL SERVICE: IGameConsole.AssertCheatsEnabled()", Logger.System.Services);
			}

			/// <inheritdoc />
			public IGameConsole RegisterCommand(string command, Action<string[]> handle)
			{
				Logger.Info("NULL SERVICE: IGameConsole.RegisterCommand()", Logger.System.Services);
				return this;
			}

			/// <inheritdoc />
			public IGameConsole UnregisterCommand(string command)
			{
				return this;
			}

			/// <inheritdoc />
			public IGameConsole UnregisterCommand(Action<string[]> handle)
			{
				return this;
			}

			/// <inheritdoc />
			public Logger.System enabledLogLevels
			{
				get
				{
					Logger.Info("NULL SERVICE: IGameConsole.enabledLogLevels", Logger.System.Services);
					return (Logger.System)int.MaxValue;
				}
			}
		}

		/// <inheritdoc />
		private class NullInput : IInput
		{
			/// <inheritdoc />
			public IInput RegisterInput<T>(Func<T, bool> method, T key, Action command, InputLevel level, bool allowOtherKeys = true)
			{
				Logger.Info("NULL SERVICE: IInput.RegisterInput()", Logger.System.Services);
				return this;
			}

			/// <inheritdoc />
			public IInput UnregisterInput(Action command)
			{
				Logger.Info("NULL SERVICE: IInput.UnregisterInput()", Logger.System.Services);
				return this;
			}

			/// <inheritdoc />
			public IInput RegisterAxis(
				Func<string, float> method, string axis, Action<float> command, InputLevel level, bool allowOtherAxes = true)
			{
				Logger.Info("NULL SERVICE: IInput.RegisterAxis()", Logger.System.Services);
				return this;
			}

			/// <inheritdoc />
			public IInput UnregisterAxis(Action<float> command)
			{
				Logger.Info("NULL SERVICE: IInput.UnregisterAxis()", Logger.System.Services);
				return this;
			}

			/// <inheritdoc />
			public IInput SetInputLevel(InputLevel level)
			{
				Logger.Info("NULL SERVICE: IInput.SetInputLevel()", Logger.System.Services);
				return this;
			}

			/// <inheritdoc />
			public IInput SetInputLevelState(InputLevel level, bool state)
			{
				Logger.Info("NULL SERVICE: IInput.SetInputLevelState()", Logger.System.Services);
				return this;
			}

			/// <inheritdoc />
			public IInput EnableInputLevel(InputLevel level)
			{
				Logger.Info("NULL SERVICE: IInput.EnableInputLevel()", Logger.System.Services);
				return this;
			}

			/// <inheritdoc />
			public IInput DisableInputLevel(InputLevel level)
			{
				Logger.Info("NULL SERVICE: IInput.DisableInputLevel()", Logger.System.Services);
				return this;
			}

			/// <inheritdoc />
			public bool IsInputEnabled(InputLevel level)
			{
				Logger.Info("NULL SERVICE: IInput.IsInputEnabled()", Logger.System.Services);
				return false;
			}
		}

		/// <inheritdoc />
		private class NullUIManager : IUIManager
		{
			/// <inheritdoc />
			public BoundProperty<T> GetProperty<T>(int hash)
			{
				Logger.Info("NULL SERVICE: NullGameplayUIManager.GetProperty<T>()", Logger.System.Services);
				return null;
			}

			/// <inheritdoc />
			public void BindProperty(int hash, BoundProperty prop)
			{
				Logger.Info("NULL SERVICE: NullGameplayUIManager.BindProperty()", Logger.System.Services);
			}

			/// <inheritdoc />
			public void UnbindProperty(BoundProperty prop)
			{
				Logger.Info("NULL SERVICE: NullGameplayUIManager.UnbindProperty()", Logger.System.Services);
			}

			/// <inheritdoc />
			public IScreenPanel PushNewPanel(ScreenPanelTypes type)
			{
				return null;
			}

			/// <inheritdoc />
			public IUIManager PopPanel(ScreenPanelTypes type)
			{
				return this;
			}

			/// <inheritdoc />
			public IScreenPanel TogglePanel(ScreenPanelTypes type)
			{
				return null;
			}

			/// <inheritdoc />
			public IUIManager RegisterPanel(IScreenPanel panelObject, ScreenPanelTypes type, bool disablePanel = true)
			{
				return this;
			}

			/// <inheritdoc />
			public IUIManager UnregisterPanel(IScreenPanel panelObject)
			{
				return this;
			}
		}
	}
}
