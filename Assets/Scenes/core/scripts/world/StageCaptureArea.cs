using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Data;
using FiringSquad.Gameplay.UI;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Represents a stage area in the game. Stages can be captured to win legendary parts.
	/// Stages are captured while a player is alone in the area. If other players are in the area,
	/// they block the capture.
	/// Capture percentage resets when the player exits the zone.
	/// Stage areas time out after a certan amount of time.
	/// </summary>
	public class StageCaptureArea : NetworkBehaviour
	{
		/// Inspector variables
		[SerializeField] private float mTimeoutPeriod;
		[SerializeField] private float mCaptureTime;
		[SerializeField] private CollisionForwarder mTrigger;

		/// Sync variables
		/// TODO: Most of these do not NEED to be syncvar'd. There are other ways to do this.
		[SyncVar(hook = "OnCapturableChanged")] private bool mCapturable;
		[SyncVar(hook = "OnCapturePercentageChanged")] private float mCapturePercentageTimer;
		[SyncVar(hook = "OnTimeoutTimerChanged")] private float mTimeoutTimer;
		[SyncVar(hook = "OnCapturePlayerIdChanged")] private NetworkInstanceId mCapturingPlayerId;

		/// Private variables
		private static StageCaptureUI kUIManager;
		private CltPlayer mCapturingPlayer;
		private List<CltPlayer> mBlockingPlayers, mTeamPlayers;
		private List<GameObject> mChildren;

		/// <summary>
		/// The capturing player.
		/// </summary>
		private CltPlayer currentCapturingPlayer
		{
			get
			{
				return mCapturingPlayer;
			}
			set
			{
				if (value == mCapturingPlayer)
					return;

				mCapturingPlayerId = value == null ? NetworkInstanceId.Invalid : value.netId;
				mCapturingPlayer = value;
				mCapturePercentageTimer = 0.0f;
			}
		}

		/// <summary>
		/// Unity's Awake function. Called on Server and Client.
		/// </summary>
		private void Awake()
		{
			if (kUIManager == null)
				kUIManager = FindObjectOfType<StageCaptureUI>();

			mTrigger.mTriggerEnterDelegate = OnTriggerEnter;
			mTrigger.mTriggerExitDelegate = OnTriggerExit;

			mChildren = new List<GameObject>(transform.childCount);
			foreach (Transform c in transform)
				mChildren.Add(c.gameObject);
		}

		/// <summary>
		/// On Start: Server-side.
		/// </summary>
		public override void OnStartServer()
		{
			Enable();
			Disable();
		}

		/// <summary>
		/// On Start: Client-side.
		/// </summary>
		public override void OnStartClient()
		{
			ReflectActiveState(mCapturable);
		}

		/// <summary>
		/// Enable this stage immediately.
		/// Makes the stage visible and capturable.
		/// </summary>
		[Server]
		public void Enable()
		{
			mTimeoutTimer = 0.0f;
			mCapturePercentageTimer = 0.0f;
			mBlockingPlayers = new List<CltPlayer>(4);
			mTeamPlayers = new List<CltPlayer>(4);
			currentCapturingPlayer = null;
			mCapturable = true;
		}

		/// <summary>
		/// Disable the stage. Hides its visual effect and makes it uncapturable.
		/// </summary>
		[Server]
		public void Disable()
		{
			mCapturable = false;
		}

		/// <summary>
		/// Unity's Update function. Called on Server only.
		/// </summary>
		[ServerCallback]
		private void Update()
		{
			if (!mCapturable)
				return;

			UpdateCapturingPlayer();
			UpdateCapturePercent();
			UpdateTimeout();
		}

		/// <summary>
		/// Check if the previous capturer left or died. Update the capturing player appropriately.
		/// </summary>
		[Server]
		private void UpdateCapturingPlayer()
		{
			// If the current capturer left or died...
			if (currentCapturingPlayer == null)
			{
				// First check for a teammate
				if (mTeamPlayers.Count > 0)
				{
					// Transfer to them if found
					currentCapturingPlayer = mTeamPlayers[0];
					mTeamPlayers.RemoveAt(0);
				}
				else if (mBlockingPlayers.Count > 0)
				{
					// If no teammate was found but there are blocking players, transfer to them
					currentCapturingPlayer = mBlockingPlayers[0];
					mBlockingPlayers.RemoveAt(0);

					// the "capture team" swapped, so we need to swap the arrays.
					mTeamPlayers = new List<CltPlayer>(mBlockingPlayers);

					// We know there are no players on the other team (the previous if), so clear the blocking list.
					mBlockingPlayers.Clear();
				}
			}
		}

		/// <summary>
		/// Update our capture percent based on whether or not we have a capturing player.
		/// </summary>
		[Server]
		private void UpdateCapturePercent()
		{
			// If we have a capturer and no blockers...
			if (currentCapturingPlayer != null && mBlockingPlayers.Count == 0)
			{
				float increase = Time.deltaTime + mTeamPlayers.Count * Time.deltaTime;
				mCapturePercentageTimer += increase;

				if (mCapturePercentageTimer >= mCaptureTime)
				{
					EventManager.Notify(() => EventManager.Server.PlayerCapturedStage(this, currentCapturingPlayer));
					mCapturable = false;
				}
			}
		}

		/// <summary>
		/// Update the timeout for this stage if there are no capturing players.
		/// </summary>
		[Server]
		private void UpdateTimeout()
		{
			if (currentCapturingPlayer != null)
				return;

			mTimeoutTimer += Time.deltaTime;

			if (mTimeoutTimer >= mTimeoutPeriod)
			{
				mCapturable = false;
				EventManager.Notify(() => EventManager.Server.StageTimedOut(this));
			}
		}

		/// <summary>
		/// Handle the trigger being entered.
		/// Will only run on server.
		/// </summary>
		[ServerCallback]
		private void OnTriggerEnter(Collider other)
		{
			CltPlayer player = other.GetComponent<CltPlayer>();
			if (player == null)
				return;

			if (mBlockingPlayers.Contains(player) || currentCapturingPlayer == player)
				return;

			if (currentCapturingPlayer == null)
				currentCapturingPlayer = player;
			else
			{
				if (player.playerTeam == GameData.PlayerTeam.Deathmatch || player.playerTeam != currentCapturingPlayer.playerTeam)
					mBlockingPlayers.Add(player);
				else
					mTeamPlayers.Add(player);
			}
		}

		/// <summary>
		/// Handle the trigger being exited.
		/// Will only run on server.
		/// </summary>
		[ServerCallback]
		private void OnTriggerExit(Collider other)
		{
			CltPlayer player = other.GetComponent<CltPlayer>();
			if (player == null)
				return;

			if (currentCapturingPlayer == player)
				currentCapturingPlayer = null;
			else
			{
				mBlockingPlayers.Remove(player);
				mTeamPlayers.Remove(player);
			}
		}

		/// <summary>
		/// Handle our capturable state being changed.
		/// </summary>
		/// <param name="newValue">Whether or not we are capturable/visible.</param>
		private void OnCapturableChanged(bool newValue)
		{
			mCapturable = newValue;
			ReflectActiveState(mCapturable);
		}

		/// <summary>
		/// Handle the capture ID changing.
		/// TODO: We can use this to determine the capture percentage and timeout locally!
		/// </summary>
		/// <param name="id">The netid of the new capturing player.</param>
		private void OnCapturePlayerIdChanged(NetworkInstanceId id)
		{
			mCapturingPlayerId = id;
			if (id == NetworkInstanceId.Invalid)
			{
				kUIManager.SetMode(StageCaptureUI.Mode.NoCapturing, this);
				return;
			}

			CltPlayer player = ClientScene.FindLocalObject(mCapturingPlayerId).GetComponent<CltPlayer>();
			kUIManager.SetMode(player.isCurrentPlayer ? StageCaptureUI.Mode.WereCapturing : StageCaptureUI.Mode.OtherCapturing, this);
		}

		/// <summary>
		/// Handle the capture percentage changing.
		/// TODO: This can be done locally based on the capture player ID changing.
		/// </summary>
		/// <param name="p">The new time that someone has been capturing for.</param>
		private void OnCapturePercentageChanged(float p)
		{
			mCapturePercentageTimer = p;
			kUIManager.SetCapturePercent(p / mCaptureTime);

			if (p / mCaptureTime >= 1.0f)
				ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.AnnouncerStageAreaCaptured, transform);
		}

		/// <summary>
		/// Handle the timeout timer changing.
		/// TODO: This can be done locally based on the capture player ID changing.
		/// </summary>
		/// <param name="t">The new timeout time.</param>
		private void OnTimeoutTimerChanged(float t)
		{
			mTimeoutTimer = t;
			kUIManager.SetRemainingTime(mTimeoutPeriod - t);
		}

		/// <summary>
		/// Reflect visibility and capturability. Update the UI.
		/// </summary>
		/// <param name="active">Whether or not the stage is capturable.</param>
		private void ReflectActiveState(bool active)
		{
			foreach (GameObject child in mChildren)
				child.SetActive(active);

			kUIManager.SetMode(active ? StageCaptureUI.Mode.NoCapturing : StageCaptureUI.Mode.NoPoints, this);
			if (active)
				ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.AnnouncerStageAreaSpawns, transform);
		}
	}
}
