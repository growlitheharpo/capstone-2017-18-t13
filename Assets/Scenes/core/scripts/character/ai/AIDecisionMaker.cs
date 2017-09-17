using System;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.AI;
using GameLogger = Logger;
using Random = UnityEngine.Random;

namespace FiringSquad.Gameplay.AI
{
	public class AIDecisionMaker
	{
		[Serializable]
		public class DecisionMakerVariables
		{
			public float mClosestDistanceToPlayer;
			public float mMaxDistanceFromPlayer;
			public float mLineOfSight;
			public float mRecentlyAttackedTimeThreshold;
			public float mBaseInaccuracy;
			public float mPurposefullyMissPlayerScale;
			public float mRotateSpeed;
			public AIHintValueData mHintData;

			public LayerMask mVisionLayermask;
		}

		private Action mCurrentState;
		private DecisionMakerVariables mVars;

		private AIHintingSystem mHintingSystemRef;
		private NavMeshPath mCurrentPath = null;
		private NavMeshAgent mMovementAgent;
		private IWeapon mWeapon;
		private Transform mEye;
		private Transform mPlayerRef;
		private float mLastAttackedTime;

		private Transform transform { get; set; }
		private bool wasRecentlyAttacked { get { return Time.time - mLastAttackedTime <= mVars.mRecentlyAttackedTimeThreshold; } }

		private Vector3 mMovementTarget;

		public AIDecisionMaker(DecisionMakerVariables vars, IWeapon weapon, Transform eye, NavMeshAgent agent)
		{
			mCurrentState = TickIdle;
			mMovementAgent = agent;
			mWeapon = weapon;
			mEye = eye;
			mVars = vars;

			mPlayerRef = ReferenceForwarder.get.player.transform;
			mHintingSystemRef = ReferenceForwarder.get.aiHintSystem;

			mLastAttackedTime = float.NegativeInfinity;

			transform = agent.transform;

			mMovementTarget = new Vector3(-10000, -10000, -10000);
		}

		public void Tick()
		{
			mCurrentState.Invoke();
		}

		#region State Functions

		private void TickIdle()
		{
			// just hang out. play a bark. scratch your head.


			if (KnowsOfPlayer())
				mCurrentState = TickIdle; //shoot instead!
		}

		private void TickPurposefullyMissPlayer()
		{
			// instruct our gun to shoot near the player, but not AT them.
			Vector3 dirToPlayer = mPlayerRef.position - transform.position;

			Vector3 leftDir = Vector3.Cross(dirToPlayer, transform.up).normalized;
			Vector3 rightDir = -leftDir;

			Vector3 closerDir = Vector3.Dot(leftDir, transform.forward) > Vector3.Dot(rightDir, transform.forward) ? leftDir : rightDir;
			Vector3 target = dirToPlayer + closerDir * mVars.mPurposefullyMissPlayerScale * Vector3.Distance(transform.position, mPlayerRef.position);

			UnityEngine.Debug.DrawLine(transform.position, transform.position + target, Color.HSVToRGB(0.8f, 0.6f, 0.55f));
			
			// TODO: Don't face towards the target, face the player!
			ShootInDirection(target, target);
		}

		private void TickShootAtPlayer()
		{
			// shoot at the player, with some level of inaccuracy.
			Vector3 dirToPlayer = mPlayerRef.position - transform.position;
			float spreadFactor = mVars.mBaseInaccuracy;

			Vector3 randomness = new Vector3(
				Random.Range(-spreadFactor, spreadFactor),
				Random.Range(-spreadFactor, spreadFactor),
				Random.Range(-spreadFactor, spreadFactor));

			Vector3 eyeGoal = dirToPlayer + randomness;
			ShootInDirection(dirToPlayer, eyeGoal);
		}

		private void TickChaseAfterPlayer()
		{
			//if (mMovementTarget == new Vector3(-10000, -10000, -10000) || Vector3.Distance(transform.position, mPlayerRef.position) > mVars.mMaxDistanceFromPlayer)
			{
				ChooseMovementTarget();

				if (!mMovementAgent.SetDestination(mMovementTarget))
					mMovementTarget = new Vector3(-10000, -10000, -10000);
			}
		}

		private void TickChaseAndShoot()
		{
			// do both of the previous
			TickChaseAfterPlayer();
			TickShootAtPlayer();
		}

		private void TickLostPlayer()
		{
			// we lost the player. check for a timeout, then return to idle or the appropriate state.
		}

		#endregion

		#region Common Utility Funcions

		/// <summary>
		/// Returns true if we currently have any senses that let us know where the player is.
		/// This means whether we can see the player or have been recently attacked by the player.
		/// </summary>
		private bool KnowsOfPlayer()
		{
			return CanSeePlayer() || wasRecentlyAttacked;
		}

		/// <summary>
		/// If the player is within our line-of-sight
		/// </summary>
		private bool CanSeePlayer()
		{
			Vector3 directionToPlayer = (mPlayerRef.position - mEye.position).normalized;

			float dot = Vector3.Dot(mEye.forward.normalized, directionToPlayer);
			bool inLoS = dot >= mVars.mLineOfSight;

			if (inLoS)
			{
				RaycastHit hitInfo;
				Ray ray = new Ray(mEye.position, directionToPlayer);
				bool hit = Physics.Raycast(ray, out hitInfo, 5000.0f, mVars.mVisionLayermask);

				return hit && hitInfo.collider.CompareTag("Player");
			}

			return false;
		}
		
		/// <summary>
		/// Whether or not we were hit by the player within the threshold.
		/// </summary>
		public void NotifyAttackedByPlayer()
		{
			mLastAttackedTime = Time.time;
		}

		private void ShootInDirection(Vector3 faceDirection, Vector3 shootDirection)
		{
			Quaternion randomishRot = Quaternion.LookRotation(shootDirection, Vector3.up);
			Quaternion perciseRot = Quaternion.LookRotation(faceDirection, Vector3.up);
			Quaternion bodyRot = Quaternion.Euler(0.0f, perciseRot.eulerAngles.y, 0.0f);

			transform.rotation = Quaternion.Slerp(transform.rotation, bodyRot, Time.deltaTime * mVars.mRotateSpeed);

			Quaternion realEyeGoal = Quaternion.Euler(randomishRot.eulerAngles.x, transform.rotation.eulerAngles.y, randomishRot.eulerAngles.z);
			mEye.rotation = realEyeGoal;

			UnityEngine.Debug.DrawLine(mEye.position, mEye.position + mEye.forward * 20.0f, Color.yellow);

			mWeapon.FireWeapon();
		}
		
		private Action DecideToChaseOrShoot()
		{
			// return TickShootAtPlayer, TickChaseAfterPlayer, or TickChaseAndShoot
			return TickShootAtPlayer;
		}

		private Dictionary<Vector3, float> mDebugPositions;
		public void OnDrawGizmos()
		{
			if (mDebugPositions == null)
				return;

			foreach (var pos in mDebugPositions)
			{
				Gizmos.color = Color.Lerp(Color.red, Color.green, pos.Value);
				UnityEditor.Handles.Label(pos.Key, pos.Value.ToString("#.###"));
				Gizmos.DrawSphere(pos.Key, 0.25f);
			}

			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(mMovementTarget + Vector3.up, 0.4f);
		}

		private void ChooseMovementTarget()
		{
			mCurrentPath = new NavMeshPath();
			var validPositions = mHintingSystemRef.EvaluatePosition(transform.position, mVars.mHintData);
			mDebugPositions = validPositions;

			var positions = validPositions.OrderByDescending(x => x.Value).Select(x => x.Key).ToArray();
			int i = 0;
			do
			{
				mMovementTarget = positions[i];
				i++;
			} while (!mMovementAgent.CalculatePath(mMovementTarget, mCurrentPath) && i < positions.Length);

			if (i >= positions.Length)
				GameLogger.Warn("AI " + transform.name + " was unable to find a target location.");
		}

		#endregion

		public void OnGUI()
		{
			if (GUILayout.Button("IDLE STATE"))
				mCurrentState = TickIdle;
			if (GUILayout.Button("Miss Player State"))
				mCurrentState = TickPurposefullyMissPlayer;
			if (GUILayout.Button("Shoot At Player"))
				mCurrentState = TickShootAtPlayer;
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Chase After Player"))
				mCurrentState = TickChaseAfterPlayer;
			if (GUILayout.Button("Force New Position"))
				ChooseMovementTarget();
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Chase And Shoot"))
				mCurrentState = TickChaseAndShoot;
			if (GUILayout.Button("Lost Player"))
				mCurrentState = TickLostPlayer;

			GUILayout.Label("Know where the player is? " + KnowsOfPlayer());
		}

	}
}
