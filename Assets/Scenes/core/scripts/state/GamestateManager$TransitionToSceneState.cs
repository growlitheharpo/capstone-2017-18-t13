using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class GamestateManager
{
	private class TransitionToSceneState : BaseGameState
	{
		private readonly string mSceneName;
		private AsyncOperation mLoadingOperation;

		public override bool safeToTransition { get { return false; } }

		public TransitionToSceneState(string sceneName)
		{
			mSceneName = sceneName;
		}

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

		public override IGameState GetTransition()
		{
			return mLoadingOperation != null && mLoadingOperation.isDone ? instance.ChooseStateByScene() : null;
		}
	}
}
