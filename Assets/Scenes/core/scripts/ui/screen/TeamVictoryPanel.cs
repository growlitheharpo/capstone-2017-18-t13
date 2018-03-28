﻿using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Core.UI;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UIText = UnityEngine.UI.Text;


namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to manage the gameover panel.
	/// </summary>
	public class TeamVictoryPanel : MonoBehaviour, IScreenPanel
	{
		/// Inspector variables
		[SerializeField] private UIText mLabel1;
		[SerializeField] private UIText mTeamName;
		[SerializeField] private UIText mLabel2;

		[SerializeField] private GameObject mBorder1;
		[SerializeField] private GameObject mBorder2;

		int mTimer = 100;

		// Team scores
		int mOrangeScore, mBlueScore;

		IList<PlayerScore> mScores;

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			ServiceLocator.Get<IUIManager>()
				.RegisterPanel(this, ScreenPanelTypes.TeamVictory);

			gameObject.SetActive(false);
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			ServiceLocator.Get<IUIManager>()
				.UnregisterPanel(this);
		}

		private void Update()
		{
			mTimer--;
			if (mTimer < 0)
			{
				EventManager.Notify(() => EventManager.Local.ReceiveFinishEvent(mScores));
				Destroy(this);
			}
		}

		public void TallyScores(IList<PlayerScore> scores)
		{
			gameObject.SetActive(true);

			mScores = scores;

			bool deathmatch = false;

			for (uint i = 0; i < scores.Count; ++i)
			{
				PlayerScore score = scores[(int)i];
				CltPlayer player = score.player;

				if (player.playerTeam == GameData.PlayerTeam.Blue)
				{
					mBlueScore += score.score;
				}
				else if (player.playerTeam == GameData.PlayerTeam.Orange)
				{
					mOrangeScore += score.score;
				}
				else
				{
					deathmatch = true;
				}
			}

			if (mBlueScore > mOrangeScore)
			{
				mTeamName.text = GameData.PlayerTeam.Blue.ToString().ToUpper();
				mTeamName.color = scores[0].player.defaultData.blueTeamColor;
				mBorder1.GetComponent<RawImage>().color = scores[0].player.defaultData.blueTeamColor;
				mBorder2.GetComponent<RawImage>().color = scores[0].player.defaultData.blueTeamColor;
			}
			else if (mOrangeScore > mBlueScore)
			{
				mTeamName.text = GameData.PlayerTeam.Orange.ToString().ToUpper();
				mTeamName.color = scores[0].player.defaultData.orangeTeamColor;
				mBorder1.GetComponent<RawImage>().color = scores[0].player.defaultData.orangeTeamColor;
				mBorder2.GetComponent<RawImage>().color = scores[0].player.defaultData.orangeTeamColor;
			}
			else if (!deathmatch)
			{
				mLabel1.text = "It was a";
				mLabel2.text = "";
				mTeamName.text = "TIE";
			}
			else
			{
				mLabel1.text = "THIS WAS A";
				mLabel2.text = "GAME";
			}
		}

		/// <inheritdoc />
		public void OnEnablePanel() { }

		/// <inheritdoc />
		public void OnDisablePanel() { }
	}
}