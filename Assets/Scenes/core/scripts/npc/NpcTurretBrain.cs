using System.Linq;
using UnityEngine;

namespace FiringSquad.Gameplay.NPC
{
	public class NpcTurretBrain
	{
		private ICharacter[] mPotentialTargets;
		private NpcTurret mTurret;

		private ICharacter mCurrentTarget;

		public NpcTurretBrain(NpcTurret turret)
		{
			mPotentialTargets = new ICharacter[0];
			mTurret = turret;
		}

		public void UpdateTargetList(ICharacter[] targets)
		{
			mPotentialTargets = targets;

			if (mCurrentTarget != null && !mPotentialTargets.Any(x => ReferenceEquals(x, mCurrentTarget)))
				mCurrentTarget = null;
		}

		public void Think()
		{
			ValidateCurrentTarget();
			if (mCurrentTarget != null)
				TrackTarget();
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
			mTurret.transform.LookAt(mCurrentTarget.transform);
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
		}

		private bool IsTargetValid(ICharacter target)
		{
			Vector3 targetPos = target.transform.position;
			Vector3 ourPos = mTurret.transform.position;
			Vector3 ourForward = mTurret.eye.forward;
			Vector3 targetDir = targetPos - ourPos;
			Ray ray = new Ray(mTurret.weapon.currentParts.barrel.barrelTip.position, targetDir.normalized);

			if (Vector3.Distance(targetPos, ourPos) >= mTurret.data.targetingRange)
				return false;
			if (Vector3.Dot(ray.direction, ourForward) < mTurret.data.targetingCone)
				return false;

			RaycastHit hitInfo;
			if (!Physics.Raycast(ray, out hitInfo))
				return true;

			IDamageReceiver damageReceiver = hitInfo.GetDamageReceiver();
			return ReferenceEquals(damageReceiver, target);
		}
	}
}
