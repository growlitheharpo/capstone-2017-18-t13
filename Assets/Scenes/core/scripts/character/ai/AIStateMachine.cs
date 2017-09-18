using System;
using FiringSquad.Data;
using KeatsLib.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace FiringSquad.Gameplay.AI
{
	public class AIStateMachine : BaseStateMachine
	{
		[SerializeField] private StateMachineVariables mTweakables;
		[Serializable] private class StateMachineVariables
		{
			[SerializeField] private float mRecentlyAttackedTimeThreshold;
			[SerializeField] private float mLineOfSight;
			[SerializeField] private float mBaseInaccuracy;
			[SerializeField] private float mPurposefullyMissPlayerScale;
			[SerializeField] private float mPurposefullyMissPlayerTime;
			[SerializeField] private float mLostPlayerTimeout;
			[SerializeField] private AIHintValueData mHintData;
			[SerializeField] private LayerMask mVisionLayermask;

			public float lineOfSight { get { return mLineOfSight; } }
			public float recentlyAttackedTimeThreshold { get { return mRecentlyAttackedTimeThreshold; } }
			public float baseInaccuracy { get { return mBaseInaccuracy; } }
			public float purposefullyMissPlayerScale { get { return mPurposefullyMissPlayerScale; } }
			public float purposefullyMissPlayerTime { get { return mPurposefullyMissPlayerTime; } }
			public float lostPlayerTimeout { get { return mLostPlayerTimeout; } }
			public AIHintValueData hintData { get { return mHintData; } }
			public LayerMask visionLayermask { get { return mVisionLayermask; } }
		}

		private StateMachineVariables tweakables { get { return mTweakables; } }
		private NavMeshAgent mMovementAgent;
		private AICharacter mCharacter;
		private Transform mPlayerRef;

		private Vector3 mLastKnownPlayerLocation;

		private float mLastAttackedTime = float.MinValue;
		private bool wasRecentlyAttacked
		{
			get { return Time.time - mLastAttackedTime <= tweakables.recentlyAttackedTimeThreshold; }
		}

		private AIHintingSystem mHintSystemRef;
		private AIHintingSystem.PositionEvaluationResult mMostRecentResult;

		#region Unity Signals

		private void Start()
		{
			mCharacter = GetComponent<AICharacter>();
			mMovementAgent = GetComponent<NavMeshAgent>();
			mPlayerRef = ReferenceForwarder.get.player.transform;
			mHintSystemRef = ReferenceForwarder.get.aiHintSystem;

			TransitionStates(new IdleState(this));
		}

#if DEBUG || DEVELOPMENT_BUILD
		private bool mSuppressNormalTransitions;

		protected override void Update()
		{
			if (!mSuppressNormalTransitions)
				base.Update();
			else
				currentState.Update();
		}

#endif

		private void OnGUI()
		{
			GUILayout.Label("Current state: " + currentState.GetType().Name);
			if (GUILayout.Button("Idle State"))
				TransitionStates(new IdleState(this));
			if (GUILayout.Button("Just Saw Player State"))
				TransitionStates(new JustSawPlayerState(this));
			if (GUILayout.Button("Shoot At Player State"))
				TransitionStates(new ShootAtPlayerState(this));
			if (GUILayout.Button("Shoot and Chase State"))
				TransitionStates(new ShootAndChasePlayerState(this));
			if (GUILayout.Button("Seek Lost Player State"))
				TransitionStates(new SeekOutPlayer(this));
			if (GUILayout.Button("Lost Player State"))
				TransitionStates(new LostPlayerState(this));

#if DEBUG || DEVELOPMENT_BUILD
			if (GUILayout.Button(mSuppressNormalTransitions ? "Re-Enable Normal Transitions" : "Suppress Normal Transitions"))
				mSuppressNormalTransitions = !mSuppressNormalTransitions;
#endif

		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (mMostRecentResult == null || mMostRecentResult.positionScores == null)
				return;

			foreach (var pos in mMostRecentResult.positionScores)
			{
				Gizmos.color = Color.Lerp(Color.red, Color.green, pos.Value);
				UnityEditor.Handles.Label(pos.Key, pos.Value.ToString("#.###"));
				Gizmos.DrawSphere(pos.Key, 0.25f);
			}

			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(mMostRecentResult.positionScores.GetHighestValueKey() + Vector3.up, 0.4f);
		}
#endif

		#endregion

		#region States

		private class IdleState : BaseState<AIStateMachine>
		{
			public IdleState(AIStateMachine m) : base(m) { }

			public override void Update()
			{
				// TODO: Hang out. play a bark. play an idle animation.
			}

			public override IState GetTransition()
			{
				return mMachine.KnowsOfPlayer() 
					? (IState)new JustSawPlayerState(mMachine) 
					: this;
			}
		}

		private class JustSawPlayerState : BaseState<AIStateMachine>
		{
			public JustSawPlayerState(AIStateMachine m) : base(m) { }

			public override void OnEnter()
			{
				// TODO: Play a bark notifying the player we see them. Update animation.
			}

			public override IState GetTransition()
			{
				Vector3 bestPos = mMachine.GetBestTargetPosition();

				if (Vector3.Distance(bestPos, mMachine.transform.position) < 5.0f)
					return new ShootAtPlayerState(mMachine);

				return new ShootAndChasePlayerState(mMachine);
			}
		}

		private class ShootAtPlayerState : BaseState<AIStateMachine>
		{
			public ShootAtPlayerState(AIStateMachine m) : base(m)
			{
				mMissPlayerTimer = m.tweakables.purposefullyMissPlayerTime;
			}

			private CharacterMovementData movementData { get { return mMachine.mCharacter.movementData; } }
			private Transform transform { get { return mMachine.transform; } }
			private Transform eye { get { return mMachine.mCharacter.eye; } }

			private float mMissPlayerTimer;

			public override void Update()
			{
				if (mMissPlayerTimer > 0.0f)
				{
					ShootNearPlayer();
					mMissPlayerTimer -= Time.deltaTime;
				}
				else
					ShootAtPlayer();
				
			}

			private void ShootNearPlayer()
			{
				// instruct our gun to shoot near the player, but not AT them.
				Vector3 dirToPlayer = mMachine.mPlayerRef.position - transform.position;

				Vector3 leftDir = Vector3.Cross(dirToPlayer, transform.up).normalized;
				Vector3 rightDir = -leftDir;

				Vector3 closerDir = Vector3.Dot(leftDir, transform.forward) > Vector3.Dot(rightDir, transform.forward) ? leftDir : rightDir;
				Vector3 target = dirToPlayer + closerDir * mMachine.tweakables.purposefullyMissPlayerScale * Vector3.Distance(transform.position, mMachine.mPlayerRef.position);

				UnityEngine.Debug.DrawLine(transform.position, transform.position + target, Color.HSVToRGB(0.8f, 0.6f, 0.55f));

				// TODO: Don't face towards the target, face the player!
				ShootInDirection(target, target);
			}

			private void ShootAtPlayer()
			{
				Vector3 dirToPlayer = mMachine.mPlayerRef.position - transform.position;
				float spreadFactor = mMachine.tweakables.baseInaccuracy;

				Vector3 randomness = new Vector3(
					UnityEngine.Random.Range(-spreadFactor, spreadFactor),
					UnityEngine.Random.Range(-spreadFactor, spreadFactor),
					UnityEngine.Random.Range(-spreadFactor, spreadFactor));

				Vector3 eyeGoal = dirToPlayer + randomness;
				ShootInDirection(dirToPlayer, eyeGoal);
			}

			private void ShootInDirection(Vector3 faceDirection, Vector3 shootDirection)
			{
				Quaternion randomishRot = Quaternion.LookRotation(shootDirection, Vector3.up);
				Quaternion perciseRot = Quaternion.LookRotation(faceDirection, Vector3.up);
				Quaternion bodyRot = Quaternion.Euler(0.0f, perciseRot.eulerAngles.y, 0.0f);

				transform.rotation = Quaternion.Slerp(transform.rotation, bodyRot, Time.deltaTime * movementData.lookSpeed);

				Quaternion realEyeGoal = Quaternion.Euler(randomishRot.eulerAngles.x, transform.rotation.eulerAngles.y, randomishRot.eulerAngles.z);
				eye.rotation = realEyeGoal;

				UnityEngine.Debug.DrawLine(eye.position, eye.position + eye.forward * 20.0f, Color.yellow);

				mMachine.mCharacter.weapon.FireWeapon();
			}

			public override IState GetTransition()
			{
				if (!mMachine.KnowsOfPlayer())
					return new SeekOutPlayer(mMachine);

				return mMachine.IsPositionEvaluationStale() ? (IState)new ShootAndChasePlayerState(mMachine) : this;
			}
		}
		
		private class ShootAndChasePlayerState : BaseState<AIStateMachine>
		{
			public ShootAndChasePlayerState(AIStateMachine m) : base(m) { }

			private ShootAtPlayerState mInnerState;
			private Vector3 mGoal;

			public override void OnEnter()
			{
				mInnerState = new ShootAtPlayerState(mMachine);

				mGoal = mMachine.GetBestTargetPosition();
				mMachine.mMovementAgent.SetDestination(mGoal);
			}

			public override void Update()
			{
				if (Vector3.Distance(mMachine.mPlayerRef.position, mMachine.mMostRecentResult.playerPosition) > 15.0f)
				{
					mGoal = mMachine.GetBestTargetPosition();
					mMachine.mMovementAgent.SetDestination(mGoal);
				}

				mInnerState.Update();
			}

			public override IState GetTransition()
			{
				if (!mMachine.IsPositionEvaluationStale() && Vector3.Distance(mGoal, mMachine.transform.position) <= 1.0f)
					return mInnerState;
				if (!mMachine.KnowsOfPlayer())
					return new SeekOutPlayer(mMachine);
				return this;
			}
		}

		/// <summary>
		/// Go to the last known player location to see if we can find them from there.
		/// </summary>
		private class SeekOutPlayer : BaseState<AIStateMachine>
		{
			public SeekOutPlayer(AIStateMachine m) : base(m) { }

			private Vector3 mGoal;
			private float mTimer;

			public override void OnEnter()
			{
				mGoal = mMachine.mLastKnownPlayerLocation;
				mMachine.mMovementAgent.SetDestination(mGoal);
				mTimer = 10.0f;
			}

			public override void Update()
			{
				base.Update();
				mTimer -= Time.deltaTime;
			}

			public override IState GetTransition()
			{
				// if we found the player, start shooting at them
				if (mMachine.KnowsOfPlayer())
					return new JustSawPlayerState(mMachine);

				// if we reached our goal and we still don't know where they are, we give up.
				if (Vector3.Distance(mMachine.transform.position, mGoal) < 1.0f || mTimer <= 0.0f)
					return new LostPlayerState(mMachine);

				return this;
			}
		}

		private class LostPlayerState : BaseState<AIStateMachine>
		{
			public LostPlayerState(AIStateMachine m) : base(m) { }

			private float mTimer, mStartVal;
			private Quaternion mLeftRot, mRightRot;

			public override void OnEnter()
			{
				mStartVal = mMachine.tweakables.lostPlayerTimeout;
				mTimer = mStartVal;

				mLeftRot = mMachine.transform.rotation * Quaternion.AngleAxis(-90.0f, Vector3.up);
				mRightRot = mMachine.transform.rotation * Quaternion.AngleAxis(90.0f, Vector3.up);
			}

			public override void Update()
			{
				mTimer -= Time.deltaTime;
				
				// we want to go from 0.5 to 1 to 0 to 0.5
				// range is 2.5, start is 0.5, end is 3.0
				// total percent is 0 to 1
				float totalPercent = (mStartVal - mTimer) / mStartVal;
				float currentPercent = Mathf.Lerp(0.5f, 2.5f, totalPercent);
				mMachine.transform.rotation = Quaternion.Lerp(mLeftRot, mRightRot, Mathf.PingPong(currentPercent, 1.0f));
			}

			public override IState GetTransition()
			{
				if (mMachine.KnowsOfPlayer())
					return new JustSawPlayerState(mMachine);

				return mTimer <= 0.0f ? (IState)new IdleState(mMachine) : this;
			}
		}

		#endregion

		#region Utility Functions

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
			Vector3 directionToPlayer = (mPlayerRef.position - mCharacter.eye.position).normalized;

			float dot = Vector3.Dot(mCharacter.eye.forward.normalized, directionToPlayer);
			bool inLoS = dot >= tweakables.lineOfSight;

			if (!inLoS)
				return false;

			RaycastHit hitInfo;
			Ray ray = new Ray(mCharacter.eye.position, directionToPlayer);
			bool hit = Physics.Raycast(ray, out hitInfo, 5000.0f, tweakables.visionLayermask);

			if (!hit || !hitInfo.collider.CompareTag("Player"))
				return false;

			mLastKnownPlayerLocation = hitInfo.transform.position;
			return true;
		}

		/// <summary>
		/// Whether or not we were hit by the player within the threshold.
		/// </summary>
		public void NotifyAttackedByPlayer()
		{
			mLastAttackedTime = Time.time;
		}

		private Vector3 GetBestTargetPosition()
		{
			if (mMostRecentResult == null || Time.time - mMostRecentResult.time > Time.deltaTime * 2.0f)
				mMostRecentResult = mHintSystemRef.EvaluatePosition(transform.position, tweakables.hintData);

			var scores = mMostRecentResult.positionScores;
			return scores.GetHighestValueKey();
		}

		private bool IsPositionEvaluationStale()
		{
			if (Time.time - mMostRecentResult.time >= 6.0f)
				return true;

			if (Vector3.Distance(mMostRecentResult.playerPosition, mPlayerRef.position) > 10.0f)
				return true;

			if (Time.time - mMostRecentResult.time > 1.5f)
			{
				AIHintingSystem.PositionEvaluationResult currentVal = mMostRecentResult;

				Vector3 bestPos = GetBestTargetPosition();
				if (Vector3.Distance(bestPos, transform.position) > 4.0f)
					return true;

				mMostRecentResult = currentVal;
			}

			return false;
		}

		#endregion
	}
}