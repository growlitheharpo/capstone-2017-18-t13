using KeatsLib.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Input = UnityEngine.Input;

namespace Prototype2
{
	public class PlayerMovementScript : MonoBehaviour
	{
		[SerializeField] private CharacterMovementData mMovementData;

		private CharacterController mController;
		private Transform mMainCameraRef;

		private Vector3 mCumulativeMovement;
		private Vector2 mRotationAmount;

		private void Awake()
		{
			mController = GetComponent<CharacterController>();
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
				.EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterAxis(INPUT_ForwardBackMovement)
				.UnregisterAxis(INPUT_LeftRightMovement)
				.UnregisterAxis(INPUT_LookHorizontal)
				.UnregisterAxis(INPUT_LookVertical);
		}

		private void INPUT_ForwardBackMovement(float val)
		{
			mCumulativeMovement += transform.forward * mMovementData.forwardSpeed * val;
		}

		private void INPUT_LeftRightMovement(float val)
		{
			mCumulativeMovement += transform.right * val * mMovementData.strafeSpeed;
		}

		private void INPUT_LookHorizontal(float val) { }
		private void INPUT_LookVertical(float val) { }

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
			mController.SimpleMove(movement);
			mCumulativeMovement = Vector3.zero;
		}
	}
}
