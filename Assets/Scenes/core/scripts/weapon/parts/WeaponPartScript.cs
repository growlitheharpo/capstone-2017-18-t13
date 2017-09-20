using System;
using UnityEngine;
using FiringSquad.Data;

namespace FiringSquad.Gameplay
{
	public abstract class WeaponPartScript : MonoBehaviour
	{
		public const int INFINITE_DURABILITY = -1;

		[SerializeField] private WeaponPartData[] mData;
		public WeaponPartData[] data { get { return mData; } }

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
	}
}
