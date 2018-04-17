using System;
using System.Collections;
using FiringSquad.Core;
using FiringSquad.Data;
using FiringSquad.Debug;
using FiringSquad.Gameplay;
using UnityEngine;
using UnityEngine.Networking;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Prototyping
{
	/// <summary>
	/// Target should now be networked.
	/// </summary>
	public class SampleTargetScript : NetworkBehaviour, IDamageReceiver
	{
		/// Inspector variables
		[SerializeField] private ParticleSystem mDeathParticles;
		[SerializeField] private GameObject mHitIndicator;
		[SerializeField] private GameObject mMesh;
		[SerializeField] private UIText mText;
		[SerializeField] private float mStartHealth;

		/// Private variables
		private float mHealth;

		/// <inheritdoc />
		public float currentHealth { get { return mHealth; } }

		public GameData.PlayerTeam playerTeam { get { return GameData.PlayerTeam.Deathmatch; } }

		/// <summary>
		/// The health of this target.
		/// </summary>
		public float health { get { return mHealth; } }

		/// <summary>
		/// Unity function: first frame on server.
		/// </summary>
		public override void OnStartServer()
		{
			mHealth = 100;
		}

		/// <summary>
		/// Unity function: first frame on client.
		/// </summary>
		public override void OnStartClient()
		{
		}

		[ServerCallback]
		private void Update()
		{
			if (mHealth <= 0.0f)
				return;
		}

		/// <summary>
		/// Cleanup listeners and event listeners.
		/// </summary>
		private void OnDestroy()
		{
			ServiceLocator.Get<IGameConsole>()
				.UnregisterCommand("target");
		}

		/// <summary>
		/// Handle the console reset command.
		/// </summary>
		private static void CONSOLE_Reset(string[] args)
		{
			var allObjects = FindObjectsOfType<SampleTargetScript>();

			switch (args[0].ToLower())
			{
				case "reset":
					foreach (SampleTargetScript obj in allObjects)
					{
						obj.mHealth = obj.mStartHealth;
						obj.mMesh.SetActive(true);
					}
					break;
				case "sethealth":
					foreach (SampleTargetScript obj in allObjects)
					{
						obj.mHealth = float.Parse(args[1]);
						obj.mMesh.SetActive(true);
					}
					break;
				default:
					throw new ArgumentException("Invalid arguments for command: target");
			}
		}

		/// <inheritdoc />
		public void HealDamage(float amount)
		{
			mHealth += amount;
		}

		/// <inheritdoc />
		[Server]
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause, bool wasHeadshot)
		{
			RpcReflectTurretDamageLocally(point, normal, cause.source.gameObject.transform.position, amount, cause.source.netId);

			if (mHealth <= 0.0f)
				return;

			mHealth = Mathf.Clamp(mHealth - amount, 0.0f, float.MaxValue);

			if (mHealth <= 0.0f)
				Die();
		}

		/// <summary>
		/// Reflect damage that occured on the server on each local client.
		/// </summary>
		/// <param name="point">The point where the damage occurred.</param>
		/// <param name="normal">The normal of the hit.</param>
		/// <param name="origin">The position where the hit originated from.</param>
		/// <param name="amount">The amount of damage that was caused.</param>
		/// <param name="source">The network id of the source of the damage.</param>
		[ClientRpc]
		private void RpcReflectTurretDamageLocally(Vector3 point, Vector3 normal, Vector3 origin, float amount, NetworkInstanceId source)
		{
			ICharacter realSource = ClientScene.FindLocalObject(source).GetComponent<ICharacter>();
			if (realSource.isCurrentPlayer)
				EventManager.Notify(() => EventManager.Local.LocalPlayerCausedDamage(amount));
		}

		/// <summary>
		/// Handle the target hitting 0 health.
		/// </summary>
		private void Die()
		{
			//mMesh.SetActive(false);
			// Player animation here
			RpcReflectDeathLocally();
			mDeathParticles.Play();
		}

		/// <summary>
		/// Handle the target dying on the clients
		/// </summary>
		[ClientRpc]
		private void RpcReflectDeathLocally()
		{
		}
	}
}
