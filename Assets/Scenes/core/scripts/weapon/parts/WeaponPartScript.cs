using FiringSquad.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace FiringSquad.Gameplay.Weapons
{
	public abstract class WeaponPartScript : MonoBehaviour
	{
		public const int INFINITE_DURABILITY = -1;

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

		public abstract BaseWeaponScript.Attachment attachPoint { get; }

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

			// Destroy the pickup script (like when calling "interact")
			Destroy(copy.GetComponent<WeaponPickupScript>());
			Destroy(copy.GetComponent<Rigidbody>());
			Destroy(copy.GetComponent<NetworkTransform>());
			Destroy(copy.GetComponent<NetworkIdentity>());

			return copy.GetComponent<WeaponPartScript>();
		}
	}
}
