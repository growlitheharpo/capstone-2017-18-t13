using System.Collections.Generic;
using System.Linq;
using FiringSquad.Gameplay;
using UnityEngine;

public class WeaponPartManager : MonoSingleton<WeaponPartManager>, IWeaponPartManager
{
	private Dictionary<string, GameObject> mPrefabs;

	private Dictionary<string, GameObject> prefabs
	{
		get
		{
			LazyInitialize();
			return mPrefabs;
		}
	}

	public GameObject GetPartPrefab(string id)
	{
		return prefabs[id];
	}

	public GameObject this[string index] { get { return prefabs[index]; } }

	public Dictionary<string, GameObject> GetAllPrefabs(bool includeDebug)
	{
		LazyInitialize();

		if (includeDebug)
			return prefabs;

		return prefabs.Values
			.Where(x => !x.name.ToLower().Contains("debug"))
			.ToDictionary(x => x.name);
	}

	private void LazyInitialize()
	{
		if (mPrefabs != null)
			return;

		var allObjects = Resources.LoadAll<GameObject>("prefabs/weapons");
		mPrefabs = allObjects
			.Where(x => x.GetComponent<WeaponPartScript>() != null)
			.ToDictionary(x => x.name);
	}
}
