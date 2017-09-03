using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

		public override bool safeToTransition { get { return false; } }

		public TransitionToSceneState(string sceneName)
		{
			mSceneName = sceneName;
		}

		/// <inheritdoc />
		public override void OnEnter()
		{
			instance.StartCoroutine(LoadScene());
		}

		private IEnumerator LoadScene()
		{
			mLoadingOperation = SceneManager.LoadSceneAsync(mSceneName, LoadSceneMode.Single);

			while (!mLoadingOperation.isDone)
				yield return null;
		}

		/// <inheritdoc />
		public override IGameState GetTransition()
		{
			return mLoadingOperation != null && mLoadingOperation.isDone ? instance.ChooseStateByScene() : null;
		}
	}
}
