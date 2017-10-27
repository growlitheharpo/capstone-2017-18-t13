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

		protected void PlaySound(AudioEvent e)
		{
			if (!isClient)
				return;

			IAudioReference effect = ServiceLocator.Get<IAudioManager>().CreateSound(e, transform, false);
			effect.weaponType = mAudioWeaponType;
			effect.Start();
		}

		protected void PlaySound(AudioEvent e, Vector3 position)
		{
			if (!isClient)
				return;

			IAudioReference effect = ServiceLocator.Get<IAudioManager>().CreateSound(e, transform, position, Space.World, false);
			effect.weaponType = mAudioWeaponType;
			effect.Start();
		}

		protected AudioEvent GetHitAudioEvent(IDamageReceiver hitObject)
		{
			ICharacter player = hitObject as ICharacter;
			if (hitObject == null || player == null)
				return AudioEvent.ImpactWall;

			return player.isCurrentPlayer ? AudioEvent.ImpactCurrentPlayer : AudioEvent.ImpactOtherPlayer;
		}
	}
}
