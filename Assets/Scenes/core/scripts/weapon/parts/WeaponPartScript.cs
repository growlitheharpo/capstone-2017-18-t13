using System;
using FiringSquad.Core;
using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	/// <summary>
	/// The base script that handles all of the gameplay aspects of a weapon part.
	/// Considered to primarily be the "On The Gun" script.
	/// </summary>
	public abstract class WeaponPartScript : MonoBehaviour
	{
		public const int INFINITE_DURABILITY = -1;
		public const int USE_DEFAULT_DURABILITY = -2;

		/// Inspector variables
		[SerializeField] private byte mUniqueId;
		[SerializeField] private string mPrettyName;
		[SerializeField] private string mDescription;
		[SerializeField] private WeaponPartData mData;
		[SerializeField] private Sprite mDurabilitySprite;
		[SerializeField] private int mDurability = INFINITE_DURABILITY;

		/// Private variables
		private BoundProperty<float> mDurabilityPercent = new BoundProperty<float>();
		private int mBaseDurability;

		/// <summary>
		/// Which attachment point we connect to.
		/// </summary>
		public abstract BaseWeaponScript.Attachment attachPoint { get; }

		/// <summary>
		/// The collection of modifier data for this part.
		/// </summary>
		public WeaponPartData[] data { get { return new[] { mData }; } }

		/// <summary>
		/// The UI sprite used to represent this part in the durability HUD.
		/// </summary>
		public Sprite durabilitySprite { get { return mDurabilitySprite; } }

		/// <summary>
		/// The unique part ID of this weapon part.
		/// </summary>
		public byte partId { get { return mUniqueId; } }

		/// <summary>
		/// A short text description of this part. Used for UI.
		/// </summary>
		public string description { get { return mDescription; } }

		/// <summary>
		/// A short name for this part. Used for UI.
		/// </summary>
		public string prettyName { get { return mPrettyName; } }

		/// <summary>
		/// The current durability of this weapon part.
		/// </summary>
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


		/// <summary>
		/// Cleanup all listeners and event handlers
		/// </summary>
		private void OnDestroy()
		{
			mDurabilityPercent.Cleanup(); // force this so that the UI is unbound
		}

		/// <summary>
		/// Called on a prefab obtained from the IWeaponPartManager service.
		/// Returns an instantiated copy of this part, intended to exist with physics in the game world.
		/// </summary>
		public GameObject SpawnInWorld()
		{
			GameObject copy = Instantiate(gameObject);
			copy.name = name;

			// let the pickup script self-initialize in Start()

			return copy;
		}

		/// <summary>
		/// Called on a prefab obtained from the IWeaponPartManager service.
		/// Returns an instantiated copy of this part, intended to immediately be attached to a weapon.
		/// </summary>
		/// <param name="weapon">The weapon this part will be attached to.</param>
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

		/// <summary>
		/// Bind the durability of an instantiated part to the durability HUD.
		/// </summary>
		private void BindDurabilityToUI()
		{
			mBaseDurability = durability;
			mDurabilityPercent = new BoundProperty<float>(1.0f, ("player_part_durability_" + attachPoint.ToString().ToLower()).GetHashCode());
		}

		#region Serialization

		/// <summary>
		/// Write the unique ID of this weapon part.
		/// TODO: Use something more space-efficient than a string!
		/// </summary>
		public void SerializeId(NetworkWriter writer)
		{
			writer.Write(partId);
		}

		/// <summary>
		/// Read a unique ID of a part from the stream.
		/// </summary>
		public static byte DeserializeId(NetworkReader reader)
		{
			return reader.ReadByte();
		}

		/// <summary>
		/// Write the durability of this weapon part.
		/// Note: durability will be cast to a byte, and must be less than 255.
		/// </summary>
		public void SerializeDurability(NetworkWriter writer)
		{
			if (mDurability > byte.MaxValue)
				throw new ArgumentException("Durability cannot be higher than " + byte.MaxValue);

			writer.Write((byte)mDurability);
		}

		/// <summary>
		/// Read the durability of a weapon part from the stream.
		/// </summary>
		public static int DeserializeDurability(NetworkReader reader)
		{
			return reader.ReadByte();
		}
		
		#endregion
	}
}
