using System.Collections;
using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Data;
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
		[SerializeField] private WeaponMovementData mGeneralMovementData;
		[SerializeField] private WeaponMovementData mAimDownSightsMovementData;

		/// Private variables
		private BaseWeaponScript mWeaponScript;

		private Dictionary<Attachment, Transform> mAttachPoints;
		private Animator mAnimator;

		private ParticleSystem mShotParticles, mPartBreakPrefab;

		private float mWeaponBobProgress;
		private Queue<Vector3> mRecentPlayerPositions;
		private Queue<Quaternion> mRecentPlayerRotations;
		private WeaponMovementData mCurrentMovementData;

		#region Unity Callbacks

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

			mRecentPlayerPositions = new Queue<Vector3>();
			mRecentPlayerRotations = new Queue<Quaternion>();
			mCurrentMovementData = mGeneralMovementData;
		}
		
		/// <summary>
		/// Unity's LateUpdate function. Using to lerp gun rotation and follow the player's eye position.
		/// </summary>
		private void LateUpdate()
		{
			IWeaponBearer bearer = mWeaponScript.bearer;
			if (bearer == null || bearer.eye == null)
				return;
			
			mCurrentMovementData = mWeaponScript.aimDownSightsActive ? mAimDownSightsMovementData : mGeneralMovementData;

			AccumulateRecentPositions(bearer);
			AccumulateRecentRotations(bearer);

			HandleWeaponMovement();
			HandleWeaponInertia(bearer);
		}

		#endregion

		#region Weapon Inertia

		/// <summary>
		/// Update our recent player position queue to match our desired sample count and include the latest position.
		/// </summary>
		/// <param name="bearer">The bearer of this weapon.</param>
		private void AccumulateRecentPositions(ICharacter bearer)
		{
			mRecentPlayerPositions.Enqueue(bearer.transform.position);

			while (mRecentPlayerPositions.Count > mCurrentMovementData.playerPositionSamples)
				mRecentPlayerPositions.Dequeue();
		}

		/// <summary>
		/// Update our recent player rotation queue to match our desired sample count and included the latest rotation.
		/// </summary>
		/// <param name="bearer">The bearer of this weapon.</param>
		private void AccumulateRecentRotations(ICharacter bearer)
		{
			mRecentPlayerRotations.Enqueue(bearer.eye.rotation);

			while (mRecentPlayerRotations.Count > mCurrentMovementData.playerRotationSamples)
				mRecentPlayerRotations.Dequeue();
		}

		/// <summary>
		/// Handle following the player's movement with the weapon.
		/// </summary>
		private void HandleWeaponMovement()
		{
			Vector3 location = transform.localPosition;
			Vector3 targetLoc = mWeaponScript.bearer.eye.TransformPoint(mWeaponScript.positionOffset);
			targetLoc = transform.parent.InverseTransformPoint(targetLoc);

			transform.localPosition = Vector3.Lerp(location, targetLoc, Time.deltaTime * mCurrentMovementData.cameraMovementFollowFactor);
		}

		/// <summary>
		/// Rotate the weapon to follow the player's looking direction and the desired weapon inertia using the player's
		/// rotational velocity.
		/// </summary>
		/// <param name="bearer">The bearer of this weapon.</param>
		private void HandleWeaponInertia(IWeaponBearer bearer)
		{
			Quaternion bearerRotVelocity = CalculateAverageRotationalVelocity();
			Quaternion targetRot = bearer.eye.rotation;
			targetRot = targetRot * bearerRotVelocity;

			transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, mCurrentMovementData.cameraRotationFollowFactor * Time.deltaTime);
		}

		/// <summary>
		/// Calculate a weighted average of the change in the player's local rotations.
		/// Essentially gives a weighted rotational velocity in the form of a Quaternion.
		/// </summary>
		private Quaternion CalculateAverageRotationalVelocity()
		{
			var recentRotations = mRecentPlayerRotations.ToArray();
			float sampleWeightSum = CalculateTotalWeightCurveSum(recentRotations.Length);

			Quaternion accumulation = Quaternion.identity;
			for (int i = 0; i < recentRotations.Length - 1; ++i)
			{
				Quaternion a = recentRotations[i], b = recentRotations[i + 1];
				Quaternion diff = b * Quaternion.Inverse(a);

				float weight = CalculateRotationWeight(i, recentRotations.Length, sampleWeightSum);
				Quaternion weightedDiff = Quaternion.Slerp(Quaternion.identity, diff, weight);

				accumulation = accumulation * weightedDiff;
			}

			return accumulation;
		}

		/// <summary>
		/// Calculate the total sum (integration) of the curve we're using for weighting our velocity.
		/// </summary>
		private float CalculateTotalWeightCurveSum(int recentRotationsLength)
		{
			float sum = 0.0f;
			for (int i = 0; i < recentRotationsLength - 1; ++i)
				sum += mCurrentMovementData.sampleWeighting.Evaluate((float)i / (recentRotationsLength - 1));
			return sum;
		}

		/// <summary>
		/// Calculate the weight (0 - 1) of the rotation at this axis.
		/// </summary>
		/// <param name="i">The current index.</param>
		/// <param name="listLength">The length of the list.</param>
		/// <param name="weightSum">The total sum of the values under the weight curve.</param>
		private float CalculateRotationWeight(int i, int listLength, float weightSum)
		{
			float sample = mCurrentMovementData.sampleWeighting.Evaluate((float)i / (listLength - 1));
			sample /= weightSum;

			return float.IsNaN(sample) ? 0.0f : sample;
		}

		#endregion

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
