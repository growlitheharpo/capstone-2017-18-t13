using System;
using UnityEngine;
using FiringSquad.Data;

namespace FiringSquad.Gameplay
{
	public abstract class WeaponPartScript : MonoBehaviour
	{
		public const int INFINITE_DURABILITY = -1;

		[SerializeField] private WeaponPartData mData;
		public WeaponPartData[] data { get { return new [] { mData }; } }

		[SerializeField] private string mDescription;
		public string description { get { return mDescription; } }
		
		[SerializeField] private int mDurability = INFINITE_DURABILITY;

		public int durability
		{
			get
			{
				return mDurability;
			}
			set
			{
				mDurability = value;
			}
		}

		public abstract BaseWeaponScript.Attachment attachPoint { get; }

		private static bool doOnce = false;
		void Awake()
		{
			if (doOnce)
				return;

			doOnce = true;
			SpawnInWorld();
			Destroy(gameObject);
		}

		public GameObject SpawnInWorld()
		{
			GameObject copy = Instantiate(gameObject);
			copy.name = name;

			// initialize the pickup script
			WeaponPickupScript pickup = copy.GetComponent<WeaponPickupScript>();
			pickup.InitializePickupView();

			return copy;
		}

		public WeaponPartScript SpawnForWeapon(BaseWeaponScript weapon)
		{
			GameObject copy = Instantiate(gameObject);
			copy.name = name;

			// Destroy the pickup script
			WeaponPickupScript pickup = copy.GetComponent<WeaponPickupScript>();
			pickup.DestroyPickupView();

			return copy.GetComponent<WeaponPartScript>();
		}
	}
}
