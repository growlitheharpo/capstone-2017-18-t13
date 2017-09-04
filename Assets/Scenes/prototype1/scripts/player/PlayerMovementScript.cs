using UnityEngine;

namespace Prototype1
{
	public class PlayerMovementScript : MonoBehaviour
	{
		[SerializeField] private CharacterMovementData mMovementData;
		private CharacterController mController;
		private float x1, x2, y1, y2;

		// Use this for initialization
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
			x1 = value;
		}

		private void INPUT_MoveVertical(float value)
		{
			y1 = value;
		}

		private void INPUT_LookHorizontal(float value)
		{
			x2 = value;
		}

		private void INPUT_LookVertical(float value)
		{
			y2 = value;
		}

		private void LateUpdate()
		{
			MoveAround();
			LookTowardsStick();
		}

		private void MoveAround()
		{
			Vector2 input = new Vector2(x1, y1);

			input *= input.magnitude;

			Vector3 movementVector = new Vector3(input.x, 0.0f, input.y) * mMovementData.forwardSpeed * Time.deltaTime;
			mController.Move(movementVector);
		}

		private void LookTowardsStick()
		{
			float angle = Mathf.Atan2(x2, y2) * Mathf.Rad2Deg;
			Quaternion currentRot = transform.rotation;
			Quaternion newRot = Quaternion.Euler(transform.eulerAngles.x, angle, transform.eulerAngles.z);
			float newAngleWeight = x2 * x2 + y2 * y2;

			transform.rotation = Quaternion.Slerp(currentRot, newRot, (newAngleWeight * newAngleWeight) * mMovementData.lookSpeed);
		}
	}
}
