using System;
using System.Collections.Generic;
using KeatsLib.Persistence;
using UnityEngine;
using Input = KeatsLib.Unity.Input;

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

		return null;
	}

	private class NullGamestateManager : IGamestateManager
	{
		public bool isAlive { get { return false; } }
		public void RequestShutdown()
		{
			Logger.Info("NULL SERVICE: NullGamestateManager.RequestShutdown()", Logger.System.Services);
		}

		public bool IsFeatureEnabled(GamestateManager.Feature feat)
		{
			return false;
		}
	}

	private class NullAudioManager : IAudioManager
	{
		private class NullAudioReference : IAudioReference
		{
			public void Kill() { }
			public void FadeOut(float time) { }
			public void SetRepeat(bool repeat) { }
			public void SetVolume(float vol) { }
			public void SetPitch(float pitch) { }
		}

		public void InitializeDatabase()
		{
			Logger.Info("NULL SERVICE: IAudioManager.InitializeDatabase()", Logger.System.Services);
		}

		public IAudioReference PlaySound(AudioManager.AudioEvent e, IAudioProfile profile, Transform location)
		{
			Logger.Info("NULL SERVICE: IAudioManager.PlaySound()", Logger.System.Services);
			return new NullAudioReference();
		}

		public IAudioReference PlaySound(AudioManager.AudioEvent e, IAudioProfile profile, Transform location, Vector3 offset)
		{
			Logger.Info("NULL SERVICE: IAudioManager.PlaySound()", Logger.System.Services);
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
		public IInput RegisterInput<T>(Func<T, bool> method, T key, Action command, Input.InputLevel level, bool allowOtherKeys = true)
		{
			Logger.Info("NULL SERVICE: IInput.RegisterInput()", Logger.System.Services);
			return this;
		}

		public IInput UnregisterInput(Action command)
		{
			Logger.Info("NULL SERVICE: IInput.UnregisterInput()", Logger.System.Services);
			return this;
		}

		public IInput RegisterAxis(Func<string, float> method, string axis, Action<float> command, Input.InputLevel level, bool allowOtherAxes = true)
		{
			Logger.Info("NULL SERVICE: IInput.RegisterAxis()", Logger.System.Services);
			return this;
		}

		public IInput UnregisterAxis(Action<float> command)
		{
			Logger.Info("NULL SERVICE: IInput.UnregisterAxis()", Logger.System.Services);
			return this;
		}

		public void SetInputLevel(Input.InputLevel level)
		{
			Logger.Info("NULL SERVICE: IInput.SetInputLevel()", Logger.System.Services);
		}

		public void SetInputLevelState(Input.InputLevel level, bool state)
		{
			Logger.Info("NULL SERVICE: IInput.SetInputLevelState()", Logger.System.Services);
		}

		public void EnableInputLevel(Input.InputLevel level)
		{
			Logger.Info("NULL SERVICE: IInput.EnableInputLevel()", Logger.System.Services);
		}

		public void DisableInputLevel(Input.InputLevel level)
		{
			Logger.Info("NULL SERVICE: IInput.DisableInputLevel()", Logger.System.Services);
		}

		public bool IsInputEnabled(Input.InputLevel level)
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
			get {
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
	}
}
