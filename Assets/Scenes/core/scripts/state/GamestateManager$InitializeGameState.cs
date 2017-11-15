using System;
using FiringSquad.Core.Audio;
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
			private bool mAudioLoadComplete;

			/// <inheritdoc />
			public override void OnEnter()
			{
				if (kOccured)
					throw new ArgumentException("Cannot Initialize the game more than once! Manager is now in an invalid state.");

				mAudioLoadComplete = false;
				EventManager.Local.OnInitialAudioLoadComplete += AudioLoadComplete;

				ServiceLocator.Get<IAudioManager>()
					.InitializeDatabase();
			}

			/// <inheritdoc />
			public override bool safeToTransition { get { return false; } }

			/// <inheritdoc />
			public override IState GetTransition()
			{
				if (mAudioLoadComplete)
					return instance.ChooseStateByScene();
				return null;
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
				EventManager.Local.OnInitialAudioLoadComplete -= AudioLoadComplete;
			}
		}
	}
}
