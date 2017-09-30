using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeatsLib.Collections;
using UnityEngine;
using UnityEngine.Networking;

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

		private void Awake()
		{
			LoadWeaponPrefabs();

#if UNITY_EDITOR
			GenerateMeshPoints();
#endif
		}

		private void Start()
		{
			mSpawnedObjects = new List<GameObject>();

			if (isServer)
			{
				GenerateMeshPoints();
				StartCoroutine(ThrowPartTimer());
			}
		}

		private void LoadWeaponPrefabs()
		{
			var allObjects = Resources.LoadAll<GameObject>("prefabs/weapons");
			mWeaponPrefabs = allObjects
				.Where(x => x.GetComponent<WeaponPartScript>() != null)
				.ToArray();

			foreach (GameObject prefab in mWeaponPrefabs)
			{
				Logger.Info("Registering part for spawn: " + prefab.name, Logger.System.Network);
				ClientScene.RegisterPrefab(prefab);
			}
		}
		
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

		private IEnumerator ThrowPartTimer()
		{
			while (true)
			{
				yield return new WaitForSeconds(Random.Range(mMinimumThrowTime, mMaximumThrowTime));
				CleanupInstanceList();

				if (mSpawnedObjects.Count >= mMaxExistingItems)
					continue;

				GameObject prefab = mWeaponPrefabs.ChooseRandom();
				GameObject instance = InstantiatePart(prefab);
				mSpawnedObjects.Add(instance);
			}
		}

		private GameObject InstantiatePart(GameObject prefab)
		{
			Vector3 point = mMeshPoints.ChooseRandom();
			Vector3 direction = (transform.position - point).normalized + Vector3.up * 2.0f;

			GameObject instance = Instantiate(prefab, point, Quaternion.identity);
			instance.name = prefab.name;

			instance.GetComponent<Rigidbody>().AddForce(direction.normalized * 20.0f, ForceMode.Impulse);
			NetworkServer.Spawn(instance);

			return instance;
		}

		private void CleanupInstanceList()
		{
			mSpawnedObjects = mSpawnedObjects.Where(x => x != null && x.GetComponent<WeaponPickupScript>() != null).ToList();
		}
	}
}
