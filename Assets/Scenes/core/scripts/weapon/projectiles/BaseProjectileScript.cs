using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	public abstract class BaseProjectileScript : NetworkBehaviour, IProjectile
	{
		[SerializeField] private float mAudioWeaponType;

		public ICharacter source { get { return sourceWeapon.bearer; } }
		public IWeapon sourceWeapon { get; protected set; }

		[Server]
		public virtual void PreSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data)
		{
			sourceWeapon = weapon;
		}

		[Server]
		public virtual void PostSpawnInitialize(IWeapon weapon, Ray initialDirection, WeaponData data)
		{
			sourceWeapon = weapon;
		}
		
		[ClientRpc]
		protected void RpcPlaySound(NetworkInstanceId hitObj, Vector3 position) // TODO: Should this be an RPC?? Poorly optimized!!
		{
			AudioEvent e = GetHitAudioEvent(hitObj);

			IAudioReference effect = ServiceLocator.Get<IAudioManager>().CreateSound(e, transform, position, Space.World, false);
			effect.weaponType = mAudioWeaponType;
			effect.Start();
		}

		[Client]
		private AudioEvent GetHitAudioEvent(NetworkInstanceId hitObject)
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
