using System;
using System.Collections;
using System.Collections.Generic;
using KeatsLib.State;
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
	public const string GAME_SCENE = "scene1";
	public const string BASE_WORLD = "base_world";
	public const string PROTOTYPE1_SCENE = "prototype1";
	public const string PROTOTYPE1_SETUP_SCENE = "prototype1_intro";
	public const string PROTOTYPE2_SCENE = "prototype2";
	public const string PROTOTYPE3_SCENE = "prototype3";
	public const string ART_PROTOTYPE_SCENE = "artproto";
	public const string DESIGN_TEST_SCENE = "p1&p2_testLevel";

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
			{ PROTOTYPE1_SETUP_SCENE, new MenuSceneState() },
			{ PROTOTYPE1_SCENE,			new GameSceneState() },
			{ PROTOTYPE2_SCENE,			new GameSceneState() },
			{ PROTOTYPE3_SCENE,			new GameSceneState() },
			{ DESIGN_TEST_SCENE,		new GameSceneState() },
			{ ART_PROTOTYPE_SCENE,	new MenuSceneState() },
			{ BASE_WORLD, new NullState() },
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

	private void ReceiveSceneChangeRequest(string sceneName, LoadSceneMode mode)
	{
		Logger.Info("Received a scene request!!!!!!!!! " + sceneName, Logger.System.State);
		StartCoroutine(AttemptSceneChange(sceneName, mode));
		/*
		if (!mCurrentState.safeToTransition)
			return;

		mCurrentState.OnExit();
		mCurrentState = new TransitionToSceneState(sceneName, mode);
		Logger.Info("Setting current state to TransitionToSceneState because of an event.", Logger.System.State);
		mCurrentState.OnEnter();*/
	}

	private IEnumerator AttemptSceneChange(string sceneName, LoadSceneMode mode)
	{
		while (!mCurrentState.safeToTransition)
			yield return null;

		mCurrentState.OnExit();
		mCurrentState = new TransitionToSceneState(sceneName, mode);
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
		bool isStateGamestate = mCurrentState.GetType() == typeof(GameSceneState);

		switch (feat)
		{
			case Feature.WeaponDrops:
				return mOverrideEnableDrops || (isStateGamestate && ((GameSceneState)mCurrentState).IsFeatureEnabled(feat));
			case Feature.WeaponDurability:
				return mOverrideEnableDurability || (isStateGamestate && ((GameSceneState)mCurrentState).IsFeatureEnabled(feat));
			default:
				throw new ArgumentOutOfRangeException("feat", feat, null);
		}
	}
}
