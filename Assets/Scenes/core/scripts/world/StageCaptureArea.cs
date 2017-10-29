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

		private bool mCapturable;
		private float mCapturePercentageTimer;
		private float mTimeoutTimer;

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
			mTimeoutTimer = mTimeoutPeriod;
			mCapturePercentageTimer = 0.0f;
			mBlockingPlayers = new List<CltPlayer>(4);
			mCapturingPlayer = null;
			mCapturable = true;
		}

		[ServerCallback]
		private void OnDisable()
		{
			mCapturable = false;
		}

		[ServerCallback]
		private void Update()
		{
			if (!mCapturable)
				return;

			UpdateCapturingPlayer();
			UpdateCapturePercent();
			UpdateTimeout();
		}

		[Server]
		private void UpdateCapturingPlayer()
		{
			// check if the previous capturer left or died
			if (mCapturingPlayer == null && mBlockingPlayers.Count > 0)
			{
				mCapturingPlayer = mBlockingPlayers[0];
				mBlockingPlayers.RemoveAt(0);
			}
		}

		[Server]
		private void UpdateCapturePercent()
		{
			if (mCapturingPlayer != null && mBlockingPlayers.Count == 0)
			{
				mCapturePercentageTimer += Time.deltaTime;

				if (mCapturePercentageTimer >= mCaptureTime)
				{
					EventManager.Notify(() => EventManager.Server.PlayerCapturedStage(this, mCapturingPlayer));
					mCapturable = false;
				}
			}
		}

		[Server]
		private void UpdateTimeout()
		{
			if (mCapturingPlayer == null)
			{
				mTimeoutTimer -= Time.deltaTime;

				if (mTimeoutTimer >= mTimeoutPeriod)
				{
					mCapturable = false;
					EventManager.Notify(() => EventManager.Server.StageTimedOut(this));
				}
			}
		}

		// [Server] through return check
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

		// [Server] through return check
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
