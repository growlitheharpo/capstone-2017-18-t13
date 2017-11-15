using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to manage the gameover panel.
	/// </summary>
	public class GameOverPanel : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private GameObject mScorePrefab;
		[SerializeField] private GridLayoutGroup mScoreGrid;
		[SerializeField] private ActionProvider mQuitButton;

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			EventManager.LocalGUI.OnShowGameoverPanel += OnShowGameoverPanel;
			mQuitButton.OnClick += HandleQuit;

			gameObject.SetActive(false);
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.LocalGUI.OnShowGameoverPanel -= OnShowGameoverPanel;
			mQuitButton.OnClick -= HandleQuit;
		}

		/// <summary>
		/// EVENT HANDLER: LocalGUI.OnShowGameoverPanel
		/// </summary>
		private void OnShowGameoverPanel(PlayerScore[] scores)
		{
			NetworkInstanceId localPlayerId = FindObjectsOfType<CltPlayer>().First(x => x.isCurrentPlayer).netId;

			gameObject.SetActive(true);
			foreach (PlayerScore score in scores)
			{
				GameObject go = Instantiate(mScorePrefab);
				go.transform.SetParent(mScoreGrid.transform);

				Text text = go.GetComponent<Text>();
				text.text = string.Format("{0}\n{1}\n{2}\n{3}",
					score.playerId == localPlayerId ? "You" : "",
					score.player.playerName,
					score.kills,
					score.deaths);

				go.GetComponent<RectTransform>().position = Vector3.zero;
			}
		}

		/// <summary>
		/// Handle the player clicking the Quit button.
		/// Transition back to menu.
		/// </summary>
		private void HandleQuit()
		{
			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.MENU_SCENE);
		}
	}
}
