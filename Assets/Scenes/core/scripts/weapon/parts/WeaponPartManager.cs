using System.Collections.Generic;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;

namespace FiringSquad.Core.Weapons
{
	/// <inheritdoc cref="IWeaponPartManager" />
	public class WeaponPartManager : MonoSingleton<WeaponPartManager>, IWeaponPartManager
	{
		/// Private variables
		private Dictionary<byte, GameObject> mPrefabs;
		private Dictionary<byte, WeaponPartScript> mScripts;

		/// <summary>
		/// The collection of prefabs. Will LazyInitialize the first time it is accessed.
		/// </summary>
		private Dictionary<byte, GameObject> prefabs
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
		private Dictionary<byte, WeaponPartScript> scripts
		{
			get
			{
				LazyInitialize();
				return mScripts;
			}
		}

		/// <inheritdoc />
		public WeaponPartScript GetPrefabScript(byte id)
		{
			return scripts[id];
		}

		/// <inheritdoc />
		public GameObject GetPartPrefab(byte id)
		{
			return prefabs[id];
		}

		/// <inheritdoc />
		public GameObject this[byte index] { get { return prefabs[index]; } }

		/// <inheritdoc />
		public Dictionary<byte, GameObject> GetAllPrefabs(bool includeDebug)
		{
			LazyInitialize();

			if (includeDebug)
				return prefabs;
			
			var result = new Dictionary<byte, GameObject>(prefabs.Count);
			foreach (var x in prefabs)
			{
				if (!x.Value.name.ToLower().Contains("debug"))
					result.Add(x.Key, x.Value);
			}

			return result;
		}

		/// <inheritdoc />
		public Dictionary<byte, WeaponPartScript> GetAllPrefabScripts(bool includeDebug)
		{
			if (includeDebug)
				return scripts;

			var result = new Dictionary<byte, WeaponPartScript>(scripts.Count);
			foreach (var x in scripts)
			{
				if (!x.Value.name.ToLower().Contains("debug"))
					result.Add(x.Key, x.Value);
			}

			return result;
		}

		/// <summary>
		/// Initialize our collection of weapon parts by reading the Resources folder.
		/// Should only be done once per game lifetime.
		/// </summary>
		private void LazyInitialize()
		{
			if (mPrefabs != null)
				return;

			mPrefabs = new Dictionary<byte, GameObject>();
			mScripts = new Dictionary<byte, WeaponPartScript>();

			var allObjects = Resources.LoadAll<GameObject>("prefabs/weapons");
			foreach (GameObject go in allObjects)
			{
				WeaponPartScript script = go.GetComponent<WeaponPartScript>();
				if (script == null)
					continue;

				mPrefabs[script.partId] = go;
				mScripts[script.partId] = script;
			}
		}
	}
}
