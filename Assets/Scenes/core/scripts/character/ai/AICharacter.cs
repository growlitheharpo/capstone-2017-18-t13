using FiringSquad.Data;
using KeatsLib;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class AICharacter : MonoBehaviour, IWeaponBearer, IDamageReceiver
	{
		[SerializeField] private GameObject mGunPrefab;
		[SerializeField] private WeaponDefaultsData mGunDefaultParts;
		[SerializeField] private float mDefaultHealth;

		private BoundProperty<float> mCurrentHealth;
		private AIDecisionMaker mDecisionMaker;
		private AIWeaponScript mWeapon;
		private Transform mFakeEye;

		public Transform eye { get { return mFakeEye; } }

		private void Awake()
		{
			mDecisionMaker = new AIDecisionMaker();
			mFakeEye = transform.Find("FakeAIEye");

			var offset = transform.Find("Gun1Offset");
			var gun = UnityUtils.InstantiateIntoHolder(mGunPrefab, offset, true, true);
			mWeapon = gun.GetComponent<AIWeaponScript>();
		}

		// Use this for initialization
		private void Start()
		{
			mCurrentHealth = new BoundProperty<float>(mDefaultHealth, (name + "-health").GetHashCode());

			foreach (GameObject part in mGunDefaultParts)
				Instantiate(part).GetComponent<WeaponPickupScript>().ConfirmAttach(mWeapon);
		}

		private void OnDestroy()
		{
			mCurrentHealth.Cleanup();
		}

		// Update is called once per frame
		private void Update()
		{
			mDecisionMaker.Tick();
		}

		public void ApplyRecoil(Vector3 direction, float amount)
		{
			// TODO: How will the AI respond to recoil?
		}

		public void ApplyDamage(float amount, Vector3 point, IDamageSource cause)
		{
			StopAllCoroutines();
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
