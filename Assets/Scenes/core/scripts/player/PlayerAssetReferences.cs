using System;
using UnityEngine;

namespace FiringSquad.Data
{
	[Serializable]
	public class PlayerAssetReferences
	{
		[Header("Weapons")]
		[SerializeField]
		private GameObject mGravityGunPrefab;
		[SerializeField] private GameObject mBaseWeaponPrefab;

		[Header("View")]
		[SerializeField] private GameObject mLocalPlayerPrefab;
		[SerializeField] private GameObject mDeathParticlesPrefab;


		public GameObject gravityGunPrefab { get { return mGravityGunPrefab; } }
		public GameObject baseWeaponPrefab { get { return mBaseWeaponPrefab; } }

		public GameObject localPlayerPrefab { get { return mLocalPlayerPrefab; } }
		public GameObject deathParticlesPrefab { get { return mDeathParticlesPrefab; } }
	}
}
