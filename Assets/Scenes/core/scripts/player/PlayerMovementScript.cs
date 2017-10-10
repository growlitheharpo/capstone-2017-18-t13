using FiringSquad.Data;
using KeatsLib;
using UnityEngine;
using UnityEngine.Networking;
using Input = UnityEngine.Input;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Component strictly for handling the movement of the character
	/// object through the game world.
	/// </summary>
	/// <inheritdoc />
	public class PlayerMovementScript : NetworkBehaviour
	{
		[SerializeField] private CharacterMovementData mMovementData;
		[SerializeField] private Animator mAnimator;

		private CapsuleCollider mCollider;
		private CharacterController mController;
		//private Transform mMainCameraRef;

		private IWeaponBearer mPlayer;

		private PlayerInputMap mInputBindings;
		private Vector2 mInput;
		private Vector3 mMoveDirection;
		private Vector2 mRotationAmount;
		private float mMouseSensitivity;
		private float mRotationY;
		private bool mJump, mIsJumping, mIsRunning, mPreviouslyGrounded, mCrouching;

		private float mStandingHeight;
		private float mStandingRadius;

		private void Awake()
		{
			mMoveDirection = Vector3.zero;
			mCollider = GetComponent<CapsuleCollider>();
			mController = GetComponent<CharacterController>();
			mPlayer = GetComponent<PlayerScript>();

			mMouseSensitivity = 1.0f;
			mStandingHeight = mCollider.height;
			mStandingRadius = mCollider.radius;
		}

		private void Start()
		{
			if (!isLocalPlayer)
				return;

			// Remove the view for the local player.
			if (mAnimator != null)
			{
				Destroy(mAnimator.gameObject);
				mAnimator = null;
			}

			PlayerInputMap input = GetComponent<PlayerScript>().inputMap;

			ServiceLocator.Get<IInput>()
				.RegisterAxis(Input.GetAxis, input.moveSidewaysAxis, INPUT_LeftRightMovement, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, input.moveBackFrontAxis, INPUT_ForwardBackMovement, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, input.lookLeftRightAxis, INPUT_LookHorizontal, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, input.lookUpDownAxis, INPUT_LookVertical, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, input.jumpButton, INPUT_Jump, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, input.crouchButton, INPUT_CrouchStart, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, input.crouchButton, INPUT_CrouchStop, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, input.sprintButton, INPUT_SprintStart, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, input.sprintButton, INPUT_SprintStop, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);

			EventManager.OnApplyOptionsData += ApplyOptionsData;
			mInputBindings = input;
		}

		private void OnDestroy()
		{
			if (!isLocalPlayer)
				return;

			ServiceLocator.Get<IInput>()
				.UnregisterInput(INPUT_Jump)
				.UnregisterInput(INPUT_CrouchStart)
				.UnregisterInput(INPUT_CrouchStop)
				.UnregisterAxis(INPUT_ForwardBackMovement)
				.UnregisterAxis(INPUT_LeftRightMovement)
				.UnregisterAxis(INPUT_LookHorizontal)
				.UnregisterAxis(INPUT_LookVertical);

			EventManager.OnApplyOptionsData -= ApplyOptionsData;
		}

		#region Input Delegates

		private void INPUT_ForwardBackMovement(float val)
		{
			//mCumulativeMovement += transform.forward * mMovementData.forwardSpeed * val;
			mInput.y = val;
		}

		private void INPUT_LeftRightMovement(float val)
		{
			//mCumulativeMovement += transform.right * val * mMovementData.strafeSpeed;
			mInput.x = val;
		}

		private void INPUT_LookHorizontal(float val)
		{
			mRotationAmount.x += val;
		}

		private void INPUT_LookVertical(float val)
		{
			mRotationAmount.y += val;
		}

		private void INPUT_Jump()
		{
			mJump = true;
		}

		private void INPUT_CrouchStart()
		{
			mCrouching = true;
		}

		private void INPUT_CrouchStop()
		{
			mCrouching = false;
		}

		private void INPUT_SprintStart()
		{
			mIsRunning = true;
		}
		
		private void INPUT_SprintStop()
		{
			if (!mInputBindings.stickySprint)
				mIsRunning = false;
		}

		#endregion

		[ClientCallback]
		private void Update()
		{
			if (!isLocalPlayer)
				return;

			HandleRotation();
			UpdateCrouch();

			if (!mPreviouslyGrounded && mController.isGrounded)
			{
				mMoveDirection.y = 0.0f;
				// play landing sound
				mIsJumping = false;
			}
			if (!mController.isGrounded && !mIsJumping && mPreviouslyGrounded)
				mMoveDirection.y = 0.0f;
			mPreviouslyGrounded = mController.isGrounded;

			UpdateAnimatorState();
		}

		[ClientCallback]
		private void FixedUpdate()
		{
			if (!isLocalPlayer)
				return;

			ApplyMovementForce();
		}

		/// <summary>
		/// Follow the mouse or joystick rotation.
		/// Horizontal rotation is applied to this.transform.
		/// Vertical rotation is only applied to Camera.main.transform.
		/// </summary>
		private void HandleRotation()
		{
			Vector2 rotation = mRotationAmount * mMovementData.lookSpeed * mMouseSensitivity;
			transform.RotateAround(transform.position, transform.up, rotation.x);

			mRotationY += rotation.y;// + (mRecoilAmount * Time.deltaTime);
			mRotationY = GenericExt.ClampAngle(mRotationY, -85.0f, 85.0f);

			float realRotation = mRotationY + mPlayer.weapon.GetCurrentRecoil();
			mPlayer.eye.localRotation = Quaternion.AngleAxis(realRotation, Vector3.left);

			mRotationAmount = Vector2.zero;
		}

		/// <summary>
		/// Squish or stretch our collider based on the crouch state.
		/// </summary>
		private void UpdateCrouch()
		{
			float currentHeight = mCollider.height;
			float newHeight = Mathf.Lerp(currentHeight, mCrouching ? mStandingHeight * mMovementData.crouchHeight : mStandingHeight, Time.deltaTime * mMovementData.crouchSpeed);
			mCollider.height = newHeight;
			mController.height = newHeight;

			float currentRadius = mCollider.radius;
			float newRadius = Mathf.Lerp(currentRadius, mCrouching ? mStandingRadius * mMovementData.crouchHeight : mStandingRadius, Time.deltaTime * mMovementData.crouchSpeed);
			mCollider.radius = newRadius;
			mController.radius = newRadius;
		}

		private void UpdateAnimatorState()
		{
			Vector3 relativeVel = mController.velocity / mMovementData.speed;
			relativeVel = transform.InverseTransformDirection(relativeVel);
			Vector2 vel = new Vector2(relativeVel.x, relativeVel.z);

			CmdSendAnimatorState(vel.x, vel.y, mCrouching);
		}

		[Command]
		private void CmdSendAnimatorState(float velX, float velY, bool crouch)
		{
			RpcClientUpdateAnimatorState(velX, velY, crouch);
		}

		[Command]
		private void CmdStartJumpAnimation()
		{
			RpcStartJumpAnim();
		}

		[ClientRpc]
		private void RpcClientUpdateAnimatorState(float velX, float velY, bool crouch)
		{
			if (mAnimator == null)
				return;

			//VelocityX, VelocityY, Crouch -> values. Jump, Fire -> triggers
			mAnimator.SetFloat("VelocityX", Mathf.Lerp(mAnimator.GetFloat("VelocityX"), velX, Time.deltaTime * 3.0f));
			mAnimator.SetFloat("VelocityY", Mathf.Lerp(mAnimator.GetFloat("VelocityY"), velY, Time.deltaTime * 3.0f));
			mAnimator.SetBool("Crouch", crouch);
		}

		[ClientRpc]
		private void RpcStartJumpAnim()
		{
			if (mAnimator == null)
				return;

			mAnimator.SetTrigger("Jump");
		}

		/// <summary>
		/// Apply movement based on the input we received this frame.
		/// </summary>
		private void ApplyMovementForce()
		{
			Vector3 desiredMove = transform.forward * mInput.y + transform.right * mInput.x;

			RaycastHit hitInfo;
			Physics.SphereCast(transform.position, mController.radius, Vector3.down, out hitInfo,
				mController.height / 2.0f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
			desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

			float speed = mMovementData.speed;
			if (mIsRunning)
				speed *= mMovementData.sprintMultiplier;

			mMoveDirection.x = desiredMove.x * speed;
			mMoveDirection.z = desiredMove.z * speed;

			if (mController.isGrounded)
			{
				mMoveDirection.y = -mMovementData.stickToGroundForce;

				if (mJump)
				{
					mMoveDirection.y = mMovementData.jumpForce;
					// play jump sound
					CmdStartJumpAnimation();
					mJump = false;
					mIsJumping = true;
				}
			}
			else
				mMoveDirection += Physics.gravity * mMovementData.gravityMultiplier * Time.fixedDeltaTime;

			Vector3 oldPos = transform.position;
			mController.Move(mMoveDirection * Time.fixedDeltaTime);

			if (Vector3.Distance(oldPos, transform.position) < 0.1f)
				mIsRunning = false;
		}

		// TODO: Make this private again
		public void ApplyOptionsData(IOptionsData settings)
		{
			mMouseSensitivity = settings.mouseSensitivity;
		}
	}
}
