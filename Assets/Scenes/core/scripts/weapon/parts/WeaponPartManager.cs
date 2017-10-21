using System.Collections.Generic;
using System.Linq;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;

namespace FiringSquad.Core.Weapons
{
	public class WeaponPartManager : MonoSingleton<WeaponPartManager>, IWeaponPartManager
	{
		private Dictionary<string, GameObject> mPrefabs;
		private Dictionary<string, WeaponPartScript> mScripts;
		private Dictionary<string, GameObject> prefabs
		{
			get
			{
				LazyInitialize();
				return mPrefabs;
			}
		}

		private Dictionary<string, WeaponPartScript> scripts
		{
			get
			{
				LazyInitialize();
				return mScripts;
			}
		}

		public WeaponPartScript GetPrefabScript(string id)
		{
			return scripts[id];
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

		public Dictionary<string, WeaponPartScript> GetAllPrefabScripts(bool includeDebug)
		{
			if (includeDebug)
				return scripts;

			return scripts.Values
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
			mScripts = mPrefabs
				.Select(x => x.Value.GetComponent<WeaponPartScript>())
				.ToDictionary(x => x.name);
		}
	}
}
