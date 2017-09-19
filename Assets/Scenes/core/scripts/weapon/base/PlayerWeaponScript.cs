using System;
using System.Collections;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Component strictly for handling the player's weapon
	/// and how it behaves.
	/// </summary>
	/// <inheritdoc />
	public class PlayerWeaponScript : BaseWeaponScript
	{
		[SerializeField] private ParticleSystem mShotParticles;
		private Vector3 mPlayerEyeOffset;
		private Animator mAnimator;

		private const float CAMERA_FOLLOW_FACTOR = 10.0f;

		protected override void Awake()
		{
			base.Awake();
			mAnimator = GetComponent<Animator>();

			mClipSize = new BoundProperty<int>(0, GameplayUIManager.CLIP_TOTAL);
			mAmountInClip = new BoundProperty<int>(0, GameplayUIManager.CLIP_CURRENT);
		}

		private void Start()
		{
			mAimRoot = Camera.main.transform;
			mPlayerEyeOffset = mAimRoot.InverseTransformPoint(transform.position);
			EventManager.OnConfirmPartAttach += AttachNewPart;
		}

		private void OnDestroy()
		{
			EventManager.OnConfirmPartAttach -= AttachNewPart;

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

		protected override void OnPreFireShot()
		{
			if (ServiceLocator.Get<IGamestateManager>().IsFeatureEnabled(GamestateManager.Feature.WeaponDurability))
				DegradeWeapon();
		}

		private void DegradeWeapon()
		{
			UnityEngine.Debug.Log("BREAK EVERYTHING!!!");
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
		
		protected override void Update()
		{
			base.Update();
			FollowCamera();
		}

		/// <summary>
		/// Lerp our position and rotation to match the camera.
		/// </summary>
		private void FollowCamera()
		{
			Vector3 location = transform.position;
			Vector3 targetLocation = bearer.eye.TransformPoint(mPlayerEyeOffset);

			Quaternion rotation = transform.rotation;
			Quaternion targetRotation = bearer.eye.rotation;

			transform.position = Vector3.Lerp(location, targetLocation, Time.deltaTime * CAMERA_FOLLOW_FACTOR);
			transform.rotation = Quaternion.Lerp(rotation, targetRotation, Time.deltaTime * CAMERA_FOLLOW_FACTOR);
		}
	}
}
