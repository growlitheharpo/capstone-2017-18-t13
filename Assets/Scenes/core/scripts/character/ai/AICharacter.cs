using FiringSquad.Data;
using KeatsLib;
using UnityEngine;
using UnityEngine.AI;

namespace FiringSquad.Gameplay.AI
{
	public class AICharacter : MonoBehaviour, IWeaponBearer, IDamageReceiver
	{
		[SerializeField] private CharacterMovementData mMovementData;
		[SerializeField] private GameObject mGunPrefab;
		[SerializeField] private WeaponDefaultsData mGunDefaultParts;
		[SerializeField] private float mDefaultHealth;

		private BoundProperty<float> mCurrentHealth;
		private AIStateMachine mStateMachine;
		private AIWeaponScript mWeapon;
		private Transform mFakeEye;

		public CharacterMovementData movementData { get { return mMovementData; } }
		public Transform eye { get { return mFakeEye; } }
		public IWeapon weapon { get { return mWeapon; }}

		private void Awake()
		{
			mStateMachine = GetComponent<AIStateMachine>();
			mFakeEye = transform.Find("FakeAIEye");

			Transform offset = transform.Find("Gun1Offset");
			GameObject gun = UnityUtils.InstantiateIntoHolder(mGunPrefab, offset, true, true);
			mWeapon = gun.GetComponent<AIWeaponScript>();
		}

		private void Start()
		{
			mCurrentHealth = new BoundProperty<float>(mDefaultHealth, (name + "-health").GetHashCode());

			mWeapon.bearer = this;
			mWeapon.SetAimRoot(eye);

			foreach (GameObject part in mGunDefaultParts)
				Instantiate(part).GetComponent<WeaponPickupScript>().ConfirmAttach(mWeapon);
		}

		private void OnDestroy()
		{
			mCurrentHealth.Cleanup();
		}
		
		public void ApplyRecoil(Vector3 direction, float amount)
		{
			// TODO: How will the AI respond to recoil?
		}

		public void ApplyDamage(float amount, Vector3 point, IDamageSource cause)
		{
			mStateMachine.NotifyAttackedByPlayer();
			mCurrentHealth.value = Mathf.Clamp(mCurrentHealth.value - amount, 0.0f, float.MaxValue);

			if (mCurrentHealth.value <= 0.0f)
				Die();
		}

		private void Die()
		{
			// TODO: Implement death
			Logger.Info("\"I just died :(\" - " + name);
		}
	}
}
