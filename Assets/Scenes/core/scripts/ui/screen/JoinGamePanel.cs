using System;
using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Core.UI;
using FiringSquad.Debug;
using FiringSquad.Networking;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Prettier UI panel element for joining or starting a networked game.
	/// </summary>
	public class JoinGamePanel : MonoBehaviour, IScreenPanel
	{
		/// Inspector variables
		[SerializeField] private LayoutGroup mMatchDataHolder;
		[SerializeField] private MatchDataInfoPanel mMatchDataPrefab;
		[SerializeField] private Button mCreateMatchButton;
		[SerializeField] private Button mJoinMatchButton;
		[SerializeField] private Button mRefreshMatchesButton;
		[SerializeField] private Button mReturnToMenuButton;
		[SerializeField] private Text mStatusText;
		[SerializeField] private JoinGameCreateMatchPanel mCreateMatchPanel;
		[SerializeField] private GameObject mJoinMatchPanel;

		/// Private variables
		private NetworkGameManager mNetworkManager;

		/// <inheritdoc />
		public bool disablesInput { get { return true; } }

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Start()
		{
			mNetworkManager = FindObjectOfType<NetworkGameManager>();

			if (mNetworkManager == null)
			{
				Destroy(this);
				throw new InvalidOperationException("Cannot use a join game panel without the network managers in the scene!");
			}

			mRefreshMatchesButton.onClick.AddListener(RefreshMatchList);
			mCreateMatchButton.onClick.AddListener(ClickCreateMatch);
			mJoinMatchButton.onClick.AddListener(ClickJoinMatch);
			mReturnToMenuButton.onClick.AddListener(ClickReturnToMenu);

			EventManager.Local.OnLocalPlayerSpawned += OnLocalPlayerSpawned;

			ServiceLocator.Get<IUIManager>()
				.RegisterPanel(this, ScreenPanelTypes.HandleConnection);

			ServiceLocator.Get<IGameConsole>()
				.RegisterCommand("connect", CONSOLE_ConnectToIpAddress);
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			mRefreshMatchesButton.onClick.RemoveListener(RefreshMatchList);
			mCreateMatchButton.onClick.RemoveListener(ClickCreateMatch);
			EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;

			ServiceLocator.Get<IGameConsole>()
				.UnregisterCommand(CONSOLE_ConnectToIpAddress);
			ServiceLocator.Get<IUIManager>()
				.UnregisterPanel(this);
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerSpawned
		/// </summary>
		private void OnLocalPlayerSpawned(CltPlayer obj)
		{
			ServiceLocator.Get<IUIManager>()
				.PopPanel(ScreenPanelTypes.HandleConnection)
				.UnregisterPanel(this);

			Destroy(gameObject);
		}

		/// <inheritdoc />
		public void OnEnablePanel()
		{
			mNetworkManager.StartMatchMaker();
			RefreshMatchList();
		}

		/// <inheritdoc />
		public void OnDisablePanel() { }

		/// <summary>
		/// Destroys all the information panels currently displayed.
		/// </summary>
		private void DestroyAllMatchPanels()
		{
			foreach (Transform child in mMatchDataHolder.transform)
				Destroy(child.gameObject);
		}

		/// <summary>
		/// Enable the CreateMatch panel when the player clicks the appropriate button.
		/// </summary>
		private void ClickCreateMatch()
		{
			DestroyAllMatchPanels();
			mCreateMatchPanel.gameObject.SetActive(true);
		}

		/// <summary>
		/// Enable the JoinMatch panel when the player clicks the appropriate button.
		/// </summary>
		private void ClickJoinMatch()
		{
			DestroyAllMatchPanels();
			mJoinMatchPanel.gameObject.SetActive(true);
		}

		/// <summary>
		/// Send the player back to the main menu
		/// </summary>
		private void ClickReturnToMenu()
		{
			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.MENU_SCENE);

		}

		/// <summary>
		/// Ping the server and get all the latest match information and display it.
		/// </summary>
		private void RefreshMatchList()
		{
			DestroyAllMatchPanels();

			// TODO: Use the number of players for this scene as the "domain"!
			if (mNetworkManager.matchMaker == null)
				mNetworkManager.StartMatchMaker();

			mNetworkManager.matchMaker.ListMatches(0, 8, "", false, 0, mCreateMatchPanel.levelDomain, OnRefreshMatchList);
		}

		/// <summary>
		/// Handle receiving the match list data.
		/// </summary>
		private void OnRefreshMatchList(bool success, string extendedResult, List<MatchInfoSnapshot> matchList)
		{
			if (!success)
			{
				DisplayError("Unable to find matches\nERR: " + extendedResult);
				return;
			}

			mNetworkManager.OnMatchList(true, extendedResult, matchList);

			foreach (MatchInfoSnapshot match in matchList)
			{
				MatchDataInfoPanel matchData = Instantiate(mMatchDataPrefab.gameObject, mMatchDataHolder.transform, false).GetComponent<MatchDataInfoPanel>();
				string matchName = match.name.Split(':')[0],
						matchIp = match.name.Split(':')[1];

				matchData.matchName = matchName;
				matchData.matchCurrentPlayers = match.currentSize;
				matchData.matchMaxPlayers = match.maxSize;
				matchData.RefreshPlayerString();

				MatchInfoSnapshot matchCopy = match;
				matchData.joinMatchButton.onClick.AddListener(() =>
				{
					mNetworkManager.matchName = matchName;
					mNetworkManager.matchSize = (uint)matchCopy.currentSize;

					ConnectToGame(matchIp);
				});
			}
		}

		/// <summary>
		/// Console command to immediately connect to the provided IP address.
		/// </summary>
		private void CONSOLE_ConnectToIpAddress(string[] args)
		{
			if (args.Length != 1)
				throw new ArgumentException("Invalid arguments for command: connect");

			ConnectToGame(args[0]);
		}

		/// <summary>
		/// Connect immediately to the provided IP address.
		/// </summary>
		private void ConnectToGame(string matchIp)
		{
			mNetworkManager.StopMatchMaker();
			mNetworkManager.networkAddress = matchIp;
			mNetworkManager.StartClient();
		}

		/// <summary>
		/// Called when an async JoinMatch call has completed.
		/// </summary>
		/// <param name="success">Whether or not the call was complete.</param>
		/// <param name="extendedinfo">Extended string info on the error if success is false.</param>
		/// <param name="responsedata">Data about the match that has been joined.</param>
		private void OnJoinMatch(bool success, string extendedinfo, MatchInfo responsedata)
		{
			if (!success)
			{
				DisplayError("Unable to join match\nERR: " + extendedinfo);
				return;
			}

			mNetworkManager.OnMatchJoined(true, extendedinfo, responsedata);
			FinishConnection();
		}

		/// <summary>
		/// Display a message to the user as an error.
		/// </summary>
		/// <param name="message">The exact message to display.</param>
		public void DisplayError(string message)
		{
			DestroyAllMatchPanels();
			mStatusText.text = message;
		}

		/// <summary>
		/// Called when the process has completed.
		/// </summary>
		public void FinishConnection()
		{
			ServiceLocator.Get<IUIManager>()
				.PopPanel(ScreenPanelTypes.HandleConnection)
				.UnregisterPanel(this);

			Destroy(gameObject);
		}
	}
}
