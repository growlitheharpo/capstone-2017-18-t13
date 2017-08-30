using System.Collections.Generic;
using UnityEngine.SceneManagement;

public partial class GamestateManager : MonoSingleton<GamestateManager>, IGamestateManager
{
	private interface IGameState
	{
		void OnEnter();
		void Update();

		bool safeToTransition { get;  }
		IGameState GetTransition();

		void OnExit();
	}

	private abstract class BaseGameState : IGameState
	{
		public virtual void OnEnter() { }

		public void Update() { }

		public virtual bool safeToTransition { get { return true;  }}

		public virtual IGameState GetTransition()
		{
			return null;
		}

		public virtual void OnExit() { }
	}

	public const string MAIN_SCENE = "main";
	public const string MENU_SCENE = "menu";
	public const string GAME_SCENE = "scene1";

	private Dictionary<string, IGameState> mBaseStates;
	private IGameState mCurrentState;

	public bool isAlive { get { return true; } }

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
			{ GAME_SCENE, new GameSceneState() }
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
	}

	private void Update()
	{
		if (mCurrentState == null)
		{
			mCurrentState = ChooseStateByScene();
			Logger.Info("Setting current state to " + mCurrentState + " because of the scene name.", Logger.System.State);
			mCurrentState.OnEnter();
		}
		if (mCurrentState == null)
			return;

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
}
