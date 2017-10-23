using System;
using System.Collections;
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
			[SerializeField] private GameObject mPrefab;
			[Range(0.0f, 1.0f)] [SerializeField] private float mWeight;

			public GameObject prefab { get { return mPrefab; } }
			public float weight { get { return mWeight; } }
		}

		[HideInInspector] [SerializeField] private List<PartWeightSet> mParts;
		[SerializeField] private GameObject mBreakVFX;
		public float mRespawnTime;

		private Vector3 mColliderExtents;
		private Collider mCollider;
		private GameObject mView;

		[SyncVar(hook = "OnChangeVisible")] private bool mVisible = true;

		private void Awake()
		{
			mCollider = GetComponent<Collider>();
			mColliderExtents = mCollider.bounds.extents * 0.95f;
			mView = transform.Find("EnabledView").gameObject;
		}

		public override void OnStartClient()
		{
			base.OnStartClient();

			if (!mVisible)
			{
				mCollider.enabled = false;
				mView.SetActive(false);
			}
		}

		[Server]
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
		{
			if (!mVisible)
				return;

			CmdSpawnPart();
		}

		[Command]
		private void CmdSpawnPart()
		{
			mVisible = false;

			GameObject prefab = mParts.ChooseRandomWeighted(part => part.weight).prefab;
			GameObject instance = prefab.GetComponent<WeaponPartScript>().SpawnInWorld();
			instance.transform.position = transform.position + Vector3.up * 0.5f;
			instance.name = prefab.name;

			instance.GetComponent<Rigidbody>().AddForce(Vector3.up * 7.0f, ForceMode.Impulse);
			NetworkServer.Spawn(instance);

			StartCoroutine(WaitAndReappear());
			StartCoroutine(Coroutines.InvokeAfterFrames(2, () => { instance.GetComponent<WeaponPickupScript>().RpcInitializePickupView(); }));
		}

		[Server]
		private IEnumerator WaitAndReappear()
		{
			yield return new WaitForSeconds(mRespawnTime);

			bool clear;
			do
			{
				yield return null;
				var cols = Physics.OverlapBox(mCollider.bounds.center, mColliderExtents, transform.rotation);
				clear = cols.Length == 0;
			} while (!clear);

			mVisible = true;
		}

		private void OnChangeVisible(bool visible)
		{
			mVisible = visible;
			mCollider.enabled = visible;
			mView.SetActive(visible);

			if (!visible)
				CreateBreakParticles();
		}

		private void CreateBreakParticles()
		{
			ParticleSystem ps = Instantiate(mBreakVFX, transform.position + Vector3.up * 0.25f, Quaternion.identity).GetComponent<ParticleSystem>();
			ps.Play();
			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(ps));
		}
	}
}
