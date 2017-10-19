using System;
using UnityEngine;

namespace FiringSquad.Data
{
	[Serializable]
	public class PlayerDefaultsData
	{
		[Header("Weapons")] [SerializeField] private WeaponPartCollection mDefaultWeaponParts;

		[Header("Base Data")] [SerializeField] private float mInteractDistance;
		[SerializeField] private float mDefaultHealth;

		[Header("Startup Options")] [SerializeField] private bool mShouldInstantiateWeapon = true;
		[SerializeField] private bool mShouldInstantiateGravityGun = true;
		[SerializeField] private bool mInstantiateParts = true;

		[SerializeField] private AudioProfile mAudioProfile;
		public AudioProfile audioProfile { get { return mAudioProfile; } }

		public WeaponPartCollection defaultWeaponParts { get { return mDefaultWeaponParts; } }
		public float interactDistance { get { return mInteractDistance; } }
		public float defaultHealth { get { return mDefaultHealth; } }

		public bool makeGravGun { get { return mShouldInstantiateGravityGun; } }
		public bool makeWeaponGun { get { return mShouldInstantiateWeapon; } }
		public bool makeParts { get { return mInstantiateParts; } }
	}
}
