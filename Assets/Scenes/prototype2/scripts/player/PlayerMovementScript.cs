using KeatsLib.Unity;
using UnityEngine;
using Input = UnityEngine.Input;

namespace Prototype2
{
	public class PlayerMovementScript : MonoBehaviour
	{
		[SerializeField] private CharacterMovementData mMovementData;
		[SerializeField] private LayerMask mJumpLayermask;

		private Transform mMainCameraRef;
		private Rigidbody mRigidbody;

		private Vector3 mCumulativeMovement;
		private Vector2 mRotationAmount;
		private bool mJump;

		private const float DOWNFORCE_MULT = 2.5f;

		private void Awake()
		{
			mRigidbody = GetComponent<Rigidbody>();
			mMainCameraRef = Camera.main.transform;
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
				.EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(INPUT_Jump)
				.UnregisterAxis(INPUT_ForwardBackMovement)
				.UnregisterAxis(INPUT_LeftRightMovement)
				.UnregisterAxis(INPUT_LookHorizontal)
				.UnregisterAxis(INPUT_LookVertical);
		}

		private void INPUT_Jump()
		{
			Ray r = new Ray(transform.position + Vector3.up * 0.5f, Vector3.up * -1.0f);
			const float dist = 0.51f;

			Debug.DrawLine(r.origin, r.origin + r.direction * dist, Color.green, 0.5f);

			if (Physics.Raycast(r, dist, mJumpLayermask))
				mJump = true;
		}

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

		private void Update()
		{
			HandleRotation();
		}

		private void HandleRotation()
		{
			// horizontal rotation is applied to us
			// vertical rotation is applied only to the camera
			Vector2 rotation = mRotationAmount * mMovementData.lookSpeed;

			transform.RotateAround(transform.position, transform.up, rotation.x);
			mMainCameraRef.RotateAround(mMainCameraRef.position, mMainCameraRef.right, -rotation.y);

			mRotationAmount = Vector2.zero;
		}

		private void FixedUpdate()
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
	}
}
