using System.Collections.Generic;
using UnityEngine;

namespace FiringSquad.Core.Weapons
{
	public interface IWeaponPartManager
	{
		GameObject GetPartPrefab(string id);
		GameObject this[string index] { get; }

		Dictionary<string, GameObject> GetAllPrefabs(bool includeDebug);
	}
}
