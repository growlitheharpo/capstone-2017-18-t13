﻿using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	public abstract class WeaponPartScript : MonoBehaviour
	{
		public const int INFINITE_DURABILITY = -1;
		public const int USE_DEFAULT_DURABILITY = -2;

		public abstract BaseWeaponScript.Attachment attachPoint { get; }

		[SerializeField] private Sprite mDurabilitySprite;
		public Sprite durabilitySprite { get { return mDurabilitySprite; } }

		[SerializeField] private WeaponPartData mData;
		public WeaponPartData[] data { get { return new[] { mData }; } }

		[SerializeField] private string mDescription;
		public string description { get { return mDescription; } }

		[SerializeField] private string mPrettyName;
		public string prettyName { get { return mPrettyName; } }

		[SerializeField] private int mDurability = INFINITE_DURABILITY;

		public int durability { get { return mDurability; } set { mDurability = value; } }

		public string partId
		{
			get
			{
				if (gameObject.name.Contains("(Clone)"))
					return gameObject.name.Replace("(Clone)", "");
				return gameObject.name;
			}
		}


		public GameObject SpawnInWorld()
		{
			GameObject copy = Instantiate(gameObject);
			copy.name = name;

			// let the pickup script self-initialize in Start()

			return copy;
		}

		public WeaponPartScript SpawnForWeapon(BaseWeaponScript weapon)
		{
			GameObject copy = Instantiate(gameObject);
			copy.name = name;

			// Destroy the pickup script (like when calling "interact")
			Destroy(copy.GetComponent<WeaponPickupScript>());
			Destroy(copy.GetComponent<Rigidbody>());
			Destroy(copy.GetComponent<NetworkTransform>());
			Destroy(copy.GetComponent<NetworkIdentity>());

			return copy.GetComponent<WeaponPartScript>();
		}
	}
}
