using FiringSquad.Data;
using KeatsLib.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// A little wrapper for the game over panel individual scorecards.
	/// Allows the gameover panel to easily set values.
	/// </summary>
	public class GameOverIndividualScorePanel : MonoBehaviour
	{
		[SerializeField] private Image mBackgroundImage;
		[SerializeField] private Text mPlayerNameText;
		[SerializeField] private Text mPlayerRank;
		[SerializeField] private Text mScoreText;
		[SerializeField] private Text mKillsText;
		[SerializeField] private Text mDeathsText;

		/// <summary>
		/// The player's name for this score.
		/// </summary>
		public string playerName
		{
			get { return mPlayerNameText.text; }
			set { mPlayerNameText.text = value; }
		}

		/// <summary>
		/// The player's rank for this score (starts at 1)
		/// </summary>
		public uint playerRank
		{
			set { mPlayerRank.text = value.ToStringOrdinal(); }
		}

		/// <summary>
		/// The player's score for this score panel
		/// </summary>
		public int playerScore
		{
			get { return int.Parse(mScoreText.text); }
			set { mScoreText.text = value.ToString("0000"); }
		}

		/// <summary>
		/// The kill count for this score
		/// </summary>
		public uint killCount
		{
			get { return uint.Parse(mKillsText.text); }
			set { mKillsText.text = value.ToString("00"); }
		}

		/// <summary>
		/// The death count for this score
		/// </summary>
		public uint deathCount
		{
			get { return uint.Parse(mDeathsText.text); }
			set { mDeathsText.text = value.ToString("00"); }
		}

		/// <summary>
		/// Apply the player's team color to their score.
		/// </summary>
		public void ApplyTeamColor(Color c)
		{
			mBackgroundImage.color = new Color(c.r, c.g, c.b, 0.25f);
		}
	}
}
