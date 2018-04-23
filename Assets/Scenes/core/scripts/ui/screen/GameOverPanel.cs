using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.UI;
using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to manage the gameover panel.
	/// </summary>
	public class GameOverPanel : MonoBehaviour, IScreenPanel
	{
		/// Inspector variables
		[SerializeField] private GameOverIndividualScorePanel mScorePrefab;
		[SerializeField] private LayoutGroup mScoreGrid;
		[SerializeField] private ActionProvider mQuitButton;
		[SerializeField] private Text mBlueScore;
		[SerializeField] private Text mOrangeScore;

		/// <inheritdoc />
		public bool disablesInput { get { return true; } }

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			mQuitButton.OnClick += HandleQuit;

			ServiceLocator.Get<IUIManager>()
				.RegisterPanel(this, ScreenPanelTypes.GameOver);
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			mQuitButton.OnClick -= HandleQuit;

			ServiceLocator.Get<IUIManager>()
				.UnregisterPanel(this);
		}

		/// <summary>
		/// Set the scores displayed on the UI panel.
		/// </summary>
		/// <param name="scores">The array of scores that will be displayed.</param>
		public void SetDisplayScores(IList<PlayerScore> scores)
		{
			gameObject.SetActive(true);
			scores = scores.OrderByDescending(x => x.score).ToArray();

			int blue = 0, orange = 0;

			for (uint i = 0; i < scores.Count; ++i)
			{
				PlayerScore score = scores[(int)i];
				CltPlayer player = score.player;

				GameOverIndividualScorePanel panel = Instantiate(mScorePrefab.gameObject, mScoreGrid.transform)
					.GetComponent<GameOverIndividualScorePanel>();

				if (player.playerTeam == GameData.PlayerTeam.Deathmatch)
				{
					mOrangeScore.gameObject.SetActive(false);
					mBlueScore.gameObject.SetActive(false);
				}

				panel.ApplyTeamColor(player.teamColor);
				panel.playerRank = i + 1;
				panel.playerName = player.playerName;
				panel.playerScore = score.score;
				panel.killCount = score.kills > 0 ? (uint)score.kills : 0;
				panel.deathCount = score.deaths > 0 ? (uint)score.deaths : 0;

				if (player.playerTeam == GameData.PlayerTeam.Blue)
					blue += score.score;
				else if (player.playerTeam == GameData.PlayerTeam.Orange)
					orange += score.score;
			}

			mBlueScore.text = "BLUE: " + blue.ToString("0000");
			mOrangeScore.text = "ORANGE: " + orange.ToString("0000");

			// Force the player back to the main menu if they forget to click the button
			StartCoroutine(Coroutines.InvokeAfterSeconds(60.0f, HandleQuit));
		}

		/// <summary>
		/// Handle the player clicking the Quit button.
		/// Transition back to menu.
		/// </summary>
		private void HandleQuit()
		{
			EventManager.Notify(EventManager.Local.ConfirmQuitGame);
		}

		/// <inheritdoc />
		public void OnEnablePanel() { }

		/// <inheritdoc />
		public void OnDisablePanel() { }
	}
}
