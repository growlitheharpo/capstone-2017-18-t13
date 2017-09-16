using KeatsLib;
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
	public class PlayerMovementScript : MonoBehaviour
	{
		[SerializeField] private CharacterMovementData mMovementData;
		[SerializeField] private LayerMask mJumpLayermask;

		private CapsuleCollider mCollider;
		private Transform mMainCameraRef;
		private Rigidbody mRigidbody;

		private Vector3 mCumulativeMovement;
		private Vector2 mRotationAmount;
		private float mRecoilAmount;
		private bool mJump, mCrouching;

		private const float STANDING_HEIGHT = 3.0f;
		private const float STANDING_RADIUS = 0.75f;
		private const float DOWNFORCE_MULT = 2.5f;

		private void Awake()
		{
			mCollider = GetComponent<CapsuleCollider>();
			mRigidbody = GetComponent<Rigidbody>();
			mMainCameraRef = Camera.main.transform;
			mRecoilAmount = 0.0f;
		}

		private void Start()
		{
			ServiceLocator.Get<IInput>()
				.RegisterAxis(Input.GetAxis, "Horizontal", INPUT_LeftRightMovement, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, "Vertical", INPUT_ForwardBackMovement, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, "Mouse X", INPUT_LookHorizontal, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, "Mouse Y", INPUT_LookVertical, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, "J1_RightStickH", INPUT_LookHorizontal, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, "J1_RightStickV", INPUT_LookVertical, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, "Jump", INPUT_Jump, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, "Crouch", INPUT_CrouchStart, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonUp, "Crouch", INPUT_CrouchStop, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);
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
		}

		#region Input Delegates

		private void INPUT_ForwardBackMovement(float val)
		{
			mCumulativeMovement += transform.forward * mMovementData.forwardSpeed * val;
		}

		private void INPUT_LeftRightMovement(float val)
		{
			mCumulativeMovement += transform.right * val * mMovementData.strafeSpeed;
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
			Ray r = new Ray(transform.position + Vector3.up * 0.5f, Vector3.up * -1.0f);
			const float dist = 0.51f;

			UnityEngine.Debug.DrawLine(r.origin, r.origin + r.direction * dist, Color.green, 0.5f);

			if (Physics.Raycast(r, dist, mJumpLayermask))
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

		#endregion

		private void Update()
		{
			HandleRotation();
			UpdateCrouch();
		}

		private void FixedUpdate()
		{
			ApplyMovementForce();
		}

		private float mRotationY;

		/// <summary>
		/// Follow the mouse or joystick rotation.
		/// Horizontal rotation is applied to this.transform.
		/// Vertical rotation is only applied to Camera.main.transform.
		/// </summary>
		private void HandleRotation()
		{
			Vector2 rotation = mRotationAmount * mMovementData.lookSpeed;
			transform.RotateAround(transform.position, transform.up, rotation.x);

			mRotationY += rotation.y + mRecoilAmount;

			mRotationY = GenericExt.ClampAngle(mRotationY, -85.0f, 85.0f);
			mMainCameraRef.localRotation = Quaternion.AngleAxis(mRotationY, Vector3.left);

			mRotationAmount = Vector2.zero;
			mRecoilAmount = Mathf.Lerp(mRecoilAmount, 0.0f, Time.deltaTime * 20.0f);
		}

		/// <summary>
		/// Squish or stretch our collider based on the crouch state.
		/// </summary>
		private void UpdateCrouch()
		{
			float current = mCollider.height;
			mCollider.height = Mathf.Lerp(current, mCrouching ? STANDING_HEIGHT * mMovementData.crouchHeight : STANDING_HEIGHT, Time.deltaTime * mMovementData.crouchSpeed);
			current = mCollider.radius;
			mCollider.radius = Mathf.Lerp(current, mCrouching ? STANDING_RADIUS * mMovementData.crouchHeight : STANDING_RADIUS, Time.deltaTime * mMovementData.crouchSpeed);
		}

		/// <summary>
		/// Apply movement based on the input we received this frame.
		/// </summary>
		private void ApplyMovementForce()
		{
			Vector3 movement = mCumulativeMovement.ClampMagnitude(mMovementData.forwardSpeed * Time.deltaTime);
			mRigidbody.AddForce(movement, ForceMode.Acceleration);

			if (mJump)
			{
				mRigidbody.AddForce(Vector3.up * mMovementData.jumpForce, ForceMode.Impulse);
				mJump = false;
			}
			else if (mRigidbody.velocity.y < 0.0f)
				mRigidbody.AddForce(Vector3.down * mMovementData.jumpForce * DOWNFORCE_MULT, ForceMode.Force);

			mCumulativeMovement = Vector3.zero;
		}

		public void AddRecoil(Vector3 direction, float amount)
		{
			mRecoilAmount = amount;
		}
	}
}
