using System;
using UnityEngine;
using Input = KeatsLib.Unity.Input;

namespace FiringSquad.Gameplay
{
	public class ArenaGameManager : MonoBehaviour
	{
		[SerializeField] private float mRoundTime;

		private BoundProperty<int> mPlayer1Score;
		private BoundProperty<int> mPlayer2Score;
		private BoundProperty<float> mRemainingTime;
		private PlayerScript[] mPlayerList;

		private void Awake()
		{
			mPlayer1Score = new BoundProperty<int>(0, GameplayUIManager.PLAYER1_SCORE);
			mPlayer2Score = new BoundProperty<int>(0, GameplayUIManager.PLAYER2_SCORE);
			mRemainingTime = new BoundProperty<float>(mRoundTime, GameplayUIManager.ARENA_ROUND_TIME);

			EventManager.OnPlayerDied += HandlePlayerDeath;
		}

		private void Start()
		{
			GeneratePlayerList();
		}

		private void GeneratePlayerList()
		{
			mPlayerList = FindObjectsOfType<PlayerScript>();
		}

		private void Update()
		{
			mRemainingTime.value -= Time.deltaTime;

			if (mRemainingTime.value <= 0.0f)
				HandleMatchEnd();
		}

		private void HandleMatchEnd()
		{
			ServiceLocator.Get<IInput>()
				.DisableInputLevel(Input.InputLevel.Gameplay);
		}

		private void HandlePlayerDeath(ICharacter obj)
		{
			if (ReferenceEquals(obj, mPlayerList[0]))
				mPlayer2Score.value += 1;
			else if (ReferenceEquals(obj, mPlayerList[1]))
				mPlayer1Score.value += 1;
			else
				throw new ArgumentException("We got an invalid player from OnPlayerDied!");
		}
	}
}
