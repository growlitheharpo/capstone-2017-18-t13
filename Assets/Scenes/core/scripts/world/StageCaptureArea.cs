using System.Collections.Generic;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	public class StageCaptureArea : NetworkBehaviour
	{
		[SerializeField] private float mTimeoutPeriod;
		[SerializeField] private float mCaptureTime;
		[SerializeField] private CollisionForwarder mTrigger;

		private float mCapturePercentage;
		private float mTimeout;

		private CltPlayer mCapturingPlayer;
		private List<CltPlayer> mBlockingPlayers;

		private void Awake()
		{
			mTrigger.mTriggerEnterDelegate = OnTriggerEnter;
			mTrigger.mTriggerExitDelegate = OnTriggerExit;
		}

		[ServerCallback]
		private void OnEnable()
		{
			mTimeout = mTimeoutPeriod;
			mCapturePercentage = 0.0f;
			mBlockingPlayers = new List<CltPlayer>(4);
			mCapturingPlayer = null;
		}

		[ServerCallback]
		private void Update()
		{
			UpdateCapturingPlayer();
			UpdateCapturePercent();
			UpdateTimeout();
		}

		private void UpdateCapturingPlayer()
		{
			// check if the previous capturer left or died
			if (mCapturingPlayer == null && mBlockingPlayers.Count > 0)
				mCapturingPlayer = mBlockingPlayers[0];
		}

		private void UpdateCapturePercent()
		{
			if (mCapturingPlayer != null && mBlockingPlayers.Count == 0)
				mCapturePercentage += Time.deltaTime;
		}

		private void UpdateTimeout()
		{
			
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!isServer)
				return;

			CltPlayer player = other.GetComponent<CltPlayer>();
			if (player == null)
				return;

			if (mBlockingPlayers.Contains(player) || mCapturingPlayer == player)
				return;

			if (mCapturingPlayer == null)
				mCapturingPlayer = player;
			else
				mBlockingPlayers.Add(player);
		}

		private void OnTriggerExit(Collider other)
		{
			if (!isServer)
				return;

			CltPlayer player = other.GetComponent<CltPlayer>();
			if (player == null)
				return;

			if (mCapturingPlayer == player)
				mCapturingPlayer = null;
			else
				mBlockingPlayers.Remove(player);
		}
	}
}
