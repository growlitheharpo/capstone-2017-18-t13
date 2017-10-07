using System.Collections;
using FiringSquad.Data;
using KeatsLib;
using UnityEngine;

namespace FiringSquad.Gameplay.AI
{
	public class AICharacter : MonoBehaviour, IWeaponBearer, IDamageReceiver
	{
		[SerializeField] private AudioProfile mAudioProfile;
		[SerializeField] private WeaponDefaultsData mGunDefaultParts;
		[SerializeField] private GameObject mDeathParticlesPrefab;
		[SerializeField] private GameObject mGunPrefab;
		[SerializeField] private float mDefaultHealth;

		[SerializeField] private WeaponDropWeights mDropWeights;

		private BoundProperty<float> mCurrentHealth;
		private AIStateMachine mStateMachine;
		private AIWeaponScript mWeapon;
		private Transform mFakeEye;

		public bool isCurrentPlayer { get { return false; } }
		public Transform eye { get { return mFakeEye; } }
		public IWeapon weapon { get { return mWeapon; } }
		public WeaponDefaultsData defaultParts { get { return mGunDefaultParts; } }

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
			{
				Instantiate(part)
					.GetComponent<WeaponPickupScript>()
					.OverrideDurability(WeaponPartScript.INFINITE_DURABILITY)
					.ConfirmAttach(mWeapon);
			}
		}

		private void OnDestroy()
		{
			mCurrentHealth.Cleanup();
		}
		
		public void ApplyRecoil(Vector3 direction, float amount)
		{
			// TODO: How will the AI respond to recoil?
		}

		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
		{
			mStateMachine.NotifyAttackedByPlayer();
			mCurrentHealth.value -= amount;

			// HACK: Checking "mCurrentHealth.value > -50000.0f" is how we check to see if we're already dying.
			if (mCurrentHealth.value <= 0.0f && mCurrentHealth.value > -50000.0f)
				Die();
		}

		private void Die()
		{
			mCurrentHealth.value = float.MinValue;

			EventManager.Notify(() => EventManager.PlayerKilledEnemy(this));

			Destroy(mStateMachine);
			Destroy(GetComponent<Collider>());

			foreach (Transform child in transform)
				Destroy(child.gameObject);

			if (ServiceLocator.Get<IGamestateManager>().IsFeatureEnabled(GamestateManager.Feature.WeaponDrops))
				DropWeapon();

			StartCoroutine(DoDeathEffects());
		}

		private void DropWeapon()
		{
			BaseWeaponScript.Attachment attachment = mDropWeights.ChooseRandomWeightedAttachment();
			GameObject prefab = mGunDefaultParts[attachment];

			GameObject particles = Instantiate(ReferenceForwarder.get.droppedWeaponParticlesPrefab, Vector3.zero, Quaternion.identity);

			GameObject instance = Instantiate(prefab, transform.position, Quaternion.identity);
			Transform parent = instance.transform.Find("PickupCollider");

			particles.transform.SetParent(parent, false);
		}

		private IEnumerator DoDeathEffects()
		{
			ParticleSystem ps = Instantiate(mDeathParticlesPrefab).GetComponent<ParticleSystem>();
			ps.transform.SetParent(transform);
			ps.transform.localPosition = Vector3.zero;
			ps.Play();

			IAudioReference audRef = ServiceLocator.Get<IAudioManager>()
				.PlaySound(AudioManager.AudioEvent.DeathSound, mAudioProfile, ps.transform);

			yield return null;
			yield return null;
			yield return new WaitForParticles(ps);
			yield return new WaitForAudio(audRef);
			yield return null;
			audRef.Kill();
			Destroy(gameObject);
		}
	}
}
