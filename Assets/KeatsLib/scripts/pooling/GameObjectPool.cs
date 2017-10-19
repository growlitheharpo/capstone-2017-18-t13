using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KeatsLib.Unity
{
	/// <summary>
	/// Generic class for a Unity-based GameObject pool. Does not handle dynamic growing or shrinking;
	/// its size at instantiation is permanent.
	/// </summary>
	public class GameObjectPool
	{
		public int capacity { get { return mObjects.Length; } }
		public int numInUse { get { return mNextEmptyIndex - 1; } }
		public float usePercentage { get { return (float)mNextEmptyIndex / capacity; } }

		private Transform mHolder;
		private GameObject[] mObjects;
		private int mNextEmptyIndex;

		private Dictionary<GameObject, IPoolable[]> mObjectPoolableComponents;

		private static readonly Vector3 HIDDEN_POSITION = new Vector3(-12000, -12000, -12000);

		/// <summary>
		/// Creates an object pool of the provided size using the provided prefab. Immediately instantiates size number of the
		/// object and stores them.
		/// </summary>
		/// <param name="size">The permanent size of this object pool.</param>
		/// <param name="prefab">The prefab to Instantiate.</param>
		/// <param name="objectPoolHolder">The "folder" in the scene hierarchy to place the objects.</param>
		/// <exception cref="NullReferenceException">The prefab passed was null, or could not be Instantiated.</exception>
		public GameObjectPool(int size, GameObject prefab, Transform objectPoolHolder = null)
		{
			mHolder = objectPoolHolder;
			mObjects = new GameObject[size];
			mObjectPoolableComponents = new Dictionary<GameObject, IPoolable[]>();

			for (int i = 0; i < size; i++)
			{
				mObjects[i] = Object.Instantiate(prefab, HIDDEN_POSITION, Quaternion.identity);

				if (mObjects[i] == null)
					throw new NullReferenceException("Instantiate returned null--object pool cannot continue to populate.");

				mObjects[i].name = prefab.name;

				if (objectPoolHolder != null)
					mObjects[i].transform.SetParent(objectPoolHolder);

				mObjectPoolableComponents[mObjects[i]] = mObjects[i].GetComponents<IPoolable>().ToArray();

				mObjects[i].SetActive(false);
			}
		}

		/// <summary>
		/// Substitute for "Instantiate()". Grabs an object from the pool and places it at (0, 0, 0) with a rotation of (0, 0,
		/// 0).
		/// </summary>
		/// <exception cref="IndexOutOfRangeException">Trying to release an item from a pool that is fully in-use.</exception>
		/// <returns>Thew new GameObject from the pool.</returns>
		public GameObject ReleaseNewItem()
		{
			return ReleaseNewItem(Vector3.zero, Quaternion.identity);
		}

		/// <summary>
		/// Substitude for "Instantiate()". Grabs an object from the pool.
		/// </summary>
		/// <param name="newPosition">World position to place the object.</param>
		/// <param name="newRotation">World rotation to place the object.</param>
		/// <exception cref="IndexOutOfRangeException">Trying to release an item from a pool that is fully in-use.</exception>
		/// <returns>The new GameObject from the pool.</returns>
		public GameObject ReleaseNewItem(Vector3 newPosition, Quaternion newRotation)
		{
			if (mNextEmptyIndex >= mObjects.Length)
				throw new IndexOutOfRangeException("Trying to get more items from the pool than it contains");

			if (capacity == 0)
				throw new InvalidOperationException("Trying to access an empty pool!");

			foreach (IPoolable p in mObjectPoolableComponents[mObjects[mNextEmptyIndex]])
				p.PreSetup();

			mObjects[mNextEmptyIndex].transform.position = newPosition;
			mObjects[mNextEmptyIndex].transform.rotation = newRotation;
			mObjects[mNextEmptyIndex].SetActive(true);

			foreach (IPoolable p in mObjectPoolableComponents[mObjects[mNextEmptyIndex]])
				p.PostSetup();

			mNextEmptyIndex++;
			return mObjects[mNextEmptyIndex - 1];
		}

		/// <summary>
		/// Substitute for "Destroy()". Returns an object to the pool.
		/// </summary>
		/// <param name="item">The GameObject to be returned.</param>
		/// <exception cref="ArgumentException">The item is not a member of this pool.</exception>
		public void ReturnItem(GameObject item)
		{
			int index = mObjects.TakeWhile(g => g != item)
				.Count(); //a way to find the index of an item. See: http://stackoverflow.com/a/12958921

			if (index < 0 || index >= mObjects.Length)
				throw new ArgumentException("Tried to return an object to a pool it didn't belong to");
			if (index >= mNextEmptyIndex)
				throw new ArgumentException("Tried to return an object to a pool, but it was already returned!");

			foreach (IPoolable p in mObjectPoolableComponents[item])
				p.PreDisable();

			item.SetActive(false);
			item.transform.rotation = Quaternion.identity;
			item.transform.position = HIDDEN_POSITION;
			item.transform.SetParent(mHolder);

			//Swap the newly returned item with the last item in use.
			mNextEmptyIndex--;
			mObjects[index] = mObjects[mNextEmptyIndex];
			mObjects[mNextEmptyIndex] = item;

			foreach (IPoolable p in mObjectPoolableComponents[item])
				p.PostDisable();
		}

		/// <summary>
		/// Destroy all of the GameObjects that this pool created.
		/// </summary>
		public void Destroy()
		{
			foreach (GameObject g in mObjects)
				Object.Destroy(g);

			mObjectPoolableComponents = null;
			mObjects = null;
			mHolder = null;
		}
	}
}
