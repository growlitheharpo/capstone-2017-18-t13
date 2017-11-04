using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.State;
using FiringSquad.Data;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	public class GameOverPanel : MonoBehaviour
	{
		[SerializeField] private GameObject mScorePrefab;
		[SerializeField] private GridLayoutGroup mScoreGrid;
		[SerializeField] private ActionProvider mQuitButton;

		private void Start()
		{
			EventManager.LocalGUI.OnShowGameoverPanel += HandleGameover;
			mQuitButton.OnClick += HandleQuit;

			gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			EventManager.LocalGUI.OnShowGameoverPanel -= HandleGameover;
			mQuitButton.OnClick -= HandleQuit;
		}

		private void HandleQuit()
		{
			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.MENU_SCENE);
		}

		private void HandleGameover(PlayerScore[] scores)
		{
			NetworkInstanceId localPlayerId = FindObjectsOfType<CltPlayer>().First(x => x.isCurrentPlayer).netId;

			gameObject.SetActive(true);
			for (int i = 0; i < scores.Length; i++)
			{
				PlayerScore score = scores[i];

				GameObject go = Instantiate(mScorePrefab);
				go.transform.SetParent(mScoreGrid.transform);

				Text text = go.GetComponent<Text>();
				text.text = string.Format("{0}\n{1}\n{2}\n{3}",
					score.playerId == localPlayerId ? "You" : "",
					"P" + (i + 1),
					score.kills,
					score.deaths);

				go.GetComponent<RectTransform>().position = Vector3.zero;
			}
		}
	}
}
