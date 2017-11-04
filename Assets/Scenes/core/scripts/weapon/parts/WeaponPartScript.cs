﻿using System;
using FiringSquad.Core;
using FiringSquad.Core.Weapons;
using FiringSquad.Data;
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
		private BoundProperty<float> mDurabilityPercent = new BoundProperty<float>();
		private int mBaseDurability;

		public int durability
		{
			get
			{
				return mDurability;
			}
			set
			{
				mDurability = value;
				if (mBaseDurability <= 0)
					return;

				float percent = value / (float)mBaseDurability;
				mDurabilityPercent.value = percent;
			}
		}

		public string partId
		{
			get
			{
				if (gameObject.name.Contains("(Clone)"))
					return gameObject.name.Replace("(Clone)", "");
				return gameObject.name;
			}
		}

		private void OnDestroy()
		{
			mDurabilityPercent.Cleanup(); // force this so that the UI is unbound
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

			WeaponPartScript script = copy.GetComponent<WeaponPartScript>();
			if (weapon.bearer != null && weapon.bearer.isCurrentPlayer)
				script.BindDurabilityToUI();

			return script;
		}

		private void BindDurabilityToUI()
		{
			mBaseDurability = durability;
			mDurabilityPercent = new BoundProperty<float>(1.0f, ("player_part_durability_" + attachPoint.ToString().ToLower()).GetHashCode());
		}

		#region Serialization

		public void SerializeId(NetworkWriter writer)
		{
			writer.Write(partId);
		}

		public static string DeserializeId(NetworkReader reader)
		{
			return reader.ReadString();
		}

		public void SerializeDurability(NetworkWriter writer)
		{
			if (mDurability > byte.MaxValue)
				throw new ArgumentException("Durability cannot be higher than " + byte.MaxValue);

			writer.Write((byte)mDurability);
		}

		public static int DeserializeDurability(NetworkReader reader)
		{
			return reader.ReadByte();
		}
		
		#endregion
	}
}
