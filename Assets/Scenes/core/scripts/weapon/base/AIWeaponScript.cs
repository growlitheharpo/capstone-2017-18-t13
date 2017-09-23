using System.Collections;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class AIWeaponScript : BaseWeaponScript
	{
		[SerializeField] private ParticleSystem mShotParticles;
		private Animator mAnimator;

		protected override void Awake()
		{
			base.Awake();
			mAnimator = GetComponent<Animator>();

			mClipSize = new BoundProperty<int>(0);
			mAmountInClip = new BoundProperty<int>(0);
		}

		private void OnDestroy()
		{
			mClipSize.Cleanup();
			mAmountInClip.Cleanup();
		}

		/// <summary>
		/// Play any SFX and VFX associated with the weapon based on its current mods.
		/// </summary>
		protected override void PlayShotEffect()
		{
			mShotParticles.Stop();
			mShotParticles.time = 0.0f;
			mShotParticles.Play();
		}

		/// <summary>
		/// Play any SFX, VFX, and Animations for reloading.
		/// </summary>
		protected override void PlayReloadEffect(float time)
		{
			AnimationUtility.PlayAnimation(mAnimator, "reload");
			StartCoroutine(WaitForReload(time));
		}

		private IEnumerator WaitForReload(float time)
		{
			yield return null;
			mAnimator.speed = 1.0f / time;
			yield return new WaitForAnimation(mAnimator);
			mAnimator.speed = 1.0f;
			OnReloadComplete();
		}

		/// <summary>
		/// Set the override eye to a particular transform.
		/// </summary>
		public void SetAimRoot(Transform eye)
		{
			mAimRoot = eye;
		}
	}
}
