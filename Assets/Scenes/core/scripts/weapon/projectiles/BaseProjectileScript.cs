using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc cref="IProjectile" />
	public abstract class BaseProjectileScript : NetworkBehaviour, IProjectile
	{
		/// Inspector variables
		[SerializeField] private float mAudioWeaponType;

		/// <inheritdoc />
		public ICharacter source { get { return sourceWeapon.bearer; } }

		/// <inheritdoc />
		public IWeapon sourceWeapon { get; protected set; }

		/// <inheritdoc />
		[Server]
		public virtual bool PreSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data)
		{
			sourceWeapon = weapon;
			return true;
		}

		/// <inheritdoc />
		[Server]
		public virtual void PostSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data)
		{
			sourceWeapon = weapon;
		}
		
		/// <summary>
		/// Play a sound for this projectile across all clients.
		/// TODO: This should NOT be an RPC. This should be determined locally after the projectile is spawned.
		/// </summary>
		/// <param name="hitObj">The netId of the object that was hit, or invalid if none were.</param>
		/// <param name="position">The world position to spawn the sound.</param>
		[ClientRpc]
		protected void RpcPlaySound(NetworkInstanceId hitObj, Vector3 position) // TODO: Should this be an RPC?? Poorly optimized!!
		{
			AudioEvent e = GetHitAudioEvent(hitObj);

			IAudioReference effect = ServiceLocator.Get<IAudioManager>().CreateSound(e, transform, position, Space.World, false);
			effect.weaponType = mAudioWeaponType;
			effect.Start();
		}

		/// <summary>
		/// Choose an appropriate audio event based on the type of object that was hit.
		/// </summary>
		/// <param name="hitObject">The network instance ID of the object that was hit.</param>
		[Client]
		public static AudioEvent GetHitAudioEvent(NetworkInstanceId hitObject)
		{
			if (hitObject == NetworkInstanceId.Invalid)
				return AudioEvent.ImpactWall;

			GameObject obj = ClientScene.FindLocalObject(hitObject);
			if (obj == null)
				return AudioEvent.ImpactWall;

			ICharacter player = obj.GetComponent<ICharacter>();
			if (player == null)
				return AudioEvent.ImpactWall;

			return player.isCurrentPlayer ? AudioEvent.ImpactCurrentPlayer : AudioEvent.ImpactOtherPlayer;
		}
	}
}
