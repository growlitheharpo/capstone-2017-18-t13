using KeatsLib.Unity;
using UnityEngine;
using Input = UnityEngine.Input;

/// <summary>
/// Script for binding first-person player movement to the Input
/// system and reflecting it in a character controller.
/// </summary>
public class PlayerMovementScript : MonoBehaviour
{
	[SerializeField] private CharacterMovementData mMovementData;
	private CharacterController mBaseController;
	private Transform mMainCameraRef;

	private Vector3 mCumulativeMovement;
	private Vector2 mRotationAmount;

	private void Awake()
	{
		mBaseController = GetComponent<CharacterController>();
	}

	private void Start()
	{
		mMainCameraRef = Camera.main.transform;
		ServiceLocator.Get<IInput>()
			.RegisterInput(Input.GetKey, KeyCode.W, INPUT_MoveForward, KeatsLib.Unity.Input.InputLevel.Gameplay)
			.RegisterInput(Input.GetKey, KeyCode.A, INPUT_MoveLeft, KeatsLib.Unity.Input.InputLevel.Gameplay)
			.RegisterInput(Input.GetKey, KeyCode.S, INPUT_MoveBackward, KeatsLib.Unity.Input.InputLevel.Gameplay)
			.RegisterInput(Input.GetKey, KeyCode.D, INPUT_MoveRight, KeatsLib.Unity.Input.InputLevel.Gameplay)
			.RegisterAxis(Input.GetAxis, "Mouse X", INPUT_LookHorizontal, KeatsLib.Unity.Input.InputLevel.Gameplay)
			.RegisterAxis(Input.GetAxis, "Mouse Y", INPUT_LookVertical, KeatsLib.Unity.Input.InputLevel.Gameplay);

	}

	private void OnDestroy()
	{
		ServiceLocator.Get<IInput>()
			.UnregisterInput(INPUT_MoveForward)
			.UnregisterInput(INPUT_MoveLeft)
			.UnregisterInput(INPUT_MoveBackward)
			.UnregisterInput(INPUT_MoveRight)
			.UnregisterAxis(INPUT_LookHorizontal)
			.UnregisterAxis(INPUT_LookVertical);
	}

	private void INPUT_MoveForward()
	{
		mCumulativeMovement += transform.forward * mMovementData.forwardSpeed;
	}
	
	private void INPUT_MoveBackward()
	{
		mCumulativeMovement += transform.forward * -1.0f * mMovementData.backwardSpeed;
	}
	
	private void INPUT_MoveLeft()
	{
		mCumulativeMovement += transform.right * -1.0f * mMovementData.strafeSpeed;
	}
	
	private void INPUT_MoveRight()
	{
		mCumulativeMovement += transform.right * mMovementData.strafeSpeed;
	}

	private void INPUT_LookHorizontal(float amount)
	{
		mRotationAmount.x += amount;
	}

	private void INPUT_LookVertical(float amount)
	{
		mRotationAmount.y += amount;
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
		mBaseController.SimpleMove(movement);
		mCumulativeMovement = Vector3.zero;
	}
}
