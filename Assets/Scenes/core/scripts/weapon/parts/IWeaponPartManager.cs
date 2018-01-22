using System.Collections.Generic;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;

namespace FiringSquad.Core.Weapons
{
	/// <summary>
	/// The public interface for the weapon part manager.
	/// Handles storing and accessing part prefabs based on their unique ID.
	/// </summary>
	public interface IWeaponPartManager : IGlobalService
	{
		/// <summary>
		/// Get the (script) prefab based for a particular ID.
		/// </summary>
		WeaponPartScript GetPrefabScript(byte id);

		/// <summary>
		/// Get the (gameobject) prefab for a particular ID.
		/// </summary>
		GameObject GetPartPrefab(byte id);

		/// <summary>
		/// Access the (gameobject) prefab for a particular ID.
		/// </summary>
		GameObject this[byte index] { get; }

		/// <summary>
		/// Access the entire dictionary of (gameobject) part prefabs.
		/// </summary>
		/// <param name="includeDebug">Whether or not to include parts flagged as debug parts.</param>
		Dictionary<byte, GameObject> GetAllPrefabs(bool includeDebug);

		/// <summary>
		/// Access the entire dictionary of (script) part prefabs.
		/// </summary>
		/// <param name="includeDebug">Whether or not to include parts flagged as debug parts.</param>
		Dictionary<byte, WeaponPartScript> GetAllPrefabScripts(bool includeDebug);
	}
}
