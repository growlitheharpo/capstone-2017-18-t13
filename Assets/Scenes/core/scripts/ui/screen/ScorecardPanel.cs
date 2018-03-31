﻿using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to manage the gameover panel.
	/// </summary>
	public class ScorecardPanel : MonoBehaviour, IScreenPanel
	{
		/// Inspector variables
		[SerializeField] private GameOverIndividualScorePanel mScorePrefab;
		[SerializeField] private LayoutGroup mScoreGrid;

		private Dictionary<CltPlayer, GameOverIndividualScorePanel> mScores;

		/// <inheritdoc />
		public bool disablesInput { get { return false; } }

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			ServiceLocator.Get<IUIManager>()
				.RegisterPanel(this, ScreenPanelTypes.Scorecard);

			mScores = new Dictionary<CltPlayer, GameOverIndividualScorePanel>();

			EventManager.Local.OnReceiveLobbyEndTime += OnReceiveLobbyEndTime;
			EventManager.Local.OnReceiveGameEndTime += OnReceiveGameEndTime;
			EventManager.LocalGeneric.OnPlayerScoreChanged += OnPlayerScoreChanged;
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnReceiveLobbyEndTime -= OnReceiveLobbyEndTime;
			EventManager.Local.OnReceiveGameEndTime -= OnReceiveGameEndTime;
			EventManager.LocalGeneric.OnPlayerScoreChanged -= OnPlayerScoreChanged;

			ServiceLocator.Get<IUIManager>()
				.UnregisterPanel(this);
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnReceiveLobbyEndTime
		/// </summary>
		private void OnReceiveLobbyEndTime(CltPlayer arg1, long arg2)
		{
			EventManager.Local.OnReceiveLobbyEndTime -= OnReceiveLobbyEndTime;
			TryHookUpAllUI();
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnReceiveGameEndTime
		/// </summary>
		private void OnReceiveGameEndTime(long obj)
		{
			EventManager.Local.OnReceiveGameEndTime -= OnReceiveGameEndTime;
			TryHookUpAllUI();
		}

		/// <summary>
		/// Attempt to grab ALL player references and update their names.
		/// </summary>
		private void TryHookUpAllUI()
		{
			var allPlayers = FindObjectsOfType<CltPlayer>();

			foreach (CltPlayer player in allPlayers)
			{
				GameOverIndividualScorePanel score;
				if (mScores.TryGetValue(player, out score))
				{
					score.ApplyTeamColor(player.teamColor);
					score.playerName = player.playerName;
					continue;
				}

				score = Instantiate(mScorePrefab.gameObject, mScoreGrid.transform)
					.GetComponent<GameOverIndividualScorePanel>();
				score.playerName = player.playerName;
				score.ApplyTeamColor(player.teamColor);

				mScores.Add(player, score);
			}
		}

		/// <summary>
		/// EVENT HANDLER: LocalGeneric.OnPlayerScoreChanged
		/// </summary>
		private void OnPlayerScoreChanged(CltPlayer player, int scoreChange, int killChange, int deathChange)
		{
			GameOverIndividualScorePanel score;

			if (!mScores.TryGetValue(player, out score))
			{
				score = Instantiate(mScorePrefab.gameObject, mScoreGrid.transform)
					.GetComponent<GameOverIndividualScorePanel>();

				score.ApplyTeamColor(player.teamColor);
				mScores.Add(player, score);
			}

			if (score.playerName != player.playerName)
				score.playerName = player.playerName;

			score.killCount = killChange > 0 ? (uint)killChange : 0;
			score.deathCount = deathChange > 0 ? (uint)deathChange : 0;
			score.playerScore += scoreChange;

			// SET RANK
			var scores = mScores.Values.OrderByDescending(x => x.playerScore).ToList();
			for (int i = 0; i < scores.Count; ++i)
			{
				scores[i].playerRank = (uint)i + 1;
				scores[i].transform.SetAsLastSibling();
			}
		}

		/// <inheritdoc />
		public void OnEnablePanel() { }

		/// <inheritdoc />
		public void OnDisablePanel() { }
	}
}
