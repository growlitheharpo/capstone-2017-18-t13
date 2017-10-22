using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Weapons;
using FiringSquad.Data;
using FiringSquad.Gameplay.UI;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.NPC
{
	public class NpcTurret : NetworkBehaviour, IWeaponBearer, IDamageReceiver
	{
		[SerializeField] private AudioProfile mAudioProfile;
		public AudioProfile audioProfile { get { return mAudioProfile; } }

		[SerializeField] private WeaponPartCollection mParts;
		public WeaponPartCollection defaultParts { get { return mParts; } }

		[SerializeField] [EnumFlags] private BaseWeaponScript.Attachment mPartsToDrop;
		[SerializeField] private float mDefaultHealth;
		[SerializeField] private float mRespawnTime;
		[SerializeField] private Transform mWeaponAttachPoint;
		[SerializeField] private GameObject mBaseWeaponPrefab;

		public IWeapon weapon { get; private set; }
		public Transform eye { get { return transform; } }
		public bool isCurrentPlayer { get { return false; } }

		private IPlayerHitIndicator mHitIndicator;
		private float mHealth;

		public override void OnStartServer()
		{
			mHealth = mDefaultHealth;

			// create our weapon & bind
			BaseWeaponScript wep = Instantiate(mBaseWeaponPrefab).GetComponent<BaseWeaponScript>();
			BindWeaponToBearer(wep);
			AddDefaultPartsToWeapon(wep);
			NetworkServer.Spawn(wep.gameObject);
		}

		public override void OnStartClient()
		{
			// create a hit indicator
			GameObject hitObject = new GameObject("HitIndicator");
			hitObject.transform.SetParent(transform);
			mHitIndicator = hitObject.AddComponent<RemotePlayerHitIndicator>();
		}

		public void BindWeaponToBearer(IModifiableWeapon wep, bool bindUI = false)
		{
			// find attach spot in view and set parent
			wep.transform.SetParent(mWeaponAttachPoint);
			wep.transform.ResetLocalValues();
			wep.positionOffset = transform.InverseTransformPoint(mWeaponAttachPoint.position);
			wep.transform.SetParent(transform);
			wep.bearer = this;
			weapon = wep;
		}

		[ServerCallback]
		private void Update()
		{
			if (mHealth <= 0.0f)
				return;

			weapon.FireWeaponHold();
		}

		[Server]
		private void AddDefaultPartsToWeapon(IWeapon wep)
		{
			foreach (WeaponPartScript part in defaultParts)
				wep.AttachNewPart(part.partId, WeaponPartScript.INFINITE_DURABILITY);
		}

		public void PlayFireAnimation()
		{
			// ignore for now
		}

		[Server]
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
		{
			RpcReflectDamageLocally(point, normal, cause.source.gameObject.transform.position, amount);

			if (mHealth <= 0.0f)
				return;

			mHealth = Mathf.Clamp(mHealth - amount, 0.0f, float.MaxValue);

			if (mHealth <= 0.0f)
				HandleTurretDeath();
		}

		[ClientRpc]
		private void RpcReflectDamageLocally(Vector3 point, Vector3 normal, Vector3 origin, float amount)
		{
			mHitIndicator.NotifyHit(this, origin, point, normal, amount);
		}

		private void HandleTurretDeath()
		{
			SpawnDeathWeaponParts();
		}

		[Server]
		private void SpawnDeathWeaponParts()
		{
			IWeaponPartManager partService = ServiceLocator.Get<IWeaponPartManager>();
			foreach (WeaponPartScript part in weapon.currentParts)
			{
				if ((mPartsToDrop & part.attachPoint) != part.attachPoint)
					continue;

				WeaponPartScript prefab = partService.GetPrefabScript(part.partId);
				GameObject instance = prefab.SpawnInWorld();

				instance.transform.position = weapon.transform.position + Random.insideUnitSphere;

				instance.GetComponent<WeaponPickupScript>().overrideDurability = part.durability;
				instance.GetComponent<Rigidbody>().AddExplosionForce(40.0f, transform.position, 2.0f);

				NetworkServer.Spawn(instance);

				StartCoroutine(Coroutines.InvokeAfterFrames(2, () => { instance.GetComponent<WeaponPickupScript>().RpcInitializePickupView(); }));
			}
		}
	}
}
