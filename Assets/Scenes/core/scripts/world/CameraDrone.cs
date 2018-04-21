using FiringSquad.Core;
using FiringSquad.Core.Audio;
using UnityEngine;
using UnityEngine.Networking;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Gameplay.NPC
{
	/// <summary>
	/// Network script to sync the camera drone across all instances
	/// </summary>
	public class CameraDrone : NetworkBehaviour, IDamageReceiver
	{
		/// Inspector variables
		[SerializeField] private string mAnimatorEntryState;
		[SerializeField] private Animator mViewAnimator;

		/// Private variables
		private Animator mMovementAnimator;

		/// <inheritdoc />
		public float currentHealth { get { return default(float); } }

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mMovementAnimator = GetComponent<Animator>();

			if (string.IsNullOrEmpty(mAnimatorEntryState))
				Logger.Warn("CameraBot " + gameObject.name + " does not have an entry state!");
		}

		/// <summary>
		/// Unity's Start function for client-side code.
		/// </summary>
		public override void OnStartClient()
		{
			EventManager.Local.OnIntroBegin += OnIntroBegin;
			EventManager.Local.OnReceiveGameEndTime += OnReceiveGameEndTime;
		}

		/// <summary>
		/// Unity's OnDestroy function. Do event cleanup.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnIntroBegin -= OnIntroBegin;
			EventManager.Local.OnReceiveGameEndTime -= OnReceiveGameEndTime;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnIntroBegin.
		/// Start our movement.
		/// </summary>
		private void OnIntroBegin()
		{
			EventManager.Local.OnIntroBegin -= OnIntroBegin;
			BeginMovementAnimation();
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnIntroBegin.
		/// Start our movement (as network synced as possible).
		/// </summary>
		private void OnReceiveGameEndTime(long gameEndTime)
		{
			EventManager.Local.OnReceiveGameEndTime -= OnReceiveGameEndTime;
			BeginMovementAnimation();
		}

		/// <summary>
		/// Start our animation based on what we were provided with.
		/// </summary>
		private void BeginMovementAnimation()
		{
			mMovementAnimator.SetTrigger(mAnimatorEntryState);
		}

		/// <inheritdoc />
		public void HealDamage(float amount) { }

		/// <inheritdoc />
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause, bool wasHeadshot)
		{
			UnityEngine.Debug.Log("We got hit! " + gameObject.name);
			RpcReflectDamage(cause.source.netId);
		}

		/// <summary>
		/// Reflect that we've been damaged locally
		/// </summary>
		[ClientRpc]
		private void RpcReflectDamage(NetworkInstanceId sourceId)
		{
			mViewAnimator.SetTrigger("Shot");

			float wasHitByPlayer = 0.0f;

			// Check if the damage source is the local player
			GameObject sourceGo = ClientScene.FindLocalObject(sourceId);
			if (sourceGo != null)
			{
				CltPlayer player = sourceGo.GetComponent<CltPlayer>();
				if (player != null && player.isCurrentPlayer)
					wasHitByPlayer = 1.0f;
			}

			// Play "ouch" sound here!
			// Just replace the event name and the parameter name!
			ServiceLocator.Get<IAudioManager>()
				.CreateSound(AudioEvent.CameraPain, transform) 
				.SetParameter("IsCurrentPlayer", wasHitByPlayer);
		}
	}
}
