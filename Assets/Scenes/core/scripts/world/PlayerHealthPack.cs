using System.Collections;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using UnityEngine;
using UnityEngine.Networking;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Component for the healthkits that are scattered throughout the level.
	/// Provide the player with a set amount of health when colided with on the server.
	/// </summary>
	public class PlayerHealthPack : NetworkBehaviour
	{
		/// Inspector variables
		[SerializeField] private float mProvidedHealth;
		[SerializeField] private float mRotationRate;
		[SerializeField] private float mRespawnTime;

		/// Syncvars
		[SyncVar(hook = "OnChangeVisible")] private bool mVisible = true;

		/// Private variables
		private Collider mCollider;
		private GameObject mView;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mCollider = GetComponent<Collider>();
			mView = transform.Find("EnabledView").gameObject;
		}

		/// <summary>
		/// Handle starting on the client.
		/// </summary>
		/// <inheritdoc />
		public override void OnStartClient()
		{
			base.OnStartClient();

			if (mVisible)
				return;

			mCollider.enabled = false;
			mView.SetActive(false);
		}

		/// <summary>
		/// Unity's per-frame update function.
		/// </summary>
		private void Update()
		{
			DoRotation();
		}

		/// <summary>
		/// Rotate the healthkit around the world "up" axis.
		/// Note: the healthkit uses a sphere hitbox instead of the mesh, so rotation
		/// does not need to be synced exactly across the network.
		/// </summary>
		private void DoRotation()
		{
			transform.Rotate(Vector3.up, mRotationRate * Time.deltaTime);
		}

		/// <summary>
		/// Unity callback: The healthkit has been colided with on the server.
		/// </summary>
		[ServerCallback]
		private void OnTriggerEnter(Collider other)
		{
			IDamageReceiver player = other.GetComponent<IDamageReceiver>();
			if (player == null)
				return;

			CltPlayer realPlayer = player as CltPlayer;
			if (realPlayer != null && realPlayer.currentHealth >= realPlayer.defaultData.defaultHealth)
				return;

			player.HealDamage(mProvidedHealth);
			mVisible = false;
			StartCoroutine(WaitAndReappear());
		}

		/// <summary>
		/// Reappear after a certain amount of time has passed.
		/// </summary>
		[Server]
		private IEnumerator WaitAndReappear()
		{
			yield return new WaitForSeconds(mRespawnTime);
			mVisible = true;
		}

		/// <summary>
		/// Unity callback for when "visible" has changed.
		/// </summary>
		private void OnChangeVisible(bool newValue)
		{
			mVisible = newValue;

			if (!mVisible)
				CheckForAudio();

			mView.SetActive(mVisible);
			mCollider.enabled = mVisible;
		}

		/// <summary>
		/// Checks to see if we should play an audio event for being picked up.
		/// In order to avoid sending extra network messages, we use a slight hack
		/// where we see if the player closest to this pack is the local player. If yes,
		/// we play the sound.
		/// </summary>
		private void CheckForAudio()
		{
			var allPlayers = FindObjectsOfType<CltPlayer>().OrderBy(x => Vector3.Distance(transform.position, x.transform.position));
			CltPlayer first = allPlayers.FirstOrDefault();

			if (first == null || !first.isCurrentPlayer)
				return;

			IAudioReference audRef = ServiceLocator.Get<IAudioManager>()
				.CreateSound(AudioEvent.PlayerHealthPickup, first.transform, false);

			audRef.healthGained = mProvidedHealth;
			audRef.Start();
		}
	}
}
