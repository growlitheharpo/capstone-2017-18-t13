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
		public float currentHealth { get { return 100.0f; } }

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
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Reflect that we've been damaged locally
		/// </summary>
		[ClientRpc]
		private void RpcReflectDamage()
		{
			mViewAnimator.SetTrigger("Shot");

			// Play "ouch" sound here!
			// Just replace the event name and nothing else!
			ServiceLocator.Get<IAudioManager>()
				.CreateSound(AudioEvent.PlayerDamagedGrunt, null);
		}
	}
}
