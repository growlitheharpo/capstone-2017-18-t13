using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FiringSquad.Data
{
	/// <summary>
	/// A collection of the GameObjects that the CltPlayer must instantiate to work properly.
	/// </summary>
	[Serializable]
	public class PlayerAssetReferences
	{
		[Header("Weapons")] [SerializeField] [FormerlySerializedAs("mGravityGunPrefab")] private GameObject mMagnetArmPrefab;
		[SerializeField] private GameObject mBaseWeaponPrefab;

		[Header("View")] [SerializeField] private GameObject mLocalPlayerPrefab;
		[SerializeField] private GameObject mDeathParticlesPrefab;

		/// <summary> The prefab for the player's magnet arm. </summary>
		public GameObject magnetArmPrefab { get { return mMagnetArmPrefab; } }

		/// <summary> The prefab for the base weapon without any parts. </summary>
		public GameObject baseWeaponPrefab { get { return mBaseWeaponPrefab; } }

		/// <summary> The prefab for the local player controller. </summary>
		public GameObject localPlayerPrefab { get { return mLocalPlayerPrefab; } }

		/// <summary> The prefab for the death particle effect. </summary>
		public GameObject deathParticlesPrefab { get { return mDeathParticlesPrefab; } }
	}
}
