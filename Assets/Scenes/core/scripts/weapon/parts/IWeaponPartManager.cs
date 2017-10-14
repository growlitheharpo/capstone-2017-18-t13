using System.Collections.Generic;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public interface IWeaponPartManager
	{
		GameObject GetPartPrefab(string id);
		GameObject this[string index] { get; }

		Dictionary<string, GameObject> GetAllPrefabs(bool includeDebug);
	}
}
