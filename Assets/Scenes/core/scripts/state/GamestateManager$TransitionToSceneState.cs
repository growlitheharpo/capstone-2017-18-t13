using System.Collections;
using KeatsLib.State;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FiringSquad.Core.State
{
	public partial class GamestateManager
	{
		/// <summary>
		/// State used during the transition to another scene.
		/// Scene loading is done async to avoid freezing the game.
		/// </summary>
		private class TransitionToSceneState : BaseGameState
		{
			private readonly string mSceneName;
			private AsyncOperation mLoadingOperation;
			private readonly LoadSceneMode mMode;

			/// <inheritdoc />
			public override bool safeToTransition { get { return false; } }

			/// <summary>
			/// State used during the transition to another scene.
			/// Scene loading is done async to avoid freezing the game.
			/// </summary>
			public TransitionToSceneState(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
			{
				mSceneName = sceneName;
				mMode = mode;
			}

			/// <inheritdoc />
			public override void OnEnter()
			{
				instance.StartCoroutine(LoadScene());
			}

			/// <summary>
			/// Start an operation to load a new scene.
			/// TODO: We could add a "loading" display of some sort here.
			/// </summary>
			private IEnumerator LoadScene()
			{
				mLoadingOperation = SceneManager.LoadSceneAsync(mSceneName, mMode);

				while (!mLoadingOperation.isDone)
					yield return null;
			}

			/// <inheritdoc />
			public override void OnExit()
			{
				Scene scene = SceneManager.GetSceneByName(mSceneName);
				SceneManager.SetActiveScene(scene);
			}

			/// <inheritdoc />
			public override IState GetTransition()
			{
				return mLoadingOperation != null && mLoadingOperation.isDone ? instance.ChooseStateByScene() : null;
			}
		}
	}
}
