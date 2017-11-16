using System.Collections.Generic;
using System.Linq;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;

namespace FiringSquad.Core.Weapons
{
	/// <inheritdoc cref="IWeaponPartManager" />
	public class WeaponPartManager : MonoSingleton<WeaponPartManager>, IWeaponPartManager
	{
		/// Private variables
		private Dictionary<string, GameObject> mPrefabs;
		private Dictionary<string, WeaponPartScript> mScripts;

		/// <summary>
		/// The collection of prefabs. Will LazyInitialize the first time it is accessed.
		/// </summary>
		private Dictionary<string, GameObject> prefabs
		{
			get
			{
				LazyInitialize();
				return mPrefabs;
			}
		}

		/// <summary>
		/// The collection of scripts. Will LazyInitialize the first time it is accessed.
		/// </summary>
		private Dictionary<string, WeaponPartScript> scripts
		{
			get
			{
				LazyInitialize();
				return mScripts;
			}
		}

		/// <inheritdoc />
		public WeaponPartScript GetPrefabScript(string id)
		{
			return scripts[id];
		}

		/// <inheritdoc />
		public GameObject GetPartPrefab(string id)
		{
			return prefabs[id];
		}

		/// <inheritdoc />
		public GameObject this[string index] { get { return prefabs[index]; } }

		/// <inheritdoc />
		public Dictionary<string, GameObject> GetAllPrefabs(bool includeDebug)
		{
			LazyInitialize();

			if (includeDebug)
				return prefabs;

			return prefabs.Values
				.Where(x => !x.name.ToLower().Contains("debug"))
				.ToDictionary(x => x.name);
		}

		/// <inheritdoc />
		public Dictionary<string, WeaponPartScript> GetAllPrefabScripts(bool includeDebug)
		{
			if (includeDebug)
				return scripts;

			return scripts.Values
				.Where(x => !x.name.ToLower().Contains("debug"))
				.ToDictionary(x => x.name);
		}

		/// <summary>
		/// Initialize our collection of weapon parts by reading the Resources folder.
		/// Should only be done once per game lifetime.
		/// </summary>
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
