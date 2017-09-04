using System.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

public class PlayerWeaponScript : MonoBehaviour
{
	[SerializeField] private float mBumperMultiplier;
	[SerializeField] private GameObject mGunObject;
	[SerializeField] private GameObject mCanvas;
	[SerializeField] private UIImage mTmpImage;
	private float mLeftInput, mRightInput;

	private float mSliderVelocity;
	private float mSliderVal;

	// Use this for initialization
	private void Start()
	{
		ServiceLocator.Get<IInput>()
			.RegisterAxis(Input.GetAxis, "J1_LeftBumper", INPUT_LeftBumperInput, KeatsLib.Unity.Input.InputLevel.Gameplay)
			.RegisterAxis(Input.GetAxis, "J1_RightBumper", INPUT_RightBumperInput, KeatsLib.Unity.Input.InputLevel.Gameplay);

		StartCoroutine(ChooseVelocity());
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
	}

	private void LateUpdate()
	{
		float l = mLeftInput / 2.0f + 1.0f, r = mRightInput / 2.0f + 1.0f;
		mSliderVal += (l - r) / 2.0f * Time.deltaTime * mBumperMultiplier;

		mSliderVal += mSliderVelocity * Time.deltaTime;
	}

	private IEnumerator ChooseVelocity()
	{
		while (true)
		{
			float currentTime = 0.0f;
			float targetTime = Random.Range(0.5f, 1.2f);
			float targetVel = Random.Range(-0.6f, 0.6f);

			while (currentTime < targetTime)
			{
				mSliderVelocity = Mathf.Lerp(mSliderVelocity, targetVel, currentTime / targetTime);
				currentTime += Time.deltaTime;
			}

			yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
		}
	}
}
