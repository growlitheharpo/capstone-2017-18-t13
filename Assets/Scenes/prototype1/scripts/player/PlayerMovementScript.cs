using UnityEngine;

namespace Prototype1
{
	public class PlayerMovementScript : MonoBehaviour
	{
		[SerializeField] private CharacterMovementData mMovementData;
		private CharacterController mController;
		private float mX1, mX2, mY1, mY2;

		private void Start()
		{
			//SHOULD NOT BE HERE
			ServiceLocator.Get<IInput>().EnableInputLevel(KeatsLib.Unity.Input.InputLevel.Gameplay);

			ServiceLocator.Get<IInput>()
				.RegisterAxis(Input.GetAxis, "J1_LeftStickH", INPUT_MoveHorizontal, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, "J1_LeftStickV", INPUT_MoveVertical, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, "J1_RightStickH", INPUT_LookHorizontal, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, "J1_RightStickV", INPUT_LookVertical, KeatsLib.Unity.Input.InputLevel.Gameplay);

			mController = GetComponent<CharacterController>();
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterAxis(INPUT_MoveHorizontal)
				.UnregisterAxis(INPUT_MoveVertical)
				.UnregisterAxis(INPUT_LookHorizontal)
				.UnregisterAxis(INPUT_LookVertical);
		}

		private void INPUT_MoveHorizontal(float value)
		{
			mX1 = value;
		}

		private void INPUT_MoveVertical(float value)
		{
			mY1 = value;
		}

		private void INPUT_LookHorizontal(float value)
		{
			mX2 = value;
		}

		private void INPUT_LookVertical(float value)
		{
			mY2 = value;
		}

		private void LateUpdate()
		{
			MoveAround();
			LookTowardsStick();
		}

		private void MoveAround()
		{
			Vector2 input = new Vector2(mX1, mY1);
			input *= input.magnitude;

			Vector3 movementVector = new Vector3(input.x, 0.0f, input.y) * mMovementData.forwardSpeed * Time.deltaTime;
			mController.Move(movementVector);
		}

		private void LookTowardsStick()
		{
			float angle = Mathf.Atan2(mX2, mY2) * Mathf.Rad2Deg;
			Quaternion currentRot = transform.rotation;
			Quaternion newRot = Quaternion.Euler(transform.eulerAngles.x, angle, transform.eulerAngles.z);
			float newAngleWeight = mX2 * mX2 + mY2 * mY2;

			transform.rotation = Quaternion.Slerp(currentRot, newRot, (newAngleWeight * newAngleWeight) * mMovementData.lookSpeed);
		}
	}
}
