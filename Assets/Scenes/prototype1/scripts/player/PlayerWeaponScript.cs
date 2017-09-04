using UnityEngine;
using UIImage = UnityEngine.UI.Image;

public class PlayerWeaponScript : MonoBehaviour
{
	[SerializeField] private float mBumperMultiplier;
	[SerializeField] private Transform mGunObject;
	[SerializeField] private GameObject mCanvas;
	[SerializeField] private UIImage mTmpImage;
	[SerializeField] private float mSliderTipWeight;
	[SerializeField] private float mGunMaxRotationRate = 360.0f;
	[SerializeField] private float mRotationGrowRate = 1.0f;

	private float mLeftInput, mRightInput;
	private float mSliderVelocity;
	private float mSliderVal;
	private float mGunCharge;

	// Use this for initialization
	private void Start()
	{
		ServiceLocator.Get<IInput>()
			.RegisterAxis(Input.GetAxis, "J1_LeftBumper", INPUT_LeftBumperInput, KeatsLib.Unity.Input.InputLevel.Gameplay)
			.RegisterAxis(Input.GetAxis, "J1_RightBumper", INPUT_RightBumperInput, KeatsLib.Unity.Input.InputLevel.Gameplay);
	}

	private void INPUT_LeftBumperInput(float val)
	{
		mLeftInput = val;
	}

	private void INPUT_RightBumperInput(float val)
	{
		mRightInput = val;
	}

	private void Update()
	{
		mSliderVal = Mathf.Clamp(mSliderVal, -0.5f, 0.5f);
		Vector3 pos = mTmpImage.transform.localPosition;
		pos.x = mSliderVal;
		mTmpImage.transform.localPosition = pos;

		mCanvas.transform.LookAt(Camera.main.transform, Vector3.down);

		float chargeChange = (0.5f - Mathf.Abs(mSliderVal)) * 4.0f - 1.0f;
		mGunCharge += chargeChange * mRotationGrowRate * Time.deltaTime;
		mGunCharge = Mathf.Clamp(mGunCharge, 0.0f, 1.0f);
		mGunObject.Rotate(mGunObject.right, mGunMaxRotationRate * mGunCharge * Time.deltaTime, Space.World);
	}

	private void LateUpdate()
	{
		float l = mLeftInput / 2.0f + 1.0f, r = mRightInput / 2.0f + 1.0f;
		mSliderVal += (l - r) / 2.0f * Time.deltaTime * mBumperMultiplier;

		mSliderVal += mSliderVelocity * Time.deltaTime;
		mSliderVelocity = mSliderVal * mSliderTipWeight;
	}
}
