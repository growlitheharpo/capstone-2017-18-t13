using FiringSquad.Core.Audio;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	/// <summary>
	/// Class to handle reflecting the local visuals and audio of hitscan projectiles
	/// to avoid saturating the network with Spawn/Despawn commands.
	/// </summary>
	public class HitscanProjectileLocalManager : NetworkBehaviour
	{
		/// Inspector variables
		[SerializeField] private GameObject mHitscanProjectilePrefab;

		/// <summary>
		/// Unity's client-side start event.
		/// </summary>
		public override void OnStartClient()
		{
			StartCoroutine(Coroutines.InvokeAfterFrames(5, () =>
			{
				NetworkManager.singleton.client.RegisterHandler(HitscanProjectile.HITSCAN_MESSAGE_TYPE, OnHitscanMessage);
			}));
		}

		/// <summary>
		/// Unity's destroy event.
		/// </summary>
		private void OnDestroy()
		{

			NetworkManager.singleton.client.UnregisterHandler(HitscanProjectile.HITSCAN_MESSAGE_TYPE);
		}

		/// <summary>
		/// Handle a hitscan projectile being spawned on the Network.
		/// </summary>
		/// <param name="netMsg"></param>
		private void OnHitscanMessage(NetworkMessage netMsg)
		{
			HitscanProjectile.HitscanMessage msg = netMsg.ReadMessage<HitscanProjectile.HitscanMessage>();

			GameObject sourceObj = ClientScene.FindLocalObject(msg.mSource);
			IWeaponBearer source = sourceObj.GetComponent<IWeaponBearer>();

			AudioEvent audioEvent = BaseProjectileScript.GetHitAudioEvent(msg.mHitObject);

			HitscanProjectile instance = Instantiate(mHitscanProjectilePrefab).GetComponent<HitscanProjectile>();
			instance.PositionAndVisualize(source.weapon, msg.mEnd, audioEvent);
		}
	}
}
