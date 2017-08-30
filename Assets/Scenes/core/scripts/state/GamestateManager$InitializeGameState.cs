using System;

public partial class GamestateManager
{
	private class InitializeGameState : BaseGameState
	{
		private static bool kOccured;
		private bool mSaveLoadComplete, mAudioLoadComplete;

		public override void OnEnter()
		{
			if (kOccured)
				throw new ArgumentException("Cannot Initialize the game more than once! Manager is now in an invalid state.");

			mSaveLoadComplete = false;
			mAudioLoadComplete = false;
			EventManager.OnInitialPersistenceLoadComplete += InitialLoadComplete;
			EventManager.OnInitialAudioLoadComplete += AudioLoadComplete;

			ServiceLocator.Get<ISaveLoadManager>()
				.LoadData();
			ServiceLocator.Get<IAudioManager>()
				.InitializeDatabase();
		}

		public override bool safeToTransition { get { return false; } }

		public override IGameState GetTransition()
		{
			if (mSaveLoadComplete && mAudioLoadComplete)
				return instance.ChooseStateByScene();
			return null;
		}

		private void InitialLoadComplete()
		{
			mSaveLoadComplete = true;
		}

		private void AudioLoadComplete()
		{
			mAudioLoadComplete = true;
		}

		public override void OnExit()
		{
			kOccured = true;
			//Cleanup handlers so we can be garbage collected
			EventManager.OnInitialPersistenceLoadComplete -= InitialLoadComplete;
			EventManager.OnInitialAudioLoadComplete -= AudioLoadComplete;
		}
	}

}
