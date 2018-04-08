using System.Collections;
using FiringSquad.Core;
using FiringSquad.Core.Audio;
using FiringSquad.Core.Input;
using FiringSquad.Data;
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
		/// Inspector variables
		[SerializeField] private CharacterMovementData mMovementData;

		/// Private variables
		private CharacterController mController;

		private CltPlayer mPlayer;
		private CltPlayerLocal mLocalPlayer;
		private IAudioReference mWalkingSound;
		private IAudioReference mJumpSound;
		private IAudioReference mLandSound;
		private Coroutine mZoomInRoutine;
		private Camera mRealCameraRef;

		private IOptionsData mPlayerOptions;
		private PlayerInputMap mInputBindings;
		private Vector3 mMoveDirection;
		private Vector2 mInput, mRotationAmount;
		private float mRotationY;
		private float mSmoothedRecoil, mStandingHeight, mStandingRadius;
		private bool mJump, mIsJumping, mIsRunning, mPreviouslyGrounded, mCrouching;

		/// Float for the speed
		private float mCurrentSpeed;
		/// Variables to keep track of previous x and y
		private float mPrevXInput, mPrevYInput;

		private const float DEFAULT_FIELD_OF_VIEW = 60.0f;

		/// <summary> Cover up Unity's "transform" component with the one of our CltPlayer. </summary>
		private new Transform transform { get { return mController.transform; } }

		private float mSpeedMultiplier, mGravMultiplier, mJumpMultiplier;


		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mMoveDirection = Vector3.zero;
			mSmoothedRecoil = 0.0f;
		}

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			// Grab all the references we need to work.
			mPlayer = GetComponentInParent<CltPlayer>();
			mLocalPlayer = mPlayer.GetComponentInChildren<CltPlayerLocal>();
			mController = mPlayer.GetComponent<CharacterController>();
			mStandingHeight = mController.height;
			mStandingRadius = mController.radius;

			// Initialize the speed
			mCurrentSpeed = 0.0f;

			mGravMultiplier = 1;
			mSpeedMultiplier = 1;
			mJumpMultiplier = 1;

			// Register all of the movement input
			mInputBindings = GetComponent<CltPlayerLocal>().inputMap;
			ServiceLocator.Get<IInput>()
				.RegisterAxis(Input.GetAxis, mInputBindings.moveSidewaysAxis, INPUT_LeftRightMovement, InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, mInputBindings.moveBackFrontAxis, INPUT_ForwardBackMovement, InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, mInputBindings.lookLeftRightAxis, INPUT_LookHorizontal, InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, mInputBindings.lookUpDownAxis, INPUT_LookVertical, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, mInputBindings.jumpButton, INPUT_Jump, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, mInputBindings.crouchButton, INPUT_CrouchStart, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, mInputBindings.crouchButton, INPUT_CrouchStop, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, mInputBindings.sprintButton, INPUT_SprintStart, InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, mInputBindings.sprintButton, INPUT_SprintStop, InputLevel.Gameplay);

			// Register for local events.
			EventManager.Local.OnApplyOptionsData += ApplyOptionsData;
			EventManager.Local.OnLocalPlayerDied += OnLocalPlayerDied;
			EventManager.LocalGUI.OnRequestNewFieldOfView += OnRequestNewFieldOfView;
			EventManager.Local.OnEquipRocketGrip += OnEquipRocketGrip;
			EventManager.Local.OnUnequipRocketGrip += OnUnequipRocketGrip;
		}

		/// <summary>
		/// Clean up all listeners and event handlers.
		/// </summary>
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
			EventManager.Local.OnLocalPlayerDied -= OnLocalPlayerDied;
			EventManager.LocalGUI.OnRequestNewFieldOfView -= OnRequestNewFieldOfView;
			EventManager.Local.OnEquipRocketGrip -= OnEquipRocketGrip;
			EventManager.Local.OnUnequipRocketGrip -= OnUnequipRocketGrip;
		}

		#region Input Delegates

		/// <summary>
		/// INPUT HANDLER: Handle back/forward movement.
		/// </summary>
		private void INPUT_ForwardBackMovement(float val)
		{
			mInput.y = val;
			HandleMovementSound();
		}

		/// <summary>
		/// INPUT HANDLER: Handle left/right movement.
		/// </summary>
		private void INPUT_LeftRightMovement(float val)
		{
			mInput.x = val;
			HandleMovementSound();
		}

		/// <summary>
		/// Update our movement sound based on the current input state.
		/// </summary>
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

		/// <summary>
		/// INPUT HANDLER: Handle mouse movement for looking horizontally.
		/// </summary>
		private void INPUT_LookHorizontal(float val)
		{
			mRotationAmount.x += val;
		}

		/// <summary>
		/// INPUT HANDLER: Handle mouse movement for looking vertically.
		/// </summary>
		private void INPUT_LookVertical(float val)
		{
			mRotationAmount.y += val;
		}

		/// <summary>
		/// INPUT HANDLER: Handle a jump command.
		/// </summary>
		private void INPUT_Jump()
		{
			mJump = true;
		}

		/// <summary>
		/// INPUT HANDLER: Handle a start crouch command.
		/// </summary>
		private void INPUT_CrouchStart()
		{
			mCrouching = true;
		}

		/// <summary>
		/// INPUT HANDLER: Handle a stop crouch command.
		/// </summary>
		private void INPUT_CrouchStop()
		{
			mCrouching = false;
		}

		/// <summary>
		/// INPUT HANDLER: Handle a start sprint command.
		/// #NotThisSprint
		/// </summary>
		private void INPUT_SprintStart()
		{
			mIsRunning = true;
		}

		/// <summary>
		/// INPUT HANDLER: Handle a stop sprint command.
		/// #NotThisSprint
		/// </summary>
		private void INPUT_SprintStop()
		{
			if (!mInputBindings.stickySprint)
				mIsRunning = false;
		}

		#endregion

		/// <summary>
		/// Unity's Update function.
		/// </summary>
		private void Update()
		{
			HandleRotation();
			UpdateCrouch();

			if (!mPreviouslyGrounded && mController.isGrounded)
			{
				mMoveDirection.y = 0.0f;

				// play landing sound
				IAudioManager audioService = ServiceLocator.Get<IAudioManager>();
				mLandSound = audioService.CheckReferenceAlive(ref mLandSound);

				if (mLandSound == null)
				{
					mLandSound = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.Land, mPlayer.transform, false);
					mLandSound.AttachToRigidbody(mController.GetComponent<Rigidbody>());
					mLandSound.Start();
				}
				
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

		/// <summary>
		/// Unity's FixedUpdate function.
		/// </summary>
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
			float sensitivity = mPlayerOptions != null ? mPlayerOptions.mouseSensitivity : 1.0f;
			float speed = mMovementData.lookSpeed * sensitivity;
			if (mLocalPlayer.inAimDownSightsMode)
				speed *= mMovementData.aimDownSightsLookMultiplier;

			Vector2 rotation = mRotationAmount * speed;
			transform.RotateAround(transform.position, transform.up, rotation.x);

			mRotationY += rotation.y;
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
		/// TODO: This entire function needs to be re-written. It's *awful* and not networked!!
		/// </summary>
		private void UpdateCrouch()
		{
			float currentHeight = mController.height;
			float newHeight = Mathf.Lerp(currentHeight, mCrouching ? mStandingHeight * mMovementData.crouchHeight : mStandingHeight, Time.deltaTime * mMovementData.crouchSpeed);
			mController.height = newHeight;

			float currentRadius = mController.radius;
			float newRadius = Mathf.Lerp(currentRadius, mCrouching ? mStandingRadius * mMovementData.crouchHeight : mStandingRadius, Time.deltaTime * mMovementData.crouchSpeed);
			mController.radius = newRadius;
		}

		/// <summary>
		/// Update whether or not we are crouching on the local animator.
		/// </summary>
		private void UpdateAnimatorState()
		{
			AnimationUtility.SetVariable(mPlayer.localAnimator, "Crouch", mCrouching);
		}

		/// <summary>
		/// Apply movement based on the input we received this frame.
		/// Based on the controller in Unity's standard input, but modified (you might even say re-modded) to fit our needs.
		/// </summary>
		private void ApplyMovementForce()
		{
			Vector3 desiredMove = transform.forward * mInput.y + transform.right * mInput.x;

			if (mInput.y != 0 || mInput.x != 0)
			{
				mPrevXInput = mInput.x;
				mPrevYInput = mInput.y;
			}

			// Check if these are equal to 0
			if (mInput.x == 0.0f && mInput.y == 0.0f)
			{
				// If there is still momentum
				if (mCurrentSpeed > 0.0f)
				{
					desiredMove = transform.forward * mPrevYInput + transform.right * mPrevXInput;
				}
				// Else there is none
				else
				{
					mPrevXInput = 0;
					mPrevYInput = 0;
				}
			}

			// Cast around us to check the plane we should move on.
			RaycastHit hitInfo;
			Physics.SphereCast(transform.position, mController.radius, Vector3.down, out hitInfo, mController.height / 2.0f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
			desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

			// Temp var for desired speed
			float desiredSpeed = 0.0f;

			if (mInput.y != 0 || mInput.x != 0)
			{
				desiredSpeed = mMovementData.speed * mSpeedMultiplier;
			}
			else if (mInput.y == 0.0f && mInput.x == 0.0f)
			{
				desiredSpeed = 0.0f;
			}

			float speed = 0.0f;

			// Check for lerp speed
			if (desiredSpeed > 0)
			{
				speed = Mathf.Lerp(mCurrentSpeed, desiredSpeed, 1.0f * Time.deltaTime * mMovementData.accelerationLerpSpeed);
			}
			else
			{
				speed = Mathf.Lerp(mCurrentSpeed, desiredSpeed, 1.0f * Time.deltaTime * mMovementData.decelerationLerpSpeed);
			}


			if (speed > mMovementData.speed * mSpeedMultiplier)
			{
				speed = mMovementData.speed * mSpeedMultiplier;
			}
			//UnityEngine.Debug.Log(mCurrentSpeed);
			//float speed = mMovementData.speed;
			//speed = Mathf.Lerp(mCurrentSpeed, mMovementData.speed, 0.1f);

			if (mIsRunning && mInput.x == 0 && mInput.y != 0) // Also make sure they're only going forward
				speed *= mMovementData.sprintMultiplier;
			if (mCrouching)
				speed *= mMovementData.crouchMoveMultiplier;
			if (mLocalPlayer.inAimDownSightsMode)
				speed *= 3 * mMovementData.aimDownSightsMoveMultiplier;

			mMoveDirection.x = desiredMove.x * speed;
			mMoveDirection.z = desiredMove.z * speed;

			if (mController.isGrounded)
			{
				mMoveDirection.y = -mMovementData.stickToGroundForce;

				if (mJump)
				{
					mMoveDirection.y = mMovementData.jumpForce * mJumpMultiplier;

					// play jump sound
					IAudioManager audioService = ServiceLocator.Get<IAudioManager>();
					mJumpSound = audioService.CheckReferenceAlive(ref mJumpSound);

					if (mJumpSound == null)
					{
						mJumpSound = ServiceLocator.Get<IAudioManager>().CreateSound(AudioEvent.Jump, mPlayer.transform, false);
						mJumpSound.AttachToRigidbody(mController.GetComponent<Rigidbody>());
						mJumpSound.Start();
					}
					
					mPlayer.localAnimator.ResetTrigger("Land");
					mPlayer.localAnimator.SetTrigger("Jump");
					mPlayer.networkAnimator.SetTrigger("Jump");

					mIsJumping = true;
				}
			}

			// Apply new air movement stuff
			else
			{


				mMoveDirection += Physics.gravity * mMovementData.gravityMultiplier * mGravMultiplier * Time.fixedDeltaTime;

			}

			mJump = false;
			mController.Move(mMoveDirection * Time.fixedDeltaTime);

			mCurrentSpeed = speed;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnApplyOptionsData
		/// </summary>
		private void ApplyOptionsData(IOptionsData settings)
		{
			mPlayerOptions = settings;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerDied
		/// </summary>
		private void OnLocalPlayerDied(PlayerKill killInfo, ICharacter killer)
		{
			mRotationY = 0.0f;
			mCurrentSpeed = 0.0f;
			mIsJumping = false;
			mIsRunning = false;
			mCrouching = false;
		}
		
		/// <summary>
		/// EVENT HANDLER: Local.OnRequestNewFieldOfView
		/// </summary>
		private void OnRequestNewFieldOfView(float fov, float time)
		{
			mRealCameraRef = mRealCameraRef ?? mPlayer.eye.GetComponentInChildren<Camera>();
			
			if (mZoomInRoutine != null)
				StopCoroutine(mZoomInRoutine);

			if (fov < 0.0f)
				fov = mPlayerOptions != null ? mPlayerOptions.fieldOfView : DEFAULT_FIELD_OF_VIEW;

			if (time > 0.0f)
				mZoomInRoutine = StartCoroutine(ZoomCameraFov(fov, 0.25f));
			else
				mRealCameraRef.fieldOfView = fov;
		}

		/// <summary>
		/// Lerp the main camera's FOV
		/// </summary>
		/// <param name="newFov">The new target field of view.</param>
		/// <param name="time">The length of time (in seconds) to lerp over.</param>
		private IEnumerator ZoomCameraFov(float newFov, float time)
		{
			mRealCameraRef = mRealCameraRef ?? mPlayer.eye.GetComponentInChildren<Camera>();

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

		/// <summary>
		/// Event handler for equipping the rocket grip
		/// </summary>
		private void OnEquipRocketGrip()
		{
			mGravMultiplier = mMovementData.rocketGripDescentMultiplier;
			mSpeedMultiplier = mMovementData.rocketGripSpeed;
			mJumpMultiplier = mMovementData.rocketJumpForce;
		}

		/// <summary>
		/// Event handler for unequipping the rocket grip
		/// </summary>
		private void OnUnequipRocketGrip()
		{
			mGravMultiplier = 1;
			mSpeedMultiplier = 1;
			mJumpMultiplier = 1;
		}
	}
}
