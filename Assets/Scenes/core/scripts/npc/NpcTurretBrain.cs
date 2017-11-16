using System.Linq;
using UnityEngine;

namespace FiringSquad.Gameplay.NPC
{
	/// <summary>
	/// The class that handles the actual "thinking" for the NPC turrets and forwards
	/// this on to the NpcTurret component.
	/// </summary>
	/// <seealso cref="NpcTurret"/>
	public class NpcTurretBrain
	{
		/// Private variables
		private readonly NpcTurret mTurret;
		private ICharacter[] mPotentialTargets;
		private ICharacter mCurrentTarget;
		private float mTriggerHoldTimer, mTriggerUpTimer;

		/// <summary>
		/// The class that handles the actual "thinking" for the NPC turrets and forwards
		/// this on to the NpcTurret component.
		/// </summary>
		public NpcTurretBrain(NpcTurret turret)
		{
			mPotentialTargets = new ICharacter[0];
			mTurret = turret;
			mTriggerHoldTimer = 0.0f;
			mTriggerUpTimer = 0.0f;
		}

		/// <summary>
		/// Update the potential list of targets for this AI turret.
		/// Resets our current target if they are no longer in the list.
		/// </summary>
		/// <param name="targets">The list of characters that we can target.</param>
		public void UpdateTargetList(ICharacter[] targets)
		{
			mPotentialTargets = targets;

			if (mCurrentTarget != null && !mPotentialTargets.Any(x => ReferenceEquals(x, mCurrentTarget)))
				ClearTarget();
		}

		/// <summary>
		/// NPC update/tick function. Once-per-frame when resources allow.
		/// </summary>
		public void Think()
		{
			ValidateCurrentTarget();
			if (mCurrentTarget != null)
			{
				TrackTarget();
				ManageTrigger();
			}
			else
				FindTarget();
		}

		/// <summary>
		/// Ensure our current target is visible and within range, and clear it otherwise.
		/// </summary>
		private void ValidateCurrentTarget()
		{
			if (mCurrentTarget == null)
				return;

			if (!IsTargetValid(mCurrentTarget))
				ClearTarget();
		}

		/// <summary>
		/// Set the turret's target to null.
		/// </summary>
		private void ClearTarget()
		{
			mCurrentTarget = null;
		}

		/// <summary>
		/// Rotate slowly towards our target.
		/// </summary>
		private void TrackTarget()
		{
			Vector3 dirToTarget = (mCurrentTarget.transform.position + Vector3.up - mTurret.transform.position).normalized;
			Quaternion goalRot = Quaternion.LookRotation(dirToTarget, Vector3.up);

			mTurret.transform.rotation = Quaternion.Slerp(mTurret.transform.rotation, goalRot, Time.deltaTime * mTurret.data.targetingSpeed);
		}
		
		/// <summary>
		/// Handle "pulling the trigger" of the turret's weapon with a built-in cooldown to avoid aim-botting.
		/// </summary>
		private void ManageTrigger()
		{
			if (mTurret.weapon == null)
				return;

			if (mTriggerHoldTimer <= mTurret.data.weaponHoldTime)
			{
				mTriggerHoldTimer += Time.deltaTime;
				mTurret.weapon.FireWeaponHold();
			}
			else
			{
				mTurret.weapon.FireWeaponUp();
				mTriggerUpTimer += Time.deltaTime;

				if (!(mTriggerUpTimer >= mTurret.data.weaponUpTime))
					return;

				mTriggerHoldTimer = 0.0f;
				mTriggerUpTimer = 0.0f;
			}
		}

		/// <summary>
		/// Locates a target. Prioritizes closer targets, and only selects targets that are visible.
		/// </summary>
		private void FindTarget()
		{
			// TODO: The square distance is cheaper than checking if the target is valid because of raycasting.
			// It might be more efficient to sort the potential targets by position first, THEN eliminate
			// based on whether or not they are valid.

			ICharacter closesetTarget = null;
			float sqrClosestDistance = float.MaxValue;

			foreach (ICharacter c in mPotentialTargets)
			{
				if (!IsTargetValid(c))
					continue;

				float sqrDistance = (c.transform.position - mTurret.transform.position).sqrMagnitude;
				if (!(sqrDistance <= sqrClosestDistance))
					continue;

				closesetTarget = c;
				sqrClosestDistance = sqrDistance;
			}

			mCurrentTarget = closesetTarget;
			mTriggerHoldTimer = 0.0f;
			mTriggerUpTimer = 0.0f;
		}

		/// <summary>
		/// Check if the provided target is valid and can be targeted.
		/// </summary>
		/// <para>
		/// Requirements to be valid:
		///		1. Must be within our targeting range defined by the turret's data.
		///		2. Must be within our targeting cone defined by the turret's data.
		///		3. Must be visible after a physics raycast.
		/// </para>
		/// <param name="target">The character to check.</param>
		/// <returns>True if the target meets all criteria and is valid.</returns>
		private bool IsTargetValid(ICharacter target)
		{
			if (target == null)
				return false;

			Vector3 targetPos = target.transform.position;
			Vector3 ourPos = mTurret.transform.position;
			Vector3 ourForward = mTurret.eye.forward;
			Vector3 targetDir = targetPos - ourPos;
			Ray ray = new Ray(ourPos + Vector3.up, targetDir.normalized);

			float distance = Vector3.Distance(targetPos, ourPos);
			float dot = Vector3.Dot(ray.direction, ourForward);

			if (distance >= mTurret.data.targetingRange)
				return false;
			if (dot < mTurret.data.targetingCone)
				return false;

			RaycastHit hitInfo;
			if (!Physics.Raycast(ray, out hitInfo, mTurret.data.targetingRange + 1000.0f, mTurret.data.visibilityMask, QueryTriggerInteraction.Ignore))
				return true;

			UnityEngine.Debug.DrawLine(ray.origin, hitInfo.point, Color.red);

			IDamageReceiver damageReceiver = hitInfo.GetDamageReceiver();
			return ReferenceEquals(damageReceiver, target);
		}
	}
}
