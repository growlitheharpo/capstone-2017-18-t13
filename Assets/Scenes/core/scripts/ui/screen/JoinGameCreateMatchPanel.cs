using System.Collections;
using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Debug;
using FiringSquad.Networking;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Gameplay.UI
{
	public class JoinGameCreateMatchPanel : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private InputField mNameEntryField;
		[SerializeField] private Button mCancelButton01;
		[SerializeField] private Button mCancelButton02;
		[SerializeField] private Button mConfirmButton;
		[SerializeField] private Dropdown mPlayerCountDropdown;
		[SerializeField] private Dropdown mGameModeDropdown;

		/// Private variables
		private NetworkGameManager mNetworkManager;
		private JoinGamePanel mJoinGamePanel;

		// Info for starting the game
		int mPlayerCount;
		GameData.MatchType mMatchType;

		/// <summary>
		/// Unity's Awake signal.
		/// </summary>
		private void Awake()
		{
			mPlayerCount = 4;
			mMatchType = GameData.MatchType.Deathmatch;
			mNetworkManager = FindObjectOfType<NetworkGameManager>();

			mCancelButton01.onClick.AddListener(OnClickCancelButton);
			mCancelButton02.onClick.AddListener(OnClickCancelButton);
			mConfirmButton.onClick.AddListener(OnClickConfirmButton);

			mPlayerCountDropdown.onValueChanged.AddListener(delegate { OnChangePlayerCount(); });
			mGameModeDropdown.onValueChanged.AddListener(delegate { OnChangeGameType(); });

			mJoinGamePanel = GetComponentInParent<JoinGamePanel>();
		}

		/// <summary>
		/// Unity's OnDestroy signal.
		/// </summary>
		private void OnDestroy()
		{
			mCancelButton01.onClick.RemoveListener(OnClickCancelButton);
			mCancelButton02.onClick.RemoveListener(OnClickCancelButton);
			mConfirmButton.onClick.RemoveListener(OnClickConfirmButton);
			mPlayerCountDropdown.onValueChanged.RemoveListener(delegate { OnChangePlayerCount(); });
			mGameModeDropdown.onValueChanged.RemoveListener(delegate { OnChangeGameType(); });
		}

		/// <summary>
		/// Unity's OnEnable signal.
		/// </summary>
		private void OnEnable()
		{
			string defaultName = ServiceLocator.Get<IGamestateManager>().currentUserName + "\'s Game";
			mNameEntryField.text = defaultName;
		}

		/// <summary>
		/// Click handler for the cancel button.
		/// </summary>
		private void OnClickCancelButton()
		{
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Click handler for the confirm create match button.
		/// </summary>
		private void OnClickConfirmButton()
		{
			string matchName = mNameEntryField.text;
			matchName += ":" + Network.player.ipAddress;

			if (mNetworkManager.matchMaker == null)
				mNetworkManager.StartMatchMaker();

			mNetworkManager.matchMaker.CreateMatch(matchName, 5, true, "", Network.player.ipAddress, Network.player.ipAddress, 0, 0, OnMatchCreate);
		}

		/// <summary>
		/// Value change handler for the player count dropdown
		/// </summary>
		private void OnChangePlayerCount()
		{
			// Check the value of the player count
			int dropVal = mPlayerCountDropdown.value;

			if (dropVal == 0)
			{
				mPlayerCount = 4;
			}
			else if (dropVal == 1)
			{
				mPlayerCount = 6;
			}
		}

		/// <summary>
		/// Value change handler for the player's match type
		/// </summary>
		private void OnChangeGameType()
		{
			// Check the value of the player count
			int dropVal = mGameModeDropdown.value;

			if (dropVal == 0)
			{
				mMatchType = GameData.MatchType.Deathmatch;
			}
			else if (dropVal == 1)
			{
				mMatchType = GameData.MatchType.TeamDeathmatch;
			}
		}

		/// <summary>
		/// Async handler for creating a new match.
		/// </summary>
		private void OnMatchCreate(bool success, string extendedinfo, MatchInfo responsedata)
		{
			if (!success)
			{
				gameObject.SetActive(false);
				mJoinGamePanel.DisplayError("Unable to create match\nERR: " + extendedinfo);
				return;
			}

			//mNetworkManager.OnMatchCreate(true, extendedinfo, responsedata);
			mNetworkManager.StartHost();
			mJoinGamePanel.FinishConnection();
		}

		public int playerCount() { return mPlayerCount; }
		public GameData.MatchType matchType() { return mMatchType; }
	}
}
