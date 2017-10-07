using System;
using System.Collections;
using FiringSquad.Data;
using KeatsLib.State;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Input = UnityEngine.Input;

namespace FiringSquad.Gameplay
{
	public class PlayerGravGunWeapon : NetworkBehaviour
	{
		private const float SNAP_DISTANCE = 4.0f;

		// mock the weapon interface
		public IWeaponBearer bearer { get; set; }

		[SerializeField] private LayerMask mGrabMask;
		[SerializeField] [Range(0.0f, 1.0f)] private float mReelInSensitivity;
		[SerializeField] private float mPullStrength;
		[SerializeField] private float mThrowForce;
		[SerializeField] private float mHoldForThrowTime;

		public IInteractable heldObject
		{
			get
			{
				IInteractable i = null;

				if (mHoldTarget != null)
					i = mHoldTarget.GetComponentUpwards<IInteractable>();

				if (i != null)
					return i;

				foreach (Transform t in transform)
				{
					i = t.GetComponentUpwards<IInteractable>();
					if (i != null)
						return i;
				}

				return null;
			}
		}

		private Rigidbody mHoldTarget;

		private GravGunStateMachine mMachine;
		
		private void Update()
		{
			if (mMachine != null)
				mMachine.Update();
		}

		[TargetRpc]
		public void TargetRpcRegisterInput(NetworkConnection target, NetworkInstanceId playerId, Vector3 localPos)
		{
			GameObject go = ClientScene.FindLocalObject(playerId);
			if (go == null)
				throw new ArgumentException("Could not attach gravity gun to a player: " + playerId);

			PlayerScript playerScript = go.GetComponent<PlayerScript>();

			bearer = playerScript;
			transform.SetParent(go.transform, false);
			transform.ResetLocalValues();
			transform.localPosition = localPos;

			PlayerInputMap input = playerScript.inputMap;

			mMachine = new GravGunStateMachine(this);

			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetButtonDown, input.fireGravGunButton, HandlePressed, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButton, input.fireGravGunButton, HandleHeld, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, input.fireGravGunButton, HandleReleased, KeatsLib.Unity.Input.InputLevel.Gameplay);
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(HandlePressed)
				.UnregisterInput(HandleHeld)
				.UnregisterInput(HandleReleased);
		}

		private void HandlePressed()
		{
			GravGunStateMachine.IGravGunState realState = mMachine.currentGravState;
			if (realState != null)
				realState.OnInputPressed();
		}

		private void HandleHeld()
		{
			GravGunStateMachine.IGravGunState realState = mMachine.currentGravState;
			if (realState != null)
				realState.OnInputHeld();
		}

		private void HandleReleased()
		{
			GravGunStateMachine.IGravGunState realState = mMachine.currentGravState;
			if (realState != null)
				realState.OnInputReleased();
		}

		[Command]
		private void CmdAddForceToObject(NetworkInstanceId id, Vector3 force)
		{
			GameObject go = NetworkServer.FindLocalObject(id);

			Rigidbody rb = go.GetComponent<Rigidbody>();
			rb.AddForce(force, ForceMode.Impulse);
		}

		[Command]
		private void CmdGrabObject(NetworkInstanceId id)
		{
			GameObject go = NetworkServer.FindLocalObject(id);
			LockObject(go);
			RpcGrabObjectClient(id);
		}

		[Command]
		private void CmdReleaseObject(NetworkInstanceId id)
		{
			GameObject go = NetworkServer.FindLocalObject(id);
			ReleaseObject(go);
			RpcReleaseObjectClient(id);
		}

		[ClientRpc]
		private void RpcGrabObjectClient(NetworkInstanceId part)
		{
			GameObject go = ClientScene.FindLocalObject(part);
			LockObject(go);
			mHoldTarget = go.GetComponent<Rigidbody>();
		}

		[ClientRpc]
		private void RpcReleaseObjectClient(NetworkInstanceId part)
		{
			GameObject go = ClientScene.FindLocalObject(part);
			ReleaseObject(go);
			mHoldTarget = null;
		}

		private void LockObject(GameObject go)
		{
			Rigidbody rb = go.GetComponent<Rigidbody>();

			go.transform.SetParent(transform);
			rb.useGravity = false;
			rb.constraints = RigidbodyConstraints.FreezeAll;
			StartCoroutine(Coroutines.LerpPosition(go.transform, Vector3.zero, 0.3f));
		}

		private void ReleaseObject(GameObject go)
		{
			Rigidbody rb = go.GetComponent<Rigidbody>();

			go.transform.SetParent(null);
			rb.useGravity = true;
			rb.constraints = RigidbodyConstraints.None;
		}

		private class GravGunStateMachine : BaseStateMachine
		{
			public IGravGunState currentGravState { get { return currentState as GravGunState; } }
			private PlayerGravGunWeapon mScript;

			public GravGunStateMachine(PlayerGravGunWeapon script)
			{
				mScript = script;
				TransitionStates(new IdleState(this));
			}

			public new void Update()
			{
				base.Update();
			}

			public interface IGravGunState
			{
				void OnInputPressed();
				void OnInputHeld();
				void OnInputReleased();
			}

			private abstract class GravGunState : BaseState<GravGunStateMachine>, IGravGunState
			{
				protected GravGunState(GravGunStateMachine machine) : base(machine) { }

				public virtual void OnInputPressed() { }
				public virtual void OnInputHeld() { }
				public virtual void OnInputReleased() { }
			}

			private class IdleState : GravGunState
			{
				public IdleState(GravGunStateMachine m) : base(m) { }

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
				public TryDrawObjectState(GravGunStateMachine m) : base(m) { }

				private NetworkIdentity mPullTarget;
				private bool mCancelled;

				private bool objectInRange
				{
					get
					{
						return mPullTarget != null
								&& Vector3.Distance(mPullTarget.transform.position, mMachine.mScript.transform.position) <= SNAP_DISTANCE;
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
					Transform eye = mMachine.mScript.bearer.eye;

					Ray ray = new Ray(eye.position, eye.forward);
					UnityEngine.Debug.DrawLine(ray.origin, ray.origin + ray.direction * 4000.0f, new Color(.6f, 0.0f, 1.0f), 0.2f);

					RaycastHit hit;
					if (!Physics.Raycast(ray, out hit, 4000.0f, mMachine.mScript.mGrabMask))
						return;

					Rigidbody rb = hit.rigidbody;
					if (rb == null)
						return;

					NetworkIdentity id = rb.GetComponent<NetworkIdentity>();
					if (id == null)
						return;

					mPullTarget = id;
				}

				private bool CheckStillLooking()
				{
					if (mPullTarget == null)
						return false;

					Vector3 direction = mPullTarget.transform.position - mMachine.mScript.bearer.eye.position;
					Vector3 looking = mMachine.mScript.bearer.eye.forward;

					float dot = Vector3.Dot(direction.normalized, looking.normalized);
					if (dot < mMachine.mScript.mReelInSensitivity)
					{
						mPullTarget = null;
						return false;
					}

					return true;
				}

				private void ReelObjectIn()
				{
					Vector3 direction = mMachine.mScript.transform.position - mPullTarget.transform.position;

					mMachine.mScript.CmdReleaseObject(mPullTarget.netId);
					mMachine.mScript.CmdAddForceToObject(mPullTarget.netId, direction * Time.deltaTime * mMachine.mScript.mPullStrength);
				}

				public override IState GetTransition()
				{
					if (objectInRange)
					{
						mMachine.mScript.mHoldTarget = mPullTarget.GetComponent<Rigidbody>();
						return new GrabAndHoldState(mMachine);
					}
					if (mCancelled)
						return new IdleState(mMachine);

					return this;
				}
			}

			private class GrabAndHoldState : GravGunState
			{
				public GrabAndHoldState(GravGunStateMachine m) : base(m) { }

				private RigidbodyConstraints mOriginalConstraints;

				private Transform mPreviousParent;
				private Coroutine mGrabRoutine;
				private bool mReleasedOnce, mExit;
				private float mHoldTime;
				private Vector3 mEndForce;

				public override void OnEnter()
				{
					/*mPreviousParent = mMachine.mScript.mHoldTarget.transform.parent;
					mOriginalConstraints = mMachine.mScript.mHoldTarget.constraints;
					mMachine.mScript.mHoldTarget.constraints = RigidbodyConstraints.FreezeAll;

					/*mMachine.mScript.mHoldTarget.transform.SetParent(mMachine.mScript.transform);
					mGrabRoutine = mMachine.mScript.StartCoroutine(LerpToMyPosition());*/
					mMachine.mScript.CmdGrabObject(mMachine.mScript.mHoldTarget.GetComponent<NetworkIdentity>().netId);
				}

				/*private IEnumerator LerpToMyPosition(float time = 0.3f)
				{
					Vector3 originalPos = mMachine.mScript.mHoldTarget.transform.position;
					float currentTime = 0.0f;

					while (currentTime < time)
					{
						if (mMachine.mScript.mHoldTarget == null)
							yield break;

						mMachine.mScript.mHoldTarget.transform.position = Vector3.Lerp(originalPos, mMachine.mScript.transform.position, currentTime / time);
						currentTime += Time.deltaTime;

						yield return null;
					}
				}*/

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
					if (mMachine.mScript.mHoldTarget == null)
						return;

					if (mHoldTime < mMachine.mScript.mHoldForThrowTime)
						mEndForce = Vector3.zero;
					else
						mEndForce = mMachine.mScript.bearer.eye.forward * mMachine.mScript.mHoldTarget.mass * mMachine.mScript.mThrowForce;

					if (mGrabRoutine != null)
						mMachine.mScript.StopCoroutine(mGrabRoutine);

					mExit = true;
				}

				public override void Update()
				{
					if (mMachine.mScript.mHoldTarget == null)
						mExit = true;
				}

				public override void OnExit()
				{
					if (mMachine.mScript.mHoldTarget == null)
						return;

					/*mMachine.mScript.mHoldTarget.constraints = mOriginalConstraints;
					mMachine.mScript.mHoldTarget.AddForce(mEndForce, ForceMode.Impulse);
					mMachine.mScript.mHoldTarget.transform.SetParent(mPreviousParent);
					mMachine.mScript.mHoldTarget = null;*/

					NetworkInstanceId id = mMachine.mScript.mHoldTarget.GetComponent<NetworkIdentity>().netId;

					mMachine.mScript.CmdReleaseObject(id);
					mMachine.mScript.CmdAddForceToObject(id, mEndForce);
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
}
