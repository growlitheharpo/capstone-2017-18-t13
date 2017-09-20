using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KeatsLib.Collections;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class ArenaPartThrower : MonoBehaviour
	{
		[SerializeField] private float mMinimumThrowTime;
		[SerializeField] private float mMaximumThrowTime;
		[SerializeField] private int mMaxExistingItems;

		private PlayerScript[] mPlayerList;
		private GameObject[] mWeaponPrefabs;
		private Vector3[] mMeshPoints;

		private List<GameObject> mSpawnedObjects;

		private void Start()
		{
			mSpawnedObjects = new List<GameObject>();

			LoadWeaponPrefabs();
			CreatePlayerList();
			GenerateMeshPoints();
			StartCoroutine(ThrowPartTimer());
		}

		private void LoadWeaponPrefabs()
		{
			var allObjects = Resources.LoadAll<GameObject>("prefabs/weapons");
			mWeaponPrefabs = allObjects
				.Where(x => x.GetComponent<WeaponPartScript>() != null)
				.ToArray();
		}

		private void CreatePlayerList()
		{
			mPlayerList = FindObjectsOfType<PlayerScript>();
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
			if (!UnityEditor.EditorApplication.isPlaying)
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

				Vector3 point = mMeshPoints.ChooseRandom();
				GameObject prefab = mWeaponPrefabs.ChooseRandom();

				Vector3 direction = (transform.position - point).normalized + Vector3.up * 2.0f;
				
				GameObject instance = Instantiate(prefab, point, Quaternion.identity);
				instance.GetComponent<Rigidbody>().AddForce(direction.normalized * 20.0f, ForceMode.Impulse);

				mSpawnedObjects.Add(instance);
			}
		}

		private void CleanupInstanceList()
		{
			mSpawnedObjects = mSpawnedObjects.Where(x => x.GetComponent<WeaponPickupScript>() != null).ToList();
		}
	}
}
