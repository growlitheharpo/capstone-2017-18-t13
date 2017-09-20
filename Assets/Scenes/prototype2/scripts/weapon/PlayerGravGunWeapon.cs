using System.Collections;
using UnityEngine;
using Input = UnityEngine.Input;

namespace FiringSquad.Gameplay
{
	public class PlayerGravGunWeapon : BaseStateMachine
	{
		private class IdleState : BaseState<PlayerGravGunWeapon>
		{
			public IdleState(PlayerGravGunWeapon m) : base(m) { }

			public override IState GetTransition()
			{

			}
		}

		private enum InputState
		{
			OFF,
			PRESSED,
			HELD,
			RELEASED,
		}

		private bool mGotInputThisFrame;
		private InputState mInput;

		private void Start()
		{
			TransitionStates(new IdleState(this));

			ServiceLocator.Get<IInput>()
				.RegisterInput(Input.GetButton, "Fire2", SetInputState, KeatsLib.Unity.Input.InputLevel.Gameplay);
		}

		private void OnDestroy()
		{
			ServiceLocator.Get<IInput>()
				.UnregisterInput(SetInputState);
		}

		protected override void Update()
		{
			if (mGotInputThisFrame)
			{
				
			}
			else if (mInput == InputState.HELD || mInput == InputState.PRESSED)
				mInput = InputState.RELEASED;
			else
				mInput = InputState.OFF;

			mGotInputThisFrame = false;
			base.Update();
		}

		private void SetInputState()
		{
			mGotInputThisFrame = true;
		}
	}

	/*public class PlayerGravGunWeapon : MonoBehaviour
	{
		public IWeaponBearer bearer { get; set; }

		[SerializeField] public float mStrength = 2.0f;

		private enum HoldState
		{
			None,
			Grabbing,
			Holding,
		}

		private HoldState mState = HoldState.None;
		private RigidbodyConstraints mOriginalConstraints;
		private float mFire2DownTime = float.MaxValue;
		private Rigidbody mHeldItem;

		private void Update()
		{
			if (mState == HoldState.Holding)
				KeepItemSteady();

			if (mState == HoldState.None && Input.GetButton("Fire2"))
				TryPullItem();
			else if (mState == HoldState.Holding)
			{
				if (Input.GetButtonDown("Fire2"))
					StartChargingDown();
				else if (Input.GetButtonUp("Fire2"))
					ReleaseButton();
			}
		}


		private void StartChargingDown()
		{
			mFire2DownTime = Time.time;
		}

		private void ReleaseButton()
		{
			if (mFire2DownTime > Time.time)
				return;

			float length = Time.time - mFire2DownTime;

			if (length < 1.0f)
				DropItem();
			else
				ThrowItem();

			mFire2DownTime = float.MaxValue;
		}

		public void FireWeapon() { }

		private void TryPullItem()
		{
			Transform eye = bearer.eye;

			Ray ray = new Ray(eye.position, eye.forward);
			UnityEngine.Debug.DrawLine(ray.origin, ray.origin + ray.direction * 4000.0f, new Color(.6f, 0.0f, 1.0f), 0.2f);

			RaycastHit hit;
			if (!Physics.Raycast(ray, out hit, 4000.0f))
				return;

			Rigidbody rb = hit.rigidbody;
			if (rb == null)
				return;

			Vector3 direction = transform.position - rb.position;

			if (direction.magnitude <= 4.0f)
				GrabItem(rb);
			else
				rb.AddForce(direction * Time.deltaTime * mStrength, ForceMode.Impulse);
		}

		private void GrabItem(Rigidbody item)
		{
			mOriginalConstraints = item.constraints;
			item.constraints = RigidbodyConstraints.FreezeAll;
			mHeldItem = item;

			StartCoroutine(LerpToMyPosition(Vector3.Distance(transform.position, mHeldItem.position) / 15.0f));
			mState = HoldState.Grabbing;
		}

		private IEnumerator LerpToMyPosition(float time = 0.3f)
		{
			Vector3 originalPos = mHeldItem.transform.position;
			float currentTime = 0.0f;

			while (currentTime < time)
			{
				mHeldItem.transform.position = Vector3.Lerp(originalPos, transform.position, currentTime / time);
				currentTime += Time.deltaTime;

				yield return null;
			}

			mState = HoldState.Holding;
		}

		private void KeepItemSteady()
		{
			//for now, just set the position
			mHeldItem.transform.position = transform.position;
		}

		private void DropItem()
		{
			mHeldItem.constraints = mOriginalConstraints;

			mState = HoldState.None;
			mHeldItem = null;
		}

		private void ThrowItem()
		{
			mHeldItem.constraints = mOriginalConstraints;

			mHeldItem.AddForce(bearer.eye.forward * 150.0f, ForceMode.Impulse);

			mState = HoldState.None;
			mHeldItem = null;
		}
	}*/
}
