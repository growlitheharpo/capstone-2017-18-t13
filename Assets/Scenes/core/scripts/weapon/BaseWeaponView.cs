using System.Collections;
using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using KeatsLib.Unity;
using UnityEngine;
using Logger = FiringSquad.Debug.Logger;

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
		[SerializeField] private int mPlayerRotationSamples = 30;
		[SerializeField] private float mCameraRotationFollowFactor = 10.0f;

		/// Private variables
		private BaseWeaponScript mWeaponScript;
		private Dictionary<Attachment, Transform> mAttachPoints;
		private ParticleSystem mShotParticles, mPartBreakPrefab;
		private Animator mAnimator;
		private Queue<Quaternion> mRecentPlayerRotations;

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

			mRecentPlayerRotations = new Queue<Quaternion>();
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

			UpdateRecentRotations(bearer);
			HandleWeaponInertia(bearer);
		}

		private void UpdateRecentRotations(IWeaponBearer bearer)
		{
			while (mRecentPlayerRotations.Count > mPlayerRotationSamples)
				mRecentPlayerRotations.Dequeue();

			mRecentPlayerRotations.Enqueue(bearer.eye.rotation);
		}

		private void HandleWeaponInertia(IWeaponBearer bearer)
		{
			Quaternion avgRot = CalculateAverageRotation();

			float avgAngle;
			Vector3 avgAxis;
			avgRot.ToAngleAxis(out avgAngle, out avgAxis);
			Logger.Info("Avg player rotation: " + avgAxis + " @ " + avgAngle);

			Quaternion targetRot = bearer.eye.rotation;

			targetRot = targetRot * avgRot;

			transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, mCameraRotationFollowFactor);
		}

		private Quaternion CalculateAverageRotation()
		{
			var recentRotations = mRecentPlayerRotations.ToArray();

			float angle = 0.0f;
			Vector3 axis = Vector3.zero;
			for (int i = 0; i < recentRotations.Length - 1; ++i)
			{
				Quaternion a = recentRotations[i], b = recentRotations[i + 1];
				Quaternion diff = b * Quaternion.Inverse(a);

				float thisAngle;
				Vector3 thisAxis;
				diff.ToAngleAxis(out thisAngle, out thisAxis);

				angle += thisAngle;
				axis += thisAxis;
			}

			angle /= recentRotations.Length - 1;
			axis /= recentRotations.Length - 1;
			return Quaternion.AngleAxis(angle, axis);
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
