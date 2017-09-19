using System;
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
		
		[SerializeField] private int mDurability = -1;

		public int durability
		{
			get
			{
				return mDurability;
			}
			set
			{
				if (mDurability == -1)
					throw new ArgumentException("Durability cannot be modified for this part!");

				mDurability = value;
			}
		}

		public abstract BaseWeaponScript.Attachment attachPoint { get; }
	}
}
