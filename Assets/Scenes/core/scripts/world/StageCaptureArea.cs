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

		[SyncVar(hook = "OnCapturableChanged")] private bool mCapturable;
		private float mCapturePercentageTimer;
		private float mTimeoutTimer;

		private CltPlayer mCapturingPlayer;
		private CltPlayer currentCapturingPlayer
		{
			get
			{
				return mCapturingPlayer;
			}
			set
			{
				if (value != mCapturingPlayer)
				{
					mCapturingPlayer = value;
					mTimeoutTimer = 0.0f;
				}
			}
		}

		private List<CltPlayer> mBlockingPlayers;
		private List<GameObject> mChildren;

		private void Awake()
		{
			mTrigger.mTriggerEnterDelegate = OnTriggerEnter;
			mTrigger.mTriggerExitDelegate = OnTriggerExit;

			mChildren = new List<GameObject>(transform.childCount);
			foreach (Transform c in transform)
				mChildren.Add(c.gameObject);
		}

		public override void OnStartServer()
		{
			Enable();
			Disable();
		}

		public override void OnStartClient()
		{
			ReflectActiveState(mCapturable);
		}

		[Server]
		public void Enable()
		{
			mTimeoutTimer = 0.0f;
			mCapturePercentageTimer = 0.0f;
			mBlockingPlayers = new List<CltPlayer>(4);
			currentCapturingPlayer = null;
			mCapturable = true;
		}

		[Server]
		public void Disable()
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
			if (currentCapturingPlayer == null && mBlockingPlayers.Count > 0)
			{
				currentCapturingPlayer = mBlockingPlayers[0];
				mBlockingPlayers.RemoveAt(0);
			}
		}

		[Server]
		private void UpdateCapturePercent()
		{
			if (currentCapturingPlayer != null && mBlockingPlayers.Count == 0)
			{
				mCapturePercentageTimer += Time.deltaTime;

				if (mCapturePercentageTimer >= mCaptureTime)
				{
					EventManager.Notify(() => EventManager.Server.PlayerCapturedStage(this, currentCapturingPlayer));
					mCapturable = false;
				}
			}
		}

		[Server]
		private void UpdateTimeout()
		{
			if (currentCapturingPlayer == null)
			{
				mTimeoutTimer += Time.deltaTime;

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

			if (mBlockingPlayers.Contains(player) || currentCapturingPlayer == player)
				return;

			if (currentCapturingPlayer == null)
				currentCapturingPlayer = player;
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

			if (currentCapturingPlayer == player)
				currentCapturingPlayer = null;
			else
				mBlockingPlayers.Remove(player);
		}

		private void OnCapturableChanged(bool newValue)
		{
			mCapturable = newValue;
			ReflectActiveState(mCapturable);
		}

		private void ReflectActiveState(bool state)
		{
			foreach (GameObject child in mChildren)
				child.SetActive(state);
		}
	}
}
