using System;
using System.Collections;
using KeatsLib.Unity;
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
		[SerializeField] private GameObject mPartBreakParticlesPrefab;
		[SerializeField] private ParticleSystem mShotParticles;
		private Vector3 mPlayerEyeOffset;
		private Animator mAnimator;

		private const float CAMERA_FOLLOW_FACTOR = 10.0f;

		protected override void Awake()
		{
			base.Awake();
			mAnimator = GetComponent<Animator>();

		}

		private void Start()
		{
			// TODO: GET RID OF THIS MESS
			var overrideName = (bearer as PlayerScript).overrideUIName;
			int val1 = string.IsNullOrEmpty(overrideName) ? GameplayUIManager.CLIP_TOTAL : (overrideName + "-cliptotal").GetHashCode();
			int val2 = string.IsNullOrEmpty(overrideName) ? GameplayUIManager.CLIP_CURRENT : (overrideName + "-clipcurrent").GetHashCode();
			mClipSize = new BoundProperty<int>(0, val1);
			mAmountInClip = new BoundProperty<int>(0, val2);

			mAimRoot = bearer.eye;
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
		protected override void PlayShotEffect(Vector3 origin)
		{
			ServiceLocator.Get<IAudioManager>().PlaySound(AudioManager.AudioEvent.PrimaryEffect1, mAudioProfile, transform);

			mShotParticles.Stop();
			mShotParticles.time = 0.0f;
			mShotParticles.transform.position = origin;
			mShotParticles.Play();
		}

		protected override void OnPreFireShot()
		{
			if (ServiceLocator.Get<IGamestateManager>().IsFeatureEnabled(GamestateManager.Feature.WeaponDurability))
				DegradeWeapon();
		}

		private void DegradeWeapon()
		{
			foreach (WeaponPartScript attachment in parts)
			{
				if (attachment.durability == WeaponPartScript.INFINITE_DURABILITY)
					continue;

				attachment.durability -= 1;
				if (attachment.durability == 0)
					BreakPart(attachment);
			}
		}

		private void BreakPart(WeaponPartScript part)
		{
			GameObject defaultPart = bearer.defaultParts[part.attachPoint];
			Instantiate(defaultPart)
				.GetComponent<WeaponPickupScript>()
				.OverrideDurability(WeaponPartScript.INFINITE_DURABILITY)
				.ConfirmAttach(this);

			ParticleSystem ps = Instantiate(mPartBreakParticlesPrefab, part.transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(ps));
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
