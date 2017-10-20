using System;
using System.Collections.Generic;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Collections;
using KeatsLib.Unity;
using UnityEngine.Networking;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class WeaponPartCrate : NetworkBehaviour, IDamageReceiver
	{
		[Serializable]
		public struct PartWeightSet
		{
			public GameObject mPrefab;
			[Range(0.0f, 1.0f)] public float mWeight;
		}

		[HideInInspector] [SerializeField] List<PartWeightSet> mParts;
		[SerializeField] private GameObject mBreakVFX;
		public float mRespawnTime;

		private Collider mCollider;
		private GameObject mView;

		[SyncVar(hook = "OnChangeVisible")] private bool mVisible = true;

		private void Awake()
		{
			mCollider = GetComponent<Collider>();
			mView = transform.Find("EnabledView").gameObject;
		}

		[Server]
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
		{
			CmdSpawnPart();
		}

		[Command]
		private void CmdSpawnPart()
		{
			mCollider.enabled = false;
			mView.SetActive(false);
			//SpawnParticles();
			mVisible = false;

			GameObject prefab = mParts.ChooseRandom().mPrefab;
			GameObject instance = prefab.GetComponent<WeaponPartScript>().SpawnInWorld();
			instance.transform.position = transform.position + Vector3.up * 0.5f;
			instance.name = prefab.name;

			instance.GetComponent<Rigidbody>().AddForce(Vector3.up * 7.0f, ForceMode.Impulse);
			NetworkServer.Spawn(instance);

			StartCoroutine(Coroutines.InvokeAfterFrames(2, () => { instance.GetComponent<WeaponPickupScript>().RpcInitializePickupView(); }));
		}

		private void OnChangeVisible(bool visible)
		{
			mVisible = visible;
			mCollider.enabled = visible;
			mView.SetActive(visible);

			if (!visible)
				SpawnParticles();
		}

		private void SpawnParticles()
		{
			ParticleSystem ps = Instantiate(mBreakVFX, transform.position + Vector3.up * 0.25f, Quaternion.identity).GetComponent<ParticleSystem>();
			ps.Play();
			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(ps));
		}
	}
}
