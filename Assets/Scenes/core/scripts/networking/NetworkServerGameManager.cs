using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Data;
using FiringSquad.Debug;
using FiringSquad.Gameplay;
using FiringSquad.Gameplay.UI;
using KeatsLib.State;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Networking
{
	/// <summary>
	/// The main game manager on the server. Handles the current game state
	/// and routes different signals that inform clients of the game state.
	/// </summary>
	public partial class NetworkServerGameManager : NetworkBehaviour
	{
		/// Inspector variables
		[SerializeField] private ServerGameDefaultData mData;

		/// Private variables
		private ServerStateMachine mStateMachine;

		

		/// <summary>
		/// Unity function. Called when this class begins on the server.
		/// </summary>
		public override void OnStartServer()
		{
			ServiceLocator.Get<IGameConsole>().RegisterCommand("force-start", CONSOLE_ForceStartGame);
			mStateMachine = new ServerStateMachine(this);

			JoinGameCreateMatchPanel panel = GameObject.Find("CreateMatchPanel").GetComponent<JoinGameCreateMatchPanel>();
			if (panel.playerCount() > 0)
				mData.goalPlayerCount = panel.playerCount();

			if (panel.matchType() != GameData.MatchType.Invalid)
				mData.currentType = panel.matchType();
		}

		/// <summary>
		/// Cleanup listeners and handlers.
		/// </summary>
		private void OnDestroy()
		{
			ServiceLocator.Get<IGameConsole>().UnregisterCommand(CONSOLE_ForceStartGame);
		}

		/// <summary>
		/// Server only. Unity's update function.
		/// </summary>
		private void Update()
		{
			mStateMachine.Update();
		}

		/// <summary>
		/// CONSOLE HANDLER: Force start the game from the server console.
		/// </summary>
		private void CONSOLE_ForceStartGame(string[] obj)
		{
			if (obj.Length == 1 && obj[0] == "1")
				mStateMachine.ForceStartGameNow(true, true);
			else if (obj.Length == 1 && obj[0] == "2")
				mStateMachine.ForceStartGameNow(false, true);
			else if (obj.Length == 1 && obj[0] == "3")
				mStateMachine.ForceStartGameNow(false, false);
			else
			{
				throw new ArgumentException("force-start called with invalid parameters.\n"
											+ "Use \"force-start 1\" to start with lobby, \"force-start 2\" to start with intro, "
											+ "or \"force-start 3\" to start immediately.");
			}
		}

		/// <summary>
		/// Static method that finds the spawn point that is furthest away from all other players.
		/// TODO: Also account for the player's death position.
		/// </summary>
		private static Transform ChooseSafestSpawnPosition(CltPlayer[] players, CltPlayer deadPlayer, IList<Transform> targets)
		{
			var scores = new float[targets.Count];
			for (int i = 0; i < scores.Length; i++)
			{
				foreach (CltPlayer p in players)
				{
					if (p == deadPlayer || p == null)
						continue;
					scores[i] += Vector3.Distance(p.transform.position, targets[i].position);
				}
			}

			return targets[Array.IndexOf(scores, scores.Max())];
		}

		/// <summary>
		/// The state machine for the server's game manager
		/// </summary>
		private partial class ServerStateMachine : BaseStateMachine
		{
			// The states:
			private partial class WaitingForConnectionState : BaseState<ServerStateMachine> { }
			private partial class StartLobbyState : BaseState<ServerStateMachine> { }
			private partial class WaitForIntroState : BaseState<ServerStateMachine> { }
			private partial class SpawnPlayersAndStartState : BaseState<ServerStateMachine> { }
			private partial class GameRunningState : BaseState<ServerStateMachine> { }
			private partial class GameFinishedState	 : BaseState<ServerStateMachine> { }

			// Private shared data
			private readonly NetworkServerGameManager mScript;
			private readonly Transform[] mLobbyStartPositions;
			private StageCaptureArea[] mCaptureAreas;
			private CltPlayer[] mPlayerList;
			private bool mForceSkipIntro;

			// Shortcut to the script's data
			private ServerGameDefaultData data { get { return mScript.mData; } }

			/// <summary>
			/// The state machine for the server's game manager
			/// </summary>
			public ServerStateMachine(NetworkServerGameManager script)
			{
				mScript = script;
				mLobbyStartPositions = FindObjectsOfType<NetworkStartPosition>().Select(x => x.transform).ToArray();
				TransitionStates(new WaitingForConnectionState(this));
			}

			/// <summary>
			/// Tick our state machine.
			/// </summary>
			public new void Update()
			{
				base.Update();
			}

			/// <summary>
			/// Force an immediate transition into a non-waiting state.
			/// </summary>
			/// <param name="lobby">True to enter the lobby, false to immediately enter the match.</param>
			/// <param name="intro">True to play the intro, false otherwise.</param>
			public void ForceStartGameNow(bool lobby, bool intro)
			{
				mForceSkipIntro = !intro;

				if (lobby)
					TransitionStates(new StartLobbyState(this));
				else if (intro)
					TransitionStates(new WaitForIntroState(this));
				else
					TransitionStates(new SpawnPlayersAndStartState(this));
			}
		}
	}
}
