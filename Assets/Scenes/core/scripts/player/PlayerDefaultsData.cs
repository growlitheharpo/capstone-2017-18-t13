using System;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// Collection of useful/necessary data for balancing and tweaking gameplay.
	/// </summary>
	[Serializable]
	public class PlayerDefaultsData
	{
		[Header("Weapons")] [SerializeField] private WeaponPartCollection mDefaultWeaponParts;

		[Header("Base Data")] [SerializeField] private float mInteractDistance;
		[SerializeField] private float mDefaultHealth;
		[SerializeField] private float mRespawnTime;

		/// <summary> The default collection of parts for this player. </summary>
		public WeaponPartCollection defaultWeaponParts { get { return mDefaultWeaponParts; } }

		/// <summary> The range of the invisible interact "gun". </summary>
		public float interactDistance { get { return mInteractDistance; } }

		/// <summary> The health that this player should spawn with. </summary>
		public float defaultHealth { get { return mDefaultHealth; } }

		/// <summary> The amount of time in seconds after dying before this player respawns. </summary>
		public float respawnTime { get { return mRespawnTime; } }
	}
}
