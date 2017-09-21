﻿using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <inheritdoc cref="IGamestateManager"/>
public partial class GamestateManager : MonoSingleton<GamestateManager>, IGamestateManager
{
	/// <summary>
	/// Interface used for a state.
	/// </summary>
	private interface IGameState
	{
		/// <summary>
		/// Called when state is first entered.
		/// </summary>
		void OnEnter();

		/// <summary>
		/// Called every frame that state is active.
		/// </summary>
		void Update();

		/// <summary>
		/// Whether this state believes a transition is safe at this time.
		/// </summary>
		bool safeToTransition { get; }

		/// <summary>
		/// The state this state wants to transition to. Returns null if we should stay as we are.
		/// </summary>
		IGameState GetTransition();

		/// <summary>
		/// Called when state is exited.
		/// </summary>
		void OnExit();
	}

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

		/// <inheritdoc />
		public virtual IGameState GetTransition()
		{
			return null;
		}

		/// <inheritdoc />
		public virtual void OnExit() { }
	}

	public const string MAIN_SCENE = "main";
	public const string MENU_SCENE = "menu";
	public const string GAME_SCENE = "scene1";
	public const string PROTOTYPE1_SCENE = "prototype1";
	public const string PROTOTYPE1_SETUP_SCENE = "prototype1_intro";
	public const string PROTOTYPE2_SCENE = "prototype2";
	public const string PROTOTYPE3_SCENE = "prototype3";
	public const string ART_PROTOTYPE_SCENE = "artproto";
	public const string DESIGN_TEST_SCENE = "p1&p2_testlevel";

	public enum Feature
	{
		WeaponDrops,
		WeaponDurability,
	}

	private bool mOverrideEnableDrops, mOverrideEnableDurability;

	private Dictionary<string, IGameState> mBaseStates;
	private IGameState mCurrentState;

	/// <inheritdoc />
	public bool isAlive { get { return true; } }

	/// <inheritdoc />
	public void RequestShutdown()
	{
		//TODO: Transition to shutdown state
		// (save persistence, etc.)
	}

	protected override void Awake()
	{
		base.Awake();
		mBaseStates = new Dictionary<string, IGameState>
		{
			{ MAIN_SCENE, new TransitionToSceneState(MENU_SCENE) },
			{ MENU_SCENE, new MenuSceneState() },
			{ GAME_SCENE, new GameSceneState() },
			{ PROTOTYPE1_SCENE, new Prototype1State() },
			{ PROTOTYPE1_SETUP_SCENE, new MenuSceneState() },
			{ PROTOTYPE2_SCENE, new Prototype2State() },
			{ PROTOTYPE3_SCENE, new GameSceneState() },
			{ ART_PROTOTYPE_SCENE, new GameSceneState() },
			{ DESIGN_TEST_SCENE, new Prototype2State() },
		};

		EventManager.OnRequestSceneChange += ReceiveSceneChangeRequest;
	}

	private void OnDestroy()
	{
		EventManager.OnRequestSceneChange -= ReceiveSceneChangeRequest;
	}

	private void Start()
	{
		mCurrentState = new InitializeGameState();
		Logger.Info("Setting current state to InitializeGameState", Logger.System.State);
		mCurrentState.OnEnter();

		ServiceLocator.Get<IGameConsole>()
			.RegisterCommand("close", s => EventManager.Notify(() => EventManager.RequestSceneChange(MENU_SCENE)))
			.RegisterCommand("feature", HandleFeatureForceCommand);
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
		IGameState newState = mCurrentState.GetTransition();

		if (newState == null)
			return;

		mCurrentState.OnExit();
		Logger.Info("Setting current state to " + newState + " because of a regular transition.", Logger.System.State);
		mCurrentState = newState;
		mCurrentState.OnEnter();
	}

	private IGameState ChooseStateByScene()
	{
		string currentScene = SceneManager.GetActiveScene().name;
		IGameState result;

		return mBaseStates.TryGetValue(currentScene, out result) ? result : null;
	}

	private void ReceiveSceneChangeRequest(string sceneName)
	{
		if (!mCurrentState.safeToTransition)
			return;

		mCurrentState.OnExit();
		mCurrentState = new TransitionToSceneState(sceneName);
		Logger.Info("Setting current state to TransitionToSceneState because of an event.", Logger.System.State);
		mCurrentState.OnEnter();
	}

	private void HandleFeatureForceCommand(string[] obj)
	{
		if (obj.Length != 2)
			throw new ArgumentException("Invalid arguments for command \"feature\".");

		string feat = obj[0].ToLower();
		int val = int.Parse(obj[1]);

		switch (feat) {
			case "drops":
				mOverrideEnableDrops = val == 1;
				break;
			case "durability":
				mOverrideEnableDurability = val == 1;
				break;
			default:
				throw new ArgumentException(obj[0] + " is not a valid feature.");
		}
	}

	public bool IsFeatureEnabled(Feature feat)
	{
		switch (feat)
		{
			case Feature.WeaponDrops:
				return SceneManager.GetActiveScene().name == PROTOTYPE2_SCENE || mOverrideEnableDrops;
			case Feature.WeaponDurability:
				return SceneManager.GetActiveScene().name == PROTOTYPE1_SCENE || mOverrideEnableDurability;
			default:
				throw new ArgumentOutOfRangeException("feat", feat, null);
		}
	}
}
