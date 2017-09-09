using UnityEngine;

namespace Prototype2
{
	public abstract class WeaponPartScript : MonoBehaviour
	{
		[SerializeField] private WeaponPartData[] mData;
		public WeaponPartData[] data { get { return mData; } }

		public abstract BaseWeaponScript.Attachment attachPoint { get; }
	}
}
