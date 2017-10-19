using System;
using FiringSquad.Core.Audio;
using FiringSquad.Core.SaveLoad;
using KeatsLib.State;

namespace FiringSquad.Core.State
{
	public partial class GamestateManager
	{
		/// <summary>
		/// State used to initialize the game at start-up.
		/// </summary>
		/// <inheritdoc />
		private class InitializeGameState : BaseGameState
		{
			private static bool kOccured;
			private bool mSaveLoadComplete, mAudioLoadComplete;

			/// <inheritdoc />
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

			/// <inheritdoc />
			public override bool safeToTransition { get { return false; } }

			/// <inheritdoc />
			public override IState GetTransition()
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

			/// <inheritdoc />
			public override void OnExit()
			{
				kOccured = true;
				//Cleanup handlers so we can be garbage collected
				EventManager.OnInitialPersistenceLoadComplete -= InitialLoadComplete;
				EventManager.OnInitialAudioLoadComplete -= AudioLoadComplete;
			}
		}
	}
}
