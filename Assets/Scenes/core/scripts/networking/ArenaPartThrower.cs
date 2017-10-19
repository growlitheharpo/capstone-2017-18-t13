using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Core;
using FiringSquad.Core.Weapons;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Collections;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Logger = FiringSquad.Debug.Logger;

namespace FiringSquad.Gameplay
{
	public class ArenaPartThrower : NetworkBehaviour
	{
		[SerializeField] private float mMinimumThrowTime;
		[SerializeField] private float mMaximumThrowTime;
		[SerializeField] private int mMaxExistingItems;

		private GameObject[] mWeaponPrefabs;
		private Vector3[] mMeshPoints;

		private List<GameObject> mSpawnedObjects;

		[ServerCallback]
		private void Awake()
		{

#if UNITY_EDITOR
			GenerateMeshPoints();
#endif
		}

		[ServerCallback]
		private void Start()
		{
			LoadWeaponPrefabs();
			mSpawnedObjects = new List<GameObject>();

			if (isServer)
			{
				GenerateMeshPoints();
				StartCoroutine(ThrowPartTimer());
			}
		}

		[Server]
		private void LoadWeaponPrefabs()
		{
			mWeaponPrefabs = ServiceLocator.Get<IWeaponPartManager>()
				.GetAllPrefabs(false).Values.ToArray();

			foreach (GameObject prefab in mWeaponPrefabs)
			{
				Logger.Info("Registering part for spawn: " + prefab.name, Logger.System.Network);
				ClientScene.RegisterPrefab(prefab);
			}
		}

		[Server]
		private void GenerateMeshPoints()
		{
			MeshFilter mesh = GetComponent<MeshFilter>();
			mMeshPoints = mesh.sharedMesh.vertices;
			for (int i = 0; i < mMeshPoints.Length; i++)
				mMeshPoints[i] = transform.TransformPoint(mMeshPoints[i]);
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (!UnityEditor.EditorApplication.isPlaying || mMeshPoints == null)
				return;

			Gizmos.color = Color.blue;

			foreach (Vector3 point in mMeshPoints)
				Gizmos.DrawSphere(point, 1.0f);
		}
#endif

		[Server]
		private IEnumerator ThrowPartTimer()
		{
			while (true)
			{
				yield return new WaitForSeconds(Random.Range(mMinimumThrowTime, mMaximumThrowTime));
				CleanupInstanceList();

				if (mSpawnedObjects.Count >= mMaxExistingItems)
					continue;

				GameObject prefab = mWeaponPrefabs.ChooseRandom();
				GameObject instance = CustomInstantiatePart(prefab);
				mSpawnedObjects.Add(instance);
			}
		}

		[Server]
		private GameObject CustomInstantiatePart(GameObject prefab)
		{
			Vector3 point = mMeshPoints.ChooseRandom();
			Vector3 direction = (transform.position - point).normalized + Vector3.up * 2.0f;

			GameObject instance = prefab.GetComponent<WeaponPartScript>().SpawnInWorld();
			instance.transform.position = point;
			instance.name = prefab.name;

			instance.GetComponent<Rigidbody>().AddForce(direction.normalized * 20.0f, ForceMode.Impulse);
			NetworkServer.Spawn(instance);

			StartCoroutine(Coroutines.InvokeAfterFrames(2, () =>
			{
				instance.GetComponent<WeaponPickupScript>().RpcInitializePickupView();
			}));

			return instance;
		}

		[Server]
		private void CleanupInstanceList()
		{
			mSpawnedObjects = mSpawnedObjects.Where(x => x != null && x.GetComponent<WeaponPickupScript>() != null).ToList();
		}
	}
}
