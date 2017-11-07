using System.Collections;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Core.Input;
using FiringSquad.Core.SaveLoad;
using FiringSquad.Data;
using FiringSquad.Gameplay.UI;
using KeatsLib.Collections;
using KeatsLib.Unity;
using UnityEngine;
using Input = UnityEngine.Input;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Component strictly for handling the movement of the character
	/// object through the game world.
	/// </summary>
	/// <inheritdoc />
	public class CltPlayerMovementScript : MonoBehaviour
	{
		[SerializeField] private CharacterMovementData mMovementData;

		private CapsuleCollider mCollider;
		private CharacterController mController;
		private new Transform transform { get { return mController.transform; } }

		private CltPlayer mPlayer;
		private CltPlayerLocal mLocalPlayer;
		private IAudioReference mWalkingSound;

		private PlayerInputMap mInputBindings;
		private Vector2 mInput;
		private Vector3 mMoveDirection;
		private Vector2 mRotationAmount;
		private float mMouseSensitivity;
		private float mRotationY;
		private bool mJump, mIsJumping, mIsRunning, mPreviouslyGrounded, mCrouching;

		private float mSmoothedRecoil;
		private float mStandingHeight;
		private float mStandingRadius;

		private Coroutine mZoomInRoutine;
		private Camera mRealCameraRef;

		private void Awake()
		{
			mMoveDirection = Vector3.zero;
			mMouseSensitivity = 1.0f;
			mSmoothedRecoil = 0.0f;
		}

		private void Start()
		{
			mPlayer = GetComponentInParent<CltPlayer>();
			mLocalPlayer = mPlayer.GetComponentInChildren<CltPlayerLocal>();
			mCollider = mPlayer.GetComponent<CapsuleCollider>();
			mController = mPlayer.GetComponent<CharacterController>();
			mStandingHeight = mCollider.height;
			mStandingRadius = mCollider.radius;

			PlayerInputMap input = GetComponent<CltPlayerLocal>().inputMap;

			ServiceLocator.Get<IInput>()
				.RegisterAxis(Input.GetAxis, input.moveSidewaysAxis, INPUT_LeftRightMovement, InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, input.moveBackFrontAxis, INPUT_ForwardBackMovement, InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, input.lookLeftRightAxis, INPUT_LookHorizontal, InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, input.lookUpDownAxis, INPUT_LookVertical, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, input.jumpButton, INPUT_Jump, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, input.crouchButton, INPUT_CrouchStart, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, input.crouchButton, INPUT_CrouchStop, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, input.sprintButton, INPUT_SprintStart, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, input.sprintButton, INPUT_SprintStop, InputLevel.Gameplay)
				.EnableInputLevel(InputLevel.Gameplay);

			EventManager.Local.OnApplyOptionsData += ApplyOptionsData;
			EventManager.Local.OnEnterAimDownSightsMode += OnEnterAimDownSightsMode;
			EventManager.Local.OnExitAimDownSightsMode += OnExitAimDownSightsMode;
			mInputBindings = input;
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(INPUT_Jump)
				.UnregisterInput(INPUT_CrouchStart)
				.UnregisterInput(INPUT_CrouchStop)
				.UnregisterAxis(INPUT_ForwardBackMovement)
				.UnregisterAxis(INPUT_LeftRightMovement)
				.UnregisterAxis(INPUT_LookHorizontal)
				.UnregisterAxis(INPUT_LookVertical);

			EventManager.Local.OnApplyOptionsData -= ApplyOptionsData;
			EventManager.Local.OnEnterAimDownSightsMode -= OnEnterAimDownSightsMode;
			EventManager.Local.OnExitAimDownSightsMode -= OnExitAimDownSightsMode;
		}

		#region Input Delegates

		private void INPUT_ForwardBackMovement(float val)
		{
			mInput.y = val;
			HandleMovementSound();
		}

		private void INPUT_LeftRightMovement(float val)
		{
			mInput.x = val;
			HandleMovementSound();
		}

		private void HandleMovementSound()
		{
			IAudioManager audioService = ServiceLocator.Get<IAudioManager>();
			mWalkingSound = audioService.CheckReferenceAlive(ref mWalkingSound);

			if (mWalkingSound == null)
			{
				if (mInput.magnitude < 0.11f)
					return;

				mWalkingSound = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.LoopWalking, mPlayer.transform, false);
				mWalkingSound.AttachToRigidbody(mController.GetComponent<Rigidbody>());
				mWalkingSound.playerSpeed = mInput.magnitude;
				mWalkingSound.Start();
			}
			else
				mWalkingSound.playerSpeed = mInput.magnitude;
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

		private void Update()
		{
			HandleRotation();
			UpdateCrouch();

			if (!mPreviouslyGrounded && mController.isGrounded)
			{
				mMoveDirection.y = 0.0f;

				// play landing sound
				mPlayer.localAnimator.ResetTrigger("Jump");
				mPlayer.localAnimator.SetTrigger("Land");
				mPlayer.networkAnimator.SetTrigger("Land");

				mJump = false;
				mIsJumping = false;
			}
			if (!mController.isGrounded && !mIsJumping && mPreviouslyGrounded)
				mMoveDirection.y = 0.0f;
			mPreviouslyGrounded = mController.isGrounded;

			UpdateAnimatorState();
		}

		private void FixedUpdate()
		{
			ApplyMovementForce();
		}

		/// <summary>
		/// Follow the mouse or joystick rotation.
		/// Horizontal rotation is applied to this.transform.
		/// Vertical rotation is only applied to Camera.main.transform.
		/// </summary>
		private void HandleRotation()
		{
			float speed = mMovementData.lookSpeed * mMouseSensitivity;
			if (mLocalPlayer.inAimDownSightsMode)
				speed *= mMovementData.aimDownSightsLookMultiplier;

			Vector2 rotation = mRotationAmount * speed;
			transform.RotateAround(transform.position, transform.up, rotation.x);

			mRotationY += rotation.y; // + (mRecoilAmount * Time.deltaTime);
			mRotationY = GenericExt.ClampAngle(mRotationY, -85.0f, 85.0f);

			float realRotation = mRotationY;
			if (mPlayer.weapon != null)
			{
				mSmoothedRecoil = Mathf.Lerp(mPlayer.weapon.GetCurrentRecoil(), mSmoothedRecoil, 0.4f);
				realRotation += mSmoothedRecoil;
			}

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
			AnimationUtility.SetVariable(mPlayer.localAnimator, "Crouch", mCrouching);
		}

		/// <summary>
		/// Apply movement based on the input we received this frame.
		/// </summary>
		private void ApplyMovementForce()
		{
			Vector3 desiredMove = transform.forward * mInput.y + transform.right * mInput.x;

			RaycastHit hitInfo;
			Physics.SphereCast(transform.position, mController.radius, Vector3.down, out hitInfo, mController.height / 2.0f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
			desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

			float speed = mMovementData.speed;
			if (mIsRunning)
				speed *= mMovementData.sprintMultiplier;
			if (mCrouching)
				speed *= mMovementData.crouchMoveMultiplier;
			if (mLocalPlayer.inAimDownSightsMode)
				speed *= mMovementData.aimDownSightsMoveMultiplier;

			mMoveDirection.x = desiredMove.x * speed;
			mMoveDirection.z = desiredMove.z * speed;

			if (mController.isGrounded)
			{
				mMoveDirection.y = -mMovementData.stickToGroundForce;

				if (mJump)
				{
					mMoveDirection.y = mMovementData.jumpForce;

					// play jump sound ?

					mPlayer.localAnimator.ResetTrigger("Land");
					mPlayer.localAnimator.SetTrigger("Jump");
					mPlayer.networkAnimator.SetTrigger("Jump");

					mJump = false;
					mIsJumping = true;
				}
			}
			else
				mMoveDirection += Physics.gravity * mMovementData.gravityMultiplier * Time.fixedDeltaTime;

			mController.Move(mMoveDirection * Time.fixedDeltaTime);
		}

		private void ApplyOptionsData(IOptionsData settings)
		{
			mMouseSensitivity = settings.mouseSensitivity;
		}

		private void OnEnterAimDownSightsMode()
		{
			mRealCameraRef = mRealCameraRef ?? mPlayer.eye.GetComponentInChildren<Camera>();

			if (mZoomInRoutine != null)
				StopCoroutine(mZoomInRoutine);

			mZoomInRoutine = StartCoroutine(ZoomCameraFov(25.0f, 0.25f));
		}

		private void OnExitAimDownSightsMode()
		{
			IOptionsData settings = ServiceLocator.Get<ISaveLoadManager>()
				.persistentData.GetOptionsData(PauseGamePanel.SETTINGS_ID);
			float fov = settings == null ? 60 : settings.fieldOfView;

			if (mZoomInRoutine != null)
				StopCoroutine(mZoomInRoutine);

			mZoomInRoutine = StartCoroutine(ZoomCameraFov(fov, 0.25f));
		}

		private IEnumerator ZoomCameraFov(float newFov, float time)
		{
			float currentTime = 0.0f;
			float startFov = mRealCameraRef.fieldOfView;

			while (currentTime < time)
			{
				mRealCameraRef.fieldOfView = Mathf.Lerp(startFov, newFov, currentTime / time);

				currentTime += Time.deltaTime;
				yield return null;
			}

			mRealCameraRef.fieldOfView = newFov;
		}
	}
}
