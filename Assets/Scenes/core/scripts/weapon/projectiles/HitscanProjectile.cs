using System.Collections;
using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class HitscanProjectile : MonoBehaviour, IProjectile
	{
		public void PreSetup() {}
		public void PostSetup() {}
		public void PreDisable() {}
		public void PostDisable() {}

		[SerializeField] private AnimationCurve mFalloffCurve;
		[SerializeField] private AudioProfile mProfile;

		private HitscanShootEffect mEffect;
		private IAudioReference mAudio;

		private void Awake()
		{
			mEffect = GetComponent<HitscanShootEffect>();
		}

		public ICharacter source { get { return sourceWeapon.bearer; } }
		public IWeapon sourceWeapon { get; private set; }

		public void Instantiate(IWeapon weapon, Ray ray, WeaponData data)
		{
			HandleShot(weapon, ray, data);
		}

		public void Instantiate(IWeapon weapon, Ray ray, WeaponData data, GameObjectPool pool)
		{
			HandleShot(weapon, ray, data, pool);
		}

		private void HandleShot(IWeapon weapon, Ray ray, WeaponData data, GameObjectPool pool = null)
		{
			SetupShot(weapon);

			UnityEngine.Debug.DrawLine(ray.origin, ray.origin + ray.direction * 2000.0f, Color.red, 1.0f / data.fireRate + 0.2f);

			// See if we hit anything
			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit))
			{
				KillSelf(pool);
				return;
			}

			AudioManager.AudioEvent eventToPlay;

			// Try to apply damage to it if we did
			IDamageReceiver component = hit.GetDamageReceiver();
			if (component != null)
			{
				float damage = GetDamage(data, Vector3.Distance(transform.position, hit.point));
				component.ApplyDamage(damage, hit.point, hit.normal, this);

				if (component is PlayerScript && ((PlayerScript)component).isCurrentPlayer)
					eventToPlay = AudioManager.AudioEvent.ImpactCurrentPlayer;
				else
					eventToPlay = AudioManager.AudioEvent.ImpactOtherPlayer;
			}
			else
				eventToPlay = AudioManager.AudioEvent.ImpactWall;

			mAudio = ServiceLocator.Get<IAudioManager>().PlaySound(eventToPlay, mProfile, transform, transform.InverseTransformPoint(hit.point));
			StartCoroutine(PlayEffectAndKillSelf(pool, hit.point));
		}

		private float GetDamage(WeaponData data, float distance)
		{
			float distancePercent = Mathf.Clamp(distance / data.damageFalloffDistance, 0.0f, 1.0f);
			return mFalloffCurve.Evaluate(distancePercent) * data.damage;
		}

		private void SetupShot(IWeapon weapon)
		{
			sourceWeapon = weapon;

			transform.position = weapon.transform.position;
			transform.forward = weapon.transform.forward;
		}

		private IEnumerator PlayEffectAndKillSelf(GameObjectPool pool, Vector3 hitPoint)
		{
			yield return mEffect.Flash(hitPoint);
			yield return new WaitForAudio(mAudio);
			KillSelf(pool);
		}

		private void KillSelf(GameObjectPool pool)
		{
			if (ServiceLocator.Get<IAudioManager>().CheckReferenceAlive(ref mAudio) != null)
				mAudio.Kill();

			if (pool != null)
				pool.ReturnItem(gameObject);
			else
				Destroy(gameObject);
		}
	}
}
