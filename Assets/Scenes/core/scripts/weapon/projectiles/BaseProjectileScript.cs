using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	public abstract class BaseProjectileScript : NetworkBehaviour, IProjectile
	{
		[SerializeField] private AudioProfile mAudioProfile;

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

		// TODO: Add audio calls here
		protected void PlaySound(AudioManager.AudioEvent e)
		{
			if (!isClient)
				return;

			ServiceLocator.Get<IAudioManager>()
				.PlaySound(e, mAudioProfile, transform);
		}

		protected void PlaySound(AudioManager.AudioEvent e, Vector3 position)
		{
			if (!isClient)
				return;

			ServiceLocator.Get<IAudioManager>()
				.PlaySound(e, mAudioProfile, transform, position);
		}

		protected AudioManager.AudioEvent GetHitAudioEvent(IDamageReceiver hitObject)
		{
			ICharacter player = hitObject as ICharacter;
			if (hitObject == null || player == null)
				return AudioManager.AudioEvent.ImpactWall;

			return player.isCurrentPlayer ? AudioManager.AudioEvent.ImpactCurrentPlayer : AudioManager.AudioEvent.ImpactOtherPlayer;
		}
	}
}
