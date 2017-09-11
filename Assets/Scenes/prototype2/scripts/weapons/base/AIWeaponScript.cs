using System.Collections;
using Prototype2;
using UnityEngine;

public class AIWeaponScript : BaseWeaponScript
{
	[SerializeField] private ParticleSystem mShotParticles;
	private Animator mAnimator;
	
	protected override void Awake()
	{
		base.Awake();
		mAnimator = GetComponent<Animator>();
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
}
