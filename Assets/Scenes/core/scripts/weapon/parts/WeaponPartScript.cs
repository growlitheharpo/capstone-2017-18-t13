using UnityEngine;
using FiringSquad.Data;

namespace FiringSquad.Gameplay
{
	public abstract class WeaponPartScript : MonoBehaviour
	{
		[SerializeField] private WeaponPartData[] mData;
		public WeaponPartData[] data { get { return mData; } }

		[SerializeField] private string mDescription;
		public string description { get { return mDescription; } }

		public abstract BaseWeaponScript.Attachment attachPoint { get; }
	}
}
