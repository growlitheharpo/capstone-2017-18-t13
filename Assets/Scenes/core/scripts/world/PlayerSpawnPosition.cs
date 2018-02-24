using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// A utility component attached to all the spawn positions.
	/// </summary>
	public class PlayerSpawnPosition : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private GameData.PlayerTeam mTeam;

		// Private variables
		private static List<PlayerSpawnPosition> kAllPositions;

		/// <summary>
		/// The team that this 
		/// </summary>
		public GameData.PlayerTeam team { get { return mTeam; } }

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			if (kAllPositions == null)
				kAllPositions = new List<PlayerSpawnPosition>();
			kAllPositions.Add(this);
		}

		/// <summary>
		/// Get a list of all the spawn positions available.
		/// </summary>
		public static List<PlayerSpawnPosition> GetAll()
		{
			return kAllPositions ?? (kAllPositions = new List<PlayerSpawnPosition>());
		}

		/// <summary>
		/// Get a list of all the spawn positions available for a specific team.
		/// </summary>
		public static List<PlayerSpawnPosition> GetAll(GameData.PlayerTeam forTeam)
		{
			return (kAllPositions ?? (kAllPositions = new List<PlayerSpawnPosition>())).Where(x => x.team == forTeam).ToList();
		}
	}
}
