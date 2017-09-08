using UnityEngine;

namespace Prototype2
{
	public class WeaponPartScript : MonoBehaviour
	{
		[SerializeField] private WeaponPartData[] mData;
		public WeaponPartData[] data { get { return mData; } }
	}
}
