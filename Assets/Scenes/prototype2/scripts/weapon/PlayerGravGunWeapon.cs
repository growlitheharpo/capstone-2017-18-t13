using System.Collections;
using UnityEngine;
using Input = UnityEngine.Input;

namespace FiringSquad.Gameplay
{
	public class PlayerGravGunWeapon : BaseStateMachine
	{
		private const float SNAP_DISTANCE = 4.0f;

		private abstract class GravGunState : BaseState<PlayerGravGunWeapon>
		{
			protected GravGunState(PlayerGravGunWeapon machine) : base(machine) { }

			public virtual void OnInputPressed() { }
			public virtual void OnInputHeld() { }
			public virtual void OnInputReleased() { }
		}

		// mock the weapon interface
		public IWeaponBearer bearer { get; set; }

		[SerializeField] [Range(0.0f, 1.0f)] private float mReelInSensitivity;
		[SerializeField] private float mPullStrength;
		[SerializeField] private float mThrowForce;
		[SerializeField] private float mHoldForThrowTime;

		public IInteractable heldObject { get { return mHoldTarget == null ? null : mHoldTarget.GetComponentUpwards<IInteractable>(); } }

		private Rigidbody mHoldTarget;
		private bool mGotInputThisFrame;

		private void Start()
		{
			TransitionStates(new IdleState(this));

			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetButtonDown, "Fire2", HandlePressed, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButton, "Fire2", HandleHeld, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, "Fire2", HandleReleased, KeatsLib.Unity.Input.InputLevel.Gameplay);
		}
		
		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(HandlePressed)
				.UnregisterInput(HandleHeld)
				.UnregisterInput(HandleReleased);
		}
		
		private void HandleReleased()
		{
			GravGunState realState = currentState as GravGunState;
			if (realState != null)
				realState.OnInputReleased();
		}

		private void HandleHeld()
		{
			GravGunState realState = currentState as GravGunState;
			if (realState != null)
				realState.OnInputHeld();
		}

		private void HandlePressed()
		{
			GravGunState realState = currentState as GravGunState;
			if (realState != null)
				realState.OnInputPressed();
		}

		private class IdleState : GravGunState
		{
			public IdleState(PlayerGravGunWeapon m) : base(m) { }

			private bool mPressed;

			public override void OnInputPressed()
			{
				mPressed = true;
			}

			public override IState GetTransition()
			{
				if (mPressed)
					return new TryDrawObjectState(mMachine);
				return this;
			}
		}

		private class TryDrawObjectState : GravGunState
		{
			public TryDrawObjectState(PlayerGravGunWeapon m) : base(m) { }

			private Rigidbody mPullTarget;
			private bool mCancelled;

			private bool objectInRange
			{
				get {
					return mPullTarget != null
							&& Vector3.Distance(mPullTarget.transform.position, mMachine.transform.position) <= SNAP_DISTANCE;
				}
			}

			public override void OnInputReleased()
			{
				mCancelled = true;
			}

			public override void OnEnter()
			{
				mPullTarget = null;
			}

			public override void Update()
			{
				if (mPullTarget == null)
					TryGrabObject();
				else
				{
					if (CheckStillLooking())
						ReelObjectIn();
				}
			}

			private void TryGrabObject()
			{
				// this is where we raycast
				Transform eye = mMachine.bearer.eye;

				Ray ray = new Ray(eye.position, eye.forward);
				UnityEngine.Debug.DrawLine(ray.origin, ray.origin + ray.direction * 4000.0f, new Color(.6f, 0.0f, 1.0f), 0.2f);

				RaycastHit hit;
				if (!Physics.Raycast(ray, out hit, 4000.0f))
					return;

				Rigidbody rb = hit.rigidbody;
				if (rb == null)
					return;

				mPullTarget = rb;
			}
			
			private bool CheckStillLooking()
			{
				if (mPullTarget == null)
					return false;

				Vector3 direction = mPullTarget.position - mMachine.bearer.eye.position;
				Vector3 looking = mMachine.bearer.eye.forward;

				float dot = Vector3.Dot(direction.normalized, looking.normalized);
				if (dot < mMachine.mReelInSensitivity)
				{
					mPullTarget = null;
					return false;
				}

				return true;
			}

			private void ReelObjectIn()
			{
				Vector3 direction = mMachine.transform.position - mPullTarget.position;
				mPullTarget.AddForce(direction * Time.deltaTime * mMachine.mPullStrength, ForceMode.Impulse);
			}

			public override IState GetTransition()
			{
				if (objectInRange)
				{
					mMachine.mHoldTarget = mPullTarget;
					return new GrabAndHoldState(mMachine);
				}
				if (mCancelled)
					return new IdleState(mMachine);

				return this;
			}
		}

		private class GrabAndHoldState : GravGunState
		{
			public GrabAndHoldState(PlayerGravGunWeapon m) : base(m) { }

			private RigidbodyConstraints mOriginalConstraints;

			private Transform mPreviousParent;
			private Coroutine mGrabRoutine;
			private bool mReleasedOnce, mExit;
			private float mHoldTime;
			private Vector3 mEndForce;

			public override void OnEnter()
			{
				mOriginalConstraints = mMachine.mHoldTarget.constraints;
				mMachine.mHoldTarget.constraints = RigidbodyConstraints.FreezeAll;

				mPreviousParent = mMachine.mHoldTarget.transform.parent;
				mMachine.mHoldTarget.transform.SetParent(mMachine.transform);

				mGrabRoutine = mMachine.StartCoroutine(LerpToMyPosition());
			}
			
			private IEnumerator LerpToMyPosition(float time = 0.3f)
			{
				Vector3 originalPos = mMachine.mHoldTarget.transform.position;
				float currentTime = 0.0f;

				while (currentTime < time)
				{
					if (mMachine.mHoldTarget == null)
						yield break;

					mMachine.mHoldTarget.transform.position = Vector3.Lerp(originalPos, mMachine.transform.position, currentTime / time);
					currentTime += Time.deltaTime;

					yield return null;
				}
			}

			public override void OnInputHeld()
			{
				if (mReleasedOnce)
					mHoldTime += Time.deltaTime;
			}

			public override void OnInputReleased()
			{
				if (!mReleasedOnce)
					mReleasedOnce = true;
				else
					HandleTriggerRelease();
			}
			
			private void HandleTriggerRelease()
			{
				if (mMachine.mHoldTarget == null)
					return;

				if (mHoldTime < mMachine.mHoldForThrowTime)
					mEndForce = Vector3.zero;
				else
					mEndForce = mMachine.bearer.eye.forward * mMachine.mHoldTarget.mass * mMachine.mThrowForce;

				if (mGrabRoutine != null)
					mMachine.StopCoroutine(mGrabRoutine);

				mExit = true;
			}

			public override void Update()
			{
				if (mMachine.mHoldTarget == null)
					mExit = true;
			}

			public override void OnExit()
			{
				if (mMachine.mHoldTarget == null)
					return;

				mMachine.mHoldTarget.constraints = mOriginalConstraints;
				mMachine.mHoldTarget.AddForce(mEndForce, ForceMode.Impulse);
				mMachine.mHoldTarget.transform.SetParent(mPreviousParent);
				mMachine.mHoldTarget = null;
			}

			public override IState GetTransition()
			{
				if (mExit)
					return new IdleState(mMachine);

				return this;
			}
		}
	}
}
