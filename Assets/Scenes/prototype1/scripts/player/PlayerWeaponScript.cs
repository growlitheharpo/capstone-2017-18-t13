using System;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace Prototype1
{
	public class PlayerWeaponScript : MonoBehaviour
	{
		private const float RAYCAST_LENGTH = 2500.0f;

		[SerializeField] private float mBumperMultiplier;
		[SerializeField] private Transform mGunObject;
		[SerializeField] private Transform mGunTipHint;
		[SerializeField] private GameObject mCanvas;
		[SerializeField] private UIImage mTmpImage;
		[SerializeField] private float mSliderTipWeight;
		[SerializeField] private float mGunMaxRotationRate = 360.0f;
		[SerializeField] private float mRotationGrowRate = 1.0f;
		[SerializeField] private LayerMask mGunHitMask;

		[ComponentTypeRestriction(typeof(IAudioProfile))] [SerializeField] private ScriptableObject mSfxProfile;

		private IAudioReference mSwishSound;
		private LineRenderer mLineRenderer;
		private float mLeftInput, mRightInput;
		private float mSliderVelocity;
		private float mSliderVal;
		private float mGunCharge;

		private void Awake()
		{
			mLineRenderer = GetComponent<LineRenderer>();
		}

		// Use this for initialization
		private void Start()
		{
			ServiceLocator.Get<IInput>()
				.RegisterAxis(Input.GetAxis, "J1_LeftBumper", INPUT_LeftBumperInput, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterAxis(Input.GetAxis, "J1_RightBumper", INPUT_RightBumperInput, KeatsLib.Unity.Input.InputLevel.Gameplay)
				.RegisterInput(Input.GetButtonDown, "J1_PrimaryButton", INPUT_FireWeapon, KeatsLib.Unity.Input.InputLevel.Gameplay);

			ServiceLocator.Get<IGameConsole>()
				.RegisterCommand("setvar", CONSOLE_SetVar);

			mSwishSound = ServiceLocator.Get<IAudioManager>()
				.PlaySound(AudioManager.AudioEvent.PrimaryEffect1, mSfxProfile as IAudioProfile, mGunObject);
		}

		#region Service Callbacks

		private void INPUT_LeftBumperInput(float val)
		{
			mLeftInput = val;
		}

		private void INPUT_RightBumperInput(float val)
		{
			mRightInput = val;
		}

		private void INPUT_FireWeapon()
		{
			ServiceLocator.Get<IAudioManager>()
				.PlaySound(AudioManager.AudioEvent.PrimaryEffect2, mSfxProfile as IAudioProfile, mGunObject);
			
			Vector3 start = transform.position + Vector3.up;
			RaycastHit hit;
			if (Physics.Raycast(new Ray(start, transform.forward), out hit, RAYCAST_LENGTH, mGunHitMask.value))
			{
				IDamageReceiver target = hit.transform.parent.GetComponent<IDamageReceiver>();
				target.ApplyDamage(mGunCharge * 100.0f);

				Debug.DrawLine(start, hit.point, Color.green, 0.5f);
			}
			else
				Debug.DrawLine(start, start + transform.forward * RAYCAST_LENGTH, Color.yellow, 0.5f);
		}

		private void CONSOLE_SetVar(string[] commands)
		{
			try
			{
				string variable = commands[0];
				float amount = float.Parse(commands[1]);

				switch (variable)
				{
					case "weight":
						mSliderTipWeight = amount;
						break;
					case "bumpmult":
						mBumperMultiplier = amount;
						break;
					case "rotgrow":
						mRotationGrowRate = amount;
						break;
					default:
						throw new Exception();
				}
			}
			catch (Exception)
			{
				throw new ArgumentException("Invalid command arguments for \"setvar\"");
			}
		}

		#endregion

		private void Update()
		{
			UpdateSlideValue();
			UpdateChargeAmount();
			PaintCrosshair();
		}

		private void UpdateSlideValue()
		{
			mSliderVal = Mathf.Clamp(mSliderVal, -0.5f, 0.5f);
			Vector3 pos = mTmpImage.transform.localPosition;
			pos.x = mSliderVal;
			mTmpImage.transform.localPosition = pos;

			mCanvas.transform.LookAt(Camera.main.transform, Vector3.down);
		}

		private void UpdateChargeAmount()
		{
			float chargeChange = (0.5f - Mathf.Abs(mSliderVal)) * 4.0f - 1.0f;
			mGunCharge += chargeChange * mRotationGrowRate * Time.deltaTime;
			mGunCharge = Mathf.Clamp(mGunCharge, 0.0f, 1.0f);
			mGunObject.Rotate(mGunObject.right, mGunMaxRotationRate * mGunCharge * Time.deltaTime, Space.World);

			mSwishSound.SetVolume(mGunCharge);
			mSwishSound.SetPitch(Mathf.Lerp(0.6f, 1.2f, mGunCharge));
		}
		
		private void PaintCrosshair()
		{
			Vector3 start = transform.position + Vector3.up;
			RaycastHit hit;
			if (Physics.Raycast(new Ray(start, transform.forward), out hit, RAYCAST_LENGTH))
			{
				mLineRenderer.SetPosition(0, mGunTipHint.position);
				mLineRenderer.SetPosition(1, hit.point);
			}
		}

		private void LateUpdate()
		{
			float l = mLeftInput / 2.0f + 1.0f, r = mRightInput / 2.0f + 1.0f;
			mSliderVal += (l - r) / 2.0f * Time.deltaTime * mBumperMultiplier;

			mSliderVal += mSliderVelocity * Time.deltaTime;
			mSliderVelocity = mSliderVal * mSliderTipWeight;
		}
	}
}