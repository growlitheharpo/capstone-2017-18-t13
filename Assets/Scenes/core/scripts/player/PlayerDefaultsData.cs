using System;
using UnityEngine;

namespace FiringSquad.Data
{
	[Serializable]
	public class PlayerDefaultsData
	{
		[Header("Weapons")]
		[SerializeField] private GameObject mGravityGunPrefab;
		[SerializeField] private GameObject mBaseWeaponPrefab;
		[SerializeField] private WeaponDefaultsData mDefaultWeaponParts;

		[Header("Base Data")]
		[SerializeField] private float mInteractDistance;
		[SerializeField] private float mDefaultHealth;

		[Header("Startup Options")]
		[SerializeField] private bool mShouldInstantiateWeapon = true;
		[SerializeField] private bool mShouldInstantiateGravityGun = true;
		[SerializeField] private bool mInstantiateParts = true;

		public GameObject gravityGunPrefab { get { return mGravityGunPrefab; } }
		public GameObject baseWeaponPrefab { get { return mBaseWeaponPrefab; } }
		public WeaponDefaultsData defaultWeaponParts { get { return mDefaultWeaponParts; } }
		public float interactDistance { get { return mInteractDistance; } }
		public float defaultHealth { get { return mDefaultHealth; } }

		public bool makeGravGun { get { return mShouldInstantiateGravityGun; } }
		public bool makeWeaponGun { get { return mShouldInstantiateWeapon; } }
		public bool makeParts { get { return mInstantiateParts; } }
	}
}
