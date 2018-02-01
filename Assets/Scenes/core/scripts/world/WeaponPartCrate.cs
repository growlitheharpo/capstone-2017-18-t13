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
	/// <summary>
	/// Component for crates that contain weapon parts.
	/// Explodes and spawns a part upon receiving damage.
	/// </summary>
	public class WeaponPartCrate : NetworkBehaviour, IDamageReceiver
	{
		/// <summary>
		/// Struct to bind a prefab to a weight (0 - 1).
		/// </summary>
		[Serializable]
		public struct PartWeightSet
		{
			[SerializeField] private GameObject mPrefab;
			[Range(0.0f, 1.0f)] [SerializeField] private float mWeight;

			public GameObject prefab { get { return mPrefab; } }
			public float weight { get { return mWeight; } }
		}

		/// Inspector variables
		[HideInInspector] [SerializeField] private List<PartWeightSet> mParts; // [HideInInspector] because this is drawn with custom editor
		[SerializeField] private GameObject mBreakVFX;
		[SerializeField] private float mRespawnTime;

		/// Syncvars
		[SyncVar(hook = "OnChangeVisible")] private bool mVisible = true;

		/// Private variables
		private Vector3 mColliderExtents;
		private Collider mCollider;
		private GameObject mView;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mCollider = GetComponent<Collider>();
			mColliderExtents = mCollider.bounds.extents * 0.95f;
			mView = transform.Find("EnabledView").gameObject;
		}

		/// <summary>
		/// Handle starting on the client.
		/// </summary>
		/// <inheritdoc />
		public override void OnStartClient()
		{
			base.OnStartClient();

			if (mVisible)
				return;

			mCollider.enabled = false;
			mView.SetActive(false);
		}

		/// <inheritdoc />
		[Server]
		public void ApplyDamage(float amount, Vector3 point, Vector3 normal, IDamageSource cause)
		{
			if (!mVisible)
				return;

			CmdSpawnPart();
		}

		/// <inheritdoc />
		public void HealDamage(float amount) { }

		/// <summary>
		/// Spawn a part after the crate has taken damage.
		/// </summary>
		[Command]
		private void CmdSpawnPart()
		{
			mVisible = false;

			GameObject prefab = mParts.ChooseRandomWeighted(part => part.weight).prefab;
			GameObject instance = prefab.GetComponent<WeaponPartScript>().SpawnInWorld();
			instance.transform.position = transform.position + Vector3.up * 0.5f;
			instance.name = prefab.name;

			instance.GetComponent<Rigidbody>().AddForce(Vector3.up * 1.5f, ForceMode.Impulse);
			NetworkServer.Spawn(instance);

			StartCoroutine(WaitAndReappear());
			StartCoroutine(Coroutines.InvokeAfterFrames(2, () => { instance.GetComponent<WeaponPickupScript>().RpcInitializePickupView(); }));
		}

		/// <summary>
		/// Reappear after a certain amount of time has passed and our collider space is clear.
		/// </summary>
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

		/// <summary>
		/// Handle our visibility changing by toggling our collider and view.
		/// </summary>
		/// <param name="visible">Whether to enable or disable the crate.</param>
		private void OnChangeVisible(bool visible)
		{
			mVisible = visible;
			mCollider.enabled = visible;
			mView.SetActive(visible);

			if (!visible)
				CreateBreakParticles();
		}

		/// <summary>
		/// Spawn particles to demonstrate the crate was broken.
		/// </summary>
		private void CreateBreakParticles()
		{
			ParticleSystem ps = Instantiate(mBreakVFX, transform.position + Vector3.up * 0.25f, Quaternion.identity).GetComponent<ParticleSystem>();
			ps.Play();
			StartCoroutine(Coroutines.WaitAndDestroyParticleSystem(ps));
		}
	}
}
