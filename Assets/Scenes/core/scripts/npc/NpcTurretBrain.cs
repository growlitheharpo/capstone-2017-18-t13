using System.Linq;
using UnityEngine;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Gameplay.NPC
{
	public class NpcTurretBrain
	{
		private ICharacter[] mPotentialTargets;
		private NpcTurret mTurret;

		private ICharacter mCurrentTarget;
		private float mTriggerHoldTimer, mTriggerUpTimer;

		public NpcTurretBrain(NpcTurret turret)
		{
			mPotentialTargets = new ICharacter[0];
			mTurret = turret;
			mTriggerHoldTimer = 0.0f;
			mTriggerUpTimer = 0.0f;
		}

		public void UpdateTargetList(ICharacter[] targets)
		{
			mPotentialTargets = targets;

			if (mCurrentTarget != null && !mPotentialTargets.Any(x => ReferenceEquals(x, mCurrentTarget)))
				ClearTarget();
		}

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

		private void ValidateCurrentTarget()
		{
			if (mCurrentTarget == null)
				return;

			if (!IsTargetValid(mCurrentTarget))
				ClearTarget();
		}

		private void ClearTarget()
		{
			mCurrentTarget = null;
		}

		private void TrackTarget()
		{
			Vector3 dirToTarget = (mCurrentTarget.transform.position + Vector3.up - mTurret.transform.position).normalized;
			Quaternion goalRot = Quaternion.LookRotation(dirToTarget, Vector3.up);

			mTurret.transform.rotation = Quaternion.Slerp(mTurret.transform.rotation, goalRot, Time.deltaTime * mTurret.data.targetingSpeed);
		}
		
		private void ManageTrigger()
		{
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

		private void FindTarget()
		{
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

		private bool IsTargetValid(ICharacter target)
		{
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
			if (!Physics.Raycast(ray, out hitInfo, mTurret.data.targetingRange + 1000.0f, mTurret.data.visibilityMask))
				return true;

			UnityEngine.Debug.DrawLine(ray.origin, hitInfo.point, Color.red);

			IDamageReceiver damageReceiver = hitInfo.GetDamageReceiver();
			return ReferenceEquals(damageReceiver, target);
		}
	}
}
