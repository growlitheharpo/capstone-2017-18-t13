using System.Collections;
using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	public class BaseWeaponView : MonoBehaviour
	{
		///Inspector variables
		[SerializeField] private Transform mBarrelAttach;
		[SerializeField] private Transform mScopeAttach;
		[SerializeField] private Transform mMechanismAttach;
		[SerializeField] private Transform mGripAttach;
		[SerializeField] private float mCameraMovementFollowFactor = 10.0f;
		[SerializeField] private float mCameraRotationFollowFactor = 10.0f;

		/// Private variables
		private BaseWeaponScript mWeaponScript;
		private Dictionary<Attachment, Transform> mAttachPoints;
		private ParticleSystem mShotParticles, mPartBreakPrefab;
		private Quaternion mPreviousRotation;
		private Animator mAnimator;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mAnimator = GetComponent<Animator>();
			mWeaponScript = GetComponent<BaseWeaponScript>();
			mShotParticles = transform.Find("shot_particles").GetComponent<ParticleSystem>();
			mPartBreakPrefab = Resources.Load<GameObject>("prefabs/weapons/effects/p_vfx_partBreak").GetComponent<ParticleSystem>();

			mAttachPoints = new Dictionary<Attachment, Transform>
			{
				{ Attachment.Scope, mScopeAttach },
				{ Attachment.Barrel, mBarrelAttach },
				{ Attachment.Mechanism, mMechanismAttach },
				{ Attachment.Grip, mGripAttach }
			};
		}

		/// <summary>
		/// Unity's Update function
		/// Used to set our position to follow the player
		/// </summary>
		private void Update()
		{
			//Follow my player
			if (mWeaponScript.bearer == null || mWeaponScript.bearer.eye == null)
				return;

			Vector3 location = transform.position;
			Vector3 targetLoc = mWeaponScript.bearer.eye.TransformPoint(mWeaponScript.positionOffset);
			transform.position = Vector3.Lerp(location, targetLoc, Time.deltaTime * mCameraMovementFollowFactor);
		}

		/// <summary>
		/// Unity's LateUpdate function. Using to lerp gun rotation
		/// </summary>
		private void LateUpdate()
		{
			IWeaponBearer bearer = mWeaponScript.bearer;
			if (bearer == null)
				return;

			// TODO: The weird "snapping" we're seeing is likely a result of using Euler angles instead of working directly with quaternions!

			transform.rotation = mPreviousRotation;
			Quaternion targetRot = Quaternion.Euler(bearer.eye.rotation.eulerAngles.x, bearer.transform.rotation.eulerAngles.y, bearer.transform.rotation.z);

			transform.rotation = Quaternion.Slerp(mPreviousRotation, targetRot, Time.deltaTime * mCameraRotationFollowFactor);

			// Compare rotations to snap if close enough
			if (Quaternion.Angle(transform.rotation, targetRot) <= 0.05) transform.rotation = targetRot;

			// If the rotations are too far apart, clamp to 20 degrees
			if (Quaternion.Angle(transform.rotation, targetRot) >= 20)
			{
				// if the current rotation is greater...
				if (transform.rotation.eulerAngles.y > targetRot.eulerAngles.y)
					transform.rotation = Quaternion.Euler(targetRot.eulerAngles.x, targetRot.eulerAngles.y + 20, targetRot.eulerAngles.z);
				// else if the target rotation is greater
				else if (transform.rotation.eulerAngles.y < targetRot.eulerAngles.y)
					transform.rotation = Quaternion.Euler(targetRot.eulerAngles.x, targetRot.eulerAngles.y - 20, targetRot.eulerAngles.z);
			}

			mPreviousRotation = transform.rotation;
		}

		#region Part Attachment

		/// <summary>
		/// Move this weapon instance to the position on the gun and destroy the existing one if necessary.
		/// </summary>
		/// <param name="newPart">The weapon part to move.</param>
		public void MoveAttachmentToPoint(WeaponPartScript newPart)
		{
			Attachment place = newPart.attachPoint;

			WeaponPartScript current = mWeaponScript.currentParts[place];
			if (current != null)
				Destroy(current.gameObject);

			newPart.transform.SetParent(mAttachPoints[place]);
			newPart.transform.ResetLocalValues();
		}

		#endregion


		#region Reloading

		/// <summary>
		/// Play an effect to show the player that they are reloading.
		/// </summary>
		/// <param name="time">The amount of time it takes to reload.</param>
		public void PlayReloadEffect(float time)
		{
			IAudioReference effect = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.Reload, transform, false);
			if (mWeaponScript.currentParts.mechanism != null)
				effect.weaponType = mWeaponScript.currentParts.mechanism.audioOverrideWeaponType;
			effect.AttachToRigidbody(mWeaponScript.bearer.gameObject.GetComponent<Rigidbody>()); // TODO: Cache this??
			effect.Start();

			AnimationUtility.PlayAnimation(mAnimator, "reload");
			StartCoroutine(WaitForReload(time));
		}

		/// <summary>
		/// Wait until the reload is finished, then reset our animator.
		/// TODO: This requires doing some hacky stuff to the weapon's animator!!
		/// </summary>
		/// <param name="time">The amount of time it takes to reload.</param>
		private IEnumerator WaitForReload(float time)
		{
			yield return null;
			yield return null;
			mAnimator.speed = 1.0f / time;
			yield return new WaitForAnimation(mAnimator);
			mAnimator.speed = 1.0f;
		}

		#endregion

		#region Shooting/Firing

		/// <summary>
		/// Activate the 'shoot' effect for this weapon.
		/// Includes audio and visuals.
		/// </summary>
		public void PlayFireEffect()
		{
			mWeaponScript.bearer.PlayFireAnimation();
			if (mWeaponScript.currentParts.barrel != null)
				mShotParticles.transform.position = mWeaponScript.currentParts.barrel.barrelTip.position;

			mShotParticles.Play();

			IAudioReference effect = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.Shoot, transform, false);
			if (mWeaponScript.currentParts.mechanism != null)
				effect.weaponType = mWeaponScript.currentParts.mechanism.audioOverrideWeaponType;

			effect.Start();
		}

		#endregion

		#region Durability

		/// <summary>
		/// Activate the part break effect on this weapon.
		/// </summary>
		public void CreateBreakPartEffect()
		{
			ParticleSystem instance = Instantiate(mPartBreakPrefab.gameObject).GetComponent<ParticleSystem>();
			instance.transform.SetParent(transform);
			instance.transform.ResetLocalValues();
			instance.Play();

			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(instance));
		}

		#endregion
	}
}
