using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Core.UI;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;
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

			for (uint i = 0; i < scores.Count; ++i)
			{
				PlayerScore score = scores[(int)i];
				CltPlayer player = score.player;

				GameOverIndividualScorePanel panel = Instantiate(mScorePrefab.gameObject, mScoreGrid.transform)
					.GetComponent<GameOverIndividualScorePanel>();

				panel.ApplyTeamColor(player.teamColor);
				panel.playerRank = i + 1;
				panel.playerName = player.playerName;
				panel.playerScore = score.score;
				panel.killCount = score.kills > 0 ? (uint)score.kills : 0;
				panel.deathCount = score.deaths > 0 ? (uint)score.deaths : 0;
			}
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
