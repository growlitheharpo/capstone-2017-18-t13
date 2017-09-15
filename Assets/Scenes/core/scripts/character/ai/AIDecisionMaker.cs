using System;
using UnityEngine;
using UnityEngine.AI;
using GameLogger = Logger;

namespace FiringSquad.Gameplay
{
	public class AIDecisionMaker
	{
		[Serializable]
		public class DecisionMakerVariables
		{
			public float mClosestDistanceToPlayer;
			public float mMaxDistanceFromPlayer;
			public float mLineOfSight;

			public LayerMask mVisionLayermask;
		}

		private Action mCurrentState;
		private DecisionMakerVariables mVars;

		private NavMeshPath mCurrentPath = null;
		private NavMeshAgent mMovementAgent;
		private IWeapon mWeapon;
		private Transform mEye;

		private Transform transform { get; set; }

		private Vector3 mMovementTarget;

		public AIDecisionMaker(DecisionMakerVariables vars, IWeapon weapon, Transform eye, NavMeshAgent agent)
		{
			mCurrentState = TickIdle;
			mMovementAgent = agent;
			mWeapon = weapon;
			mEye = eye;
			mVars = vars;

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
				mCurrentState = TickIdle;
		}

		private void TickPurposefullyMissPlayer()
		{
			// instruct our gun to shoot near the player, but not AT them.
		}

		private void TickShootAtPlayer()
		{
			// shoot at the player, with some level of inaccuracy.
		}

		private void TickChaseAfterPlayer()
		{
			Transform player = GetPlayerTransform();

			if (mMovementTarget == new Vector3(-10000, -10000, -10000) || Vector3.Distance(transform.position, player.position) > mVars.mMaxDistanceFromPlayer)
			{
				ChooseMovementTarget(player);

				GameLogger.Info(mMovementTarget.ToString());

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
			return CanSeePlayer() || AttackedByPlayer();
		}

		/// <summary>
		/// If the player is within our line-of-sight
		/// </summary>
		private bool CanSeePlayer()
		{
			Transform player = GetPlayerTransform();
			Vector3 directionToPlayer = (player.position - mEye.position).normalized;

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
		/// If the player is within our line-of-sight
		/// </summary>
		private bool CanSeePlayer(Ray ray)
		{
			Transform player = GetPlayerTransform();
			
			//TODO: Add more cone stuff.
			RaycastHit hit;
			bool hitYes = Physics.Raycast(ray, out hit, 5000.0f, mVars.mVisionLayermask);// && hit.collider.CompareTag("Player");
			
			UnityEngine.Debug.DrawLine(ray.origin, hitYes ? hit.point : ray.origin + ray.direction * 3000.0f, hitYes ? Color.cyan : Color.red, 2.0f);
			return hitYes && hit.collider.CompareTag("Player");

		}

		/// <summary>
		/// Whether or not we were hit by the player within the threshold.
		/// </summary>
		private bool AttackedByPlayer()
		{
			return false;
		}

		private static Transform GetPlayerTransform()
		{
			// TODO: Cache this for better performance
			return ReferenceForwarder.get.player.transform;
		}

		private Action DecideToChaseOrShoot()
		{
			// return TickShootAtPlayer, TickChaseAfterPlayer, or TickChaseAndShoot
			return TickShootAtPlayer;
		}

		/// <summary>
		/// TODO: Rewrite this entire function. It works for now.
		/// </summary>
		/// <param name="player"></param>
		private void ChooseMovementTarget(Transform player)
		{
			mCurrentPath = new NavMeshPath();

			Vector3 pointInFrontOfPlayer = player.position + player.forward * mVars.mClosestDistanceToPlayer + Vector3.up;
			mMovementTarget = pointInFrontOfPlayer;

			if (!CanSeePlayer(new Ray(mMovementTarget, player.position - mMovementTarget)) || !mMovementAgent.CalculatePath(mMovementTarget, mCurrentPath))
				mMovementTarget = player.position + player.right * mVars.mClosestDistanceToPlayer + Vector3.up;
			else
				return;

			// can't see from front or right, check left
			if (!CanSeePlayer(new Ray(mMovementTarget, player.position - mMovementTarget)) || !mMovementAgent.CalculatePath(mMovementTarget, mCurrentPath))
				mMovementTarget = player.position + player.right * -mVars.mClosestDistanceToPlayer + Vector3.up;
			else
				return;

			// can't see from any of the above. just use the front position.
			if (!CanSeePlayer(new Ray(mMovementTarget, player.position - mMovementTarget)) || !mMovementAgent.CalculatePath(mMovementTarget, mCurrentPath))
				return;

			NavMeshHit hit;
			NavMesh.SamplePosition(pointInFrontOfPlayer, out hit, 10.0f, NavMesh.AllAreas);
			mMovementTarget = hit.position;
			mCurrentPath = null;

			UnityEngine.Debug.DrawLine(mMovementTarget, mMovementTarget + Vector3.up * 3.0f, Color.green);
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
				ChooseMovementTarget(ReferenceForwarder.get.player.transform);
			GUILayout.EndHorizontal();
			if (GUILayout.Button("Chase And Shoot"))
				mCurrentState = TickChaseAndShoot;
			if (GUILayout.Button("Lost Player"))
				mCurrentState = TickLostPlayer;

			GUILayout.Label("Can see player? " + CanSeePlayer());
		}
	}
}
