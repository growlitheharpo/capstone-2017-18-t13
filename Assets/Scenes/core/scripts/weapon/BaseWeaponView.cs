using System.Collections;
using System.Collections.Generic;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Data;
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
		[SerializeField] private WeaponMovementData mGeneralMovementData;
		[SerializeField] private WeaponMovementData mAimDownSightsMovementData;

		/// Private variables
		private IWeapon mWeaponScript;

		private Dictionary<Attachment, Transform> mAttachPoints;
		private Animator mAnimator, mArmAnimator;

		private ParticleSystem mPartBreakPrefab, mCurrentMuzzleFlashVfx;

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
			mWeaponScript = GetComponent<IWeapon>();
			mPartBreakPrefab = Resources.Load<GameObject>("prefabs/weapons/effects/p_vfx_partBreak").GetComponent<ParticleSystem>();

			mAttachPoints = new Dictionary<Attachment, Transform>
			{
				{ Attachment.Scope, mScopeAttach },
				{ Attachment.Barrel, mBarrelAttach },
				{ Attachment.Mechanism, mMechanismAttach },
				{ Attachment.Grip, mGripAttach }
			};

			// Initialize the queues with enough storage to hold the max they should need to. This will help avoid allocations at runtime.
			mRecentPlayerPositions = new Queue<Vector3>(Mathf.Max(mGeneralMovementData.playerPositionSamples, mAimDownSightsMovementData.playerPositionSamples) + 1);
			mRecentPlayerRotations = new Queue<Quaternion>(Mathf.Max(mGeneralMovementData.playerRotationSamples, mAimDownSightsMovementData.playerRotationSamples) + 1);
			mCurrentMovementData = mGeneralMovementData;
		}

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			if (ObjectHighlight.instance == null)
				return;

			var renderers = GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers)
				ObjectHighlight.instance.AddOccluder(r);
		}

		/// <summary>
		/// Unity's LateUpdate function. Using to lerp gun rotation and follow the player's eye position.
		/// </summary>
		private void LateUpdate()
		{
			IWeaponBearer bearer = mWeaponScript.bearer;
			if (bearer == null || bearer.eye == null)
				return;

			if (mArmAnimator != null)
				mArmAnimator.SetBool("AimDownSightsActive", mWeaponScript.aimDownSightsActive);

			mCurrentMovementData = mWeaponScript.aimDownSightsActive ? mAimDownSightsMovementData : mGeneralMovementData;

			AccumulateRecentPositions(bearer);
			AccumulateRecentRotations(bearer);

			HandleWeaponMovement(bearer);
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
		/// <param name="bearer">The bearer of this weapon.</param>
		private void HandleWeaponMovement(ICharacter bearer)
		{
			Vector3 location = transform.localPosition;

			// Target location is our offset. Transform it into a local position within our parent.
			Vector3 targetLoc = mWeaponScript.bearer.eye.TransformPoint(mWeaponScript.positionOffset);
			targetLoc = transform.parent.InverseTransformPoint(targetLoc);

			// Add the weapon bob (based on player velocity) to the local player only.
			if (bearer.isCurrentPlayer)
				targetLoc += CalculateWeaponBobOffset();

			// Lerp to that position smoothly
			transform.localPosition = Vector3.Lerp(location, targetLoc, Time.deltaTime * mCurrentMovementData.cameraMovementFollowFactor);
		}

		/// <summary>
		/// Return a Vector3 adjustment to the weapon's local position based on the player's movement speed.
		/// </summary>
		/// <returns></returns>
		private Vector3 CalculateWeaponBobOffset()
		{
			Vector3 playerVelocity = CalculatePlayerVelocity();
			float speed = playerVelocity.magnitude;

			// If the player is (basically) stopped, reset our progress and don't apply any bob.
			if (speed < 0.01f)
			{
				mWeaponBobProgress = 0.0f;
				return Vector3.zero;
			}

			// Calculate our progress ("circular" from a sine wave. Provides automatic smoothing and is "fast enough")
			float param = mWeaponBobProgress * Mathf.PI * mCurrentMovementData.playerWeaponBobFrequency;
			float progress = Mathf.Sin(param) * 0.5f + 0.5f;

			// Increment our time and speed-based bob progress.
			mWeaponBobProgress += Time.deltaTime * speed;

			// Get the positions based on the input variables.
			Vector3 maxBobPosition = Vector3.forward * mCurrentMovementData.playerWeaponBobZAmount
									+ Vector3.left * mCurrentMovementData.playerWeaponBobXAmount
									+ Vector3.up * mCurrentMovementData.playerWeaponBobYAmount;
			Vector3 minBobPosition = maxBobPosition * -1.0f;

			// Lerp between them based on our progress.
			return Vector3.Lerp(minBobPosition, maxBobPosition, progress);
		}

		/// <summary>
		/// Estimate the player's recent velocity based on the number of samples provided.
		/// </summary>
		private Vector3 CalculatePlayerVelocity()
		{
			Vector3 result = Vector3.zero;
			var positions = mRecentPlayerPositions.ToArray();
			if (positions.Length < 2)
				return result;

			// Average the change in the player's position over the last few frames.
			// This is (roughly) the player's velocity.
			for (int i = 0; i < positions.Length - 1; ++i)
				result += (positions[i + 1] - positions[i]);

			return result / (positions.Length - 1);
		}

		/// <summary>
		/// Rotate the weapon to follow the player's looking direction and the desired weapon inertia using the player's
		/// rotational velocity.
		/// </summary>
		/// <param name="bearer">The bearer of this weapon.</param>
		private void HandleWeaponInertia(IWeaponBearer bearer)
		{
			Quaternion targetRot = bearer.eye.rotation;

			// Apply extra inertia to the current player's weapon based on their rotational velocity.
			if (bearer.isCurrentPlayer)
				targetRot = targetRot * CalculateAverageRotationalVelocity();

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
			{
				if (current.attachPoint == Attachment.Scope && mWeaponScript.aimDownSightsActive)
					((WeaponPartScriptScope)current).DeactivateAimDownSightsEffect(mWeaponScript, true);

				Destroy(current.gameObject);
			}

			newPart.transform.SetParent(mAttachPoints[place]);
			newPart.transform.ResetLocalValues();

			if (ObjectHighlight.instance != null && mWeaponScript.bearer != null && mWeaponScript.bearer.isCurrentPlayer)
			{
				var renderers = newPart.GetComponentsInChildren<Renderer>();
				foreach (Renderer r in renderers)
					ObjectHighlight.instance.AddOccluder(r);
			}

			// Spawn the muzzle flash if this is the barrel.
			if (newPart.attachPoint == Attachment.Barrel)
			{
				WeaponPartScriptBarrel realScript = (WeaponPartScriptBarrel)newPart;
				GameObject prefab = realScript.muzzleFlashVfxPrefab;

				if (prefab == null)
					return;

				GameObject instance = Instantiate(prefab, realScript.barrelTip, false);
				mCurrentMuzzleFlashVfx = instance.GetComponent<ParticleSystem>();
				mCurrentMuzzleFlashVfx.transform.ResetLocalValues();
			}
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
			UpdateArmRecoilAnimation(true, true);
			PlayWeaponFireAnimations();

			mWeaponScript.bearer.PlayFireAnimation();
			
			if (mCurrentMuzzleFlashVfx != null)
				mCurrentMuzzleFlashVfx .Play();

			IAudioReference effect = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.Shoot, transform, false);
			if (mWeaponScript.currentParts.mechanism != null)
				effect.weaponType = mWeaponScript.currentParts.mechanism.audioOverrideWeaponType;
			effect.Start();

			effect = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.BarrelLayer, transform, false);
			if (mWeaponScript.currentParts.barrel != null)
				effect.barrelType = mWeaponScript.currentParts.barrel.audioOverrideBarrelType;
			effect.Start();
		}

		/// <summary>
		/// Update the "IsFiring" state of our animator based on the current data.
		/// </summary>
		public void UpdateArmRecoilAnimation(bool isFiring, bool fireNow)
		{
			if (mArmAnimator == null)
				return;
			
			if (mWeaponScript.currentParts.barrel != null)
				mArmAnimator.SetBool("WeaponIsAuto", mWeaponScript.currentData.fireRate >= 3.5f);
			mArmAnimator.SetFloat("RecoilAmount", mWeaponScript.currentData.recoilAmount);
			mArmAnimator.SetFloat("FireRate", mWeaponScript.currentData.fireRate * 1.1f);

			mArmAnimator.SetBool("IsFiring", isFiring);
			if (fireNow)
				mArmAnimator.SetTrigger("Fire");
		}

		private void PlayWeaponFireAnimations()
		{
			if (mWeaponScript.currentParts.mechanism != null)
			{
				Animator anim = mWeaponScript.currentParts.mechanism.attachedAnimator;
				if (anim != null)
				{
					anim.SetFloat("FireRate", mWeaponScript.currentData.fireRate);
					anim.SetTrigger("Fire");
				}
			}

			if (mWeaponScript.currentParts.barrel != null)
			{
				Animator anim = mWeaponScript.currentParts.barrel.attachedAnimator;
				if (anim != null)
				{
					anim.SetFloat("FireRate", mWeaponScript.currentData.fireRate);
					anim.SetTrigger("Fire");
				}
			}
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

		/// <summary>
		/// Update the animator that we will update "ADS" status on.
		/// </summary>
		/// <param name="armAnimator">The animator to use.</param>
		public void SetArmAnimator(Animator armAnimator)
		{
			mArmAnimator = armAnimator;
		}
	}
}
