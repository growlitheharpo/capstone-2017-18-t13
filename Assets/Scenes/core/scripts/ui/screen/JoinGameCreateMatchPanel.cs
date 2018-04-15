using System.Collections;
using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Networking;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	public class JoinGameCreateMatchPanel : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private InputField mNameEntryField;
		[SerializeField] private int mLevelDomain;
		[SerializeField] private Button mCancelButton01;
		[SerializeField] private Button mCancelButton02;
		[SerializeField] private Button mConfirmButton;
		[SerializeField] private Dropdown mPlayerCountDropdown;
		[SerializeField] private Dropdown mGameModeDropdown;

		/// Private variables
		private NetworkGameManager mNetworkManager;
		private JoinGamePanel mJoinGamePanel;
		private bool mPanelActive;

		// Info for starting the game
		private GameData.MatchType mMatchType;
		private Coroutine mRepeatCreateRoutine;
		private int mPlayerCount;

		/// <summary>
		/// The requested player count (or -1 if use default)
		/// </summary>
		public int playerCount { get { return mPlayerCount; } }

		/// <summary>
		/// The requested match type (or invalid if use default)
		/// </summary>
		public GameData.MatchType matchType { get { return mMatchType; } }

		/// <summary>
		/// The domain for this level when using networking.
		/// </summary>
		public int levelDomain { get { return mLevelDomain; } }

		/// <summary>
		/// Unity's Awake signal.
		/// </summary>
		private void Awake()
		{
			mMatchType = GameData.MatchType.Invalid;
			mPlayerCount = -1;

			mNetworkManager = FindObjectOfType<NetworkGameManager>();

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
			EventManager.Server.OnStartGame -= OnStartGame;
		}

		/// <summary>
		/// Unity's OnEnable signal.
		/// </summary>
		private void OnEnable()
		{
			string defaultName = ServiceLocator.Get<IGamestateManager>().currentUserName + "\'s Game";
			mNameEntryField.text = defaultName;

			mCancelButton01.onClick.AddListener(OnClickCancelButton);
			mCancelButton02.onClick.AddListener(OnClickCancelButton);
			mPanelActive = true;
		}

		/// <summary>
		/// Unity's OnDisable signal.
		/// </summary>
		private void OnDisable()
		{
			mCancelButton01.onClick.RemoveListener(OnClickCancelButton);
			mCancelButton02.onClick.RemoveListener(OnClickCancelButton);
			mPanelActive = false;
		}

		/// <summary>
		/// Click handler for the cancel button.
		/// </summary>
		private void OnClickCancelButton()
		{
			if (mPanelActive)
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

			mNetworkManager.matchMaker.CreateMatch(matchName, 5, true, "", Network.player.ipAddress, Network.player.ipAddress, 0, mLevelDomain, OnMatchCreate);
			mRepeatCreateRoutine = FindObjectOfType<EventSystem>().StartCoroutine(RepeatMatchCreation());
			EventManager.Server.OnStartGame += OnStartGame;
		}

		/// <summary>
		/// Handle
		/// </summary>
		private void OnStartGame(long time)
		{
			EventManager.Server.OnStartGame -= OnStartGame;
			StopCoroutine(mRepeatCreateRoutine);
		}

		/// <summary>
		/// Just keep making the match over and over until the end of time.
		/// </summary>
		/// <returns></returns>
		private IEnumerator RepeatMatchCreation()
		{
			while (true)
			{
				yield return new WaitForSeconds(30.0f);

				string matchName = mNameEntryField.text;
				matchName += ":" + Network.player.ipAddress;

				if (mNetworkManager.matchMaker != null)
					mNetworkManager.matchMaker.CreateMatch(matchName, 5, true, "", Network.player.ipAddress, Network.player.ipAddress, 0, mLevelDomain, (x, y, z) => {});
			}
			
			// ReSharper disable once IteratorNeverReturns
			// This coroutine is ended by StopCoroutine
		}

		/// <summary>
		/// Value change handler for the player count dropdown
		/// </summary>
		private void OnChangePlayerCount()
		{
			// Check the value of the player count
			int dropVal = mPlayerCountDropdown.value;

			if (dropVal == 0)
				mPlayerCount = 6;
			else if (dropVal == 1)
				mPlayerCount = 4;
		}

		/// <summary>
		/// Value change handler for the player's match type
		/// </summary>
		private void OnChangeGameType()
		{
			// Check the value of the player count
			int dropVal = mGameModeDropdown.value;

			if (dropVal == 0)
				mMatchType = GameData.MatchType.TeamDeathmatch;
			else if (dropVal == 1)
				mMatchType = GameData.MatchType.Deathmatch;
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
	}
}
