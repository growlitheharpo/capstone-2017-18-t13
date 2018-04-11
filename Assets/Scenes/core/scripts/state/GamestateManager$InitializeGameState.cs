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
				EventManager.Local.OnInitialAudioLoadComplete += OnInitialAudioLoadComplete;

				ServiceLocator.Get<IAudioManager>()
					.InitializeDatabase();
			}

			/// <inheritdoc />
			public override void OnExit()
			{
				kOccured = true;
				EventManager.Local.OnInitialAudioLoadComplete -= OnInitialAudioLoadComplete;
			}

			/// <inheritdoc />
			public override bool safeToTransition { get { return false; } }

			/// <inheritdoc />
			public override IState GetTransition()
			{
				return mAudioLoadComplete ? instance.ChooseStateByScene() : null;
			}

			/// <summary>
			/// EVENT HANDLER: Local.OnInitialAudioLoadComplete
			/// </summary>
			private void OnInitialAudioLoadComplete()
			{
				mAudioLoadComplete = true;
			}
		}
	}
}
