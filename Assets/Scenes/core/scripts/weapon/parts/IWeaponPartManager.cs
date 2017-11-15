using System.Collections.Generic;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;

namespace FiringSquad.Core.Weapons
{
	public interface IWeaponPartManager : IGlobalService
	{
		WeaponPartScript GetPrefabScript(string id);
		GameObject GetPartPrefab(string id);
		GameObject this[string index] { get; }

		Dictionary<string, GameObject> GetAllPrefabs(bool includeDebug);
		Dictionary<string, WeaponPartScript> GetAllPrefabScripts(bool includeDebug);
	}
}
