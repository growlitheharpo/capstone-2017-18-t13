using System.Collections;
using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Debug;
using FiringSquad.Networking;
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
		[SerializeField] private Button mCancelButton;
		[SerializeField] private Button mConfirmButton;

		/// Private variables
		private NetworkGameManager mNetworkManager;
		private JoinGamePanel mJoinGamePanel;

		/// <summary>
		/// Unity's Awake signal.
		/// </summary>
		private void Awake()
		{
			mNetworkManager = FindObjectOfType<NetworkGameManager>();

			mCancelButton.onClick.AddListener(OnClickCancelButton);
			mConfirmButton.onClick.AddListener(OnClickConfirmButton);

			mJoinGamePanel = GetComponentInParent<JoinGamePanel>();
		}

		/// <summary>
		/// Unity's OnDestroy signal.
		/// </summary>
		private void OnDestroy()
		{
			mCancelButton.onClick.RemoveListener(OnClickCancelButton);
			mConfirmButton.onClick.RemoveListener(OnClickConfirmButton);
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
			mNetworkManager.matchMaker.CreateMatch(matchName, 5, true, "", Network.player.ipAddress, Network.player.ipAddress, 0, 0, OnMatchCreate);
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
