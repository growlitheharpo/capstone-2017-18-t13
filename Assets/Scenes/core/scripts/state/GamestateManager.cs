using System;
using System.Collections;
using System.Collections.Generic;
using KeatsLib.State;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <inheritdoc cref="IGamestateManager"/>
public partial class GamestateManager : MonoSingleton<GamestateManager>, IGamestateManager
{
	/// <inheritdoc />
	private interface IGameState : IState
	{
		/// <summary>
		/// Whether this state believes a transition is safe at this time.
		/// </summary>
		bool safeToTransition { get; }
	}

	/// <inheritdoc />
	/// <summary>
	/// Interior class used for game states.
	/// </summary>
	private abstract class BaseGameState : IGameState
	{
		/// <inheritdoc />
		public virtual void OnEnter() { }

		/// <inheritdoc />
		public virtual void Update() { }

		/// <inheritdoc />
		public virtual bool safeToTransition { get { return true;  }}
		
		public virtual IState GetTransition()
		{
			return this;
		}

		/// <inheritdoc />
		public virtual void OnExit() { }
	}

	private class NullState : BaseGameState
	{
		public override IState GetTransition()
		{
			return instance.ChooseStateByScene();
		}
	}

	public const string MAIN_SCENE = "main";
	public const string MENU_SCENE = "menu";
	public const string BASE_WORLD = "base_world";
	public const string PROTOTYPE3_SCENE = "prototype3";
	public const string ART_PROTOTYPE_SCENE = "artproto";

	public enum Feature
	{
		WeaponDrops,
		WeaponDurability,
	}

	private Dictionary<string, IGameState> mBaseStates;
	private IGameState mCurrentState;

	/// <inheritdoc />
	public bool isAlive { get { return true; } }

	/// <inheritdoc />
	public void RequestShutdown()
	{
		//TODO: Transition to shutdown state
		// (save persistence, etc.)
		Application.Quit();
	}

	protected override void Awake()
	{
		base.Awake();
		mBaseStates = new Dictionary<string, IGameState>
		{
			{ MAIN_SCENE, new TransitionToSceneState(MENU_SCENE) },
			{ MENU_SCENE, new MenuSceneState() },
			{ PROTOTYPE3_SCENE,			new GameSceneState() },
			{ "sandbox_networked",		new GameSceneState() },
			{ ART_PROTOTYPE_SCENE,	new MenuSceneState() },
			{ BASE_WORLD, new NullState() },
		};
	}

	private void Start()
	{
		mCurrentState = new InitializeGameState();
		Logger.Info("Setting current state to InitializeGameState", Logger.System.State);
		mCurrentState.OnEnter();

		ServiceLocator.Get<IGameConsole>()
			.RegisterCommand("close", s => RequestSceneChange(MENU_SCENE));
	}

	private void Update()
	{
		if (mCurrentState == null)
		{
			mCurrentState = ChooseStateByScene();
			Logger.Info("Setting current state to " + mCurrentState + " because of the scene name.", Logger.System.State);
			
			if (mCurrentState != null)
				mCurrentState.OnEnter();
			else
				return;
		}

		mCurrentState.Update();
		IState newState = mCurrentState.GetTransition();

		if (newState == mCurrentState || newState == null)
			return;

		mCurrentState.OnExit();
		Logger.Info("Setting current state to " + newState + " because of a regular transition.", Logger.System.State);
		mCurrentState = (IGameState)newState;
		mCurrentState.OnEnter();
	}

	private IGameState ChooseStateByScene()
	{
		string currentScene = SceneManager.GetActiveScene().name;
		IGameState result;

		return mBaseStates.TryGetValue(currentScene, out result) ? result : null;
	}

	public IGamestateManager RequestSceneChange(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
	{
		StartCoroutine(AttemptSceneChange(sceneName, mode));
		return this;
	}

	private IEnumerator AttemptSceneChange(string sceneName, LoadSceneMode mode)
	{
		while (!mCurrentState.safeToTransition)
			yield return null;

		mCurrentState.OnExit();
		mCurrentState = new TransitionToSceneState(sceneName, mode);
		Logger.Info("Setting current state to TransitionToSceneState because of a request.", Logger.System.State);
		mCurrentState.OnEnter();
	}
}
