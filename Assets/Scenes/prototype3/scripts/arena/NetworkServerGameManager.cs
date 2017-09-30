using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public class NetworkServerGameManager : NetworkBehaviour
	{
		[SerializeField] private Gamemode.ArenaSettings mSettings;

		private BoundProperty<int> mPlayer1Score;
		private BoundProperty<int> mPlayer2Score;
		private BoundProperty<float> mRemainingTime;

		[SyncVar] private float mTime;

		private void Start()
		{
			mPlayer1Score = new BoundProperty<int>(0, GameplayUIManager.PLAYER1_SCORE);
			mPlayer2Score = new BoundProperty<int>(0, GameplayUIManager.PLAYER2_SCORE);
			mRemainingTime = new BoundProperty<float>(mSettings.roundTime, GameplayUIManager.ARENA_ROUND_TIME);

			mRemainingTime.value = -1.0f;
			mTime = -1.0f;
		}

		public void NotifyStartGame()
		{
			
		}
	}
}
