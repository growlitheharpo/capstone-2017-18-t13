﻿using System.Collections;
using System.Collections.Generic;
using FiringSquad.Debug;
using KeatsLib.State;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Core.State
{
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
			public virtual bool safeToTransition { get { return true; } }

			/// <inheritdoc />
			public virtual IState GetTransition()
			{
				return this;
			}

			/// <inheritdoc />
			public virtual void OnExit() { }
		}

		private Dictionary<string, IGameState> mBaseStates;
		private IGameState mCurrentState;

		public const string MAIN_SCENE = "main";
		public const string MENU_SCENE = "menu";
		public const string ART_PROTOTYPE_SCENE = "artproto";
		public const string DRAFT_DUALMODE = "game2player_gameplay";
		public const string DRAFT_GAMEPLAY = "draft6player_gameplay";
		public const string FOURPLAYER_GAMEPLAY = "game4player_gameplay";
		public const string GUN_GLOSSARY = "gunglossary";
		public const string HOW_TO_PLAY = "howtoplay";
		public const string KIOSK_SCENE = "kioskmode";

		/// <inheritdoc />
		public bool isAlive { get { return true; } }

		/// <inheritdoc />
		public string currentUserName { get; set; }

		/// <inheritdoc />
		public void RequestShutdown()
		{
			//TODO: Transition to shutdown state
			// (save persistence, etc.)
			Application.Quit();
		}

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			mBaseStates = new Dictionary<string, IGameState>
			{
				{ MAIN_SCENE, new TransitionToSceneState(MENU_SCENE) },
				{ MENU_SCENE, new MenuSceneState() },
				{ ART_PROTOTYPE_SCENE, new MenuSceneState() },
				{ GUN_GLOSSARY, new MenuSceneState() },
				{ DRAFT_DUALMODE, new GameSceneState() },
				{ DRAFT_GAMEPLAY, new GameSceneState() },
				{ FOURPLAYER_GAMEPLAY, new GameSceneState() },
				{ "sandbox_networked", new GameSceneState() },
				{ KIOSK_SCENE, new MenuSceneState() },
				{ HOW_TO_PLAY, new MenuSceneState() }
			};
		}

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			mCurrentState = new InitializeGameState();
			Logger.Info("Setting current state to InitializeGameState", Logger.System.State);
			mCurrentState.OnEnter();
		}
		
		/// <summary>
		/// Unity's Update function.
		/// </summary>
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

		/// <summary>
		/// Attempt to resolve a new state based on the name of the current scene.
		/// </summary>
		/// <returns>A new state if possible, or null if none are found to match the current scene.</returns>
		private IGameState ChooseStateByScene()
		{
			string currentScene = SceneManager.GetActiveScene().name;
			IGameState result;

			return mBaseStates.TryGetValue(currentScene, out result) ? result : null;
		}

		/// <inheritdoc />
		public IGamestateManager RequestSceneChange(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
		{
			StartCoroutine(AttemptSceneChange(sceneName, mode));
			return this;
		}

		/// <summary>
		/// Attempt a scene change once the current state signals that it is safe to do so.
		/// </summary>
		/// <param name="sceneName">The name of the new scene.</param>
		/// <param name="mode">Which Unity scene load mode to use.</param>
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
}
