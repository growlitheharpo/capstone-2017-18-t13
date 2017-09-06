using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototype2
{
	public class PlayerWeaponScript : MonoBehaviour
	{
		[Serializable]
		public struct WeaponData
		{
			[SerializeField] private float mDefaultSpread;
			[SerializeField] private float mDefaultDamage;
			[SerializeField] private float mFireRate;
			[SerializeField] [Range(0.0f, 1.0f)] private float mDefaultRecoil;

			public float defaultSpread { get { return mDefaultSpread; } }
			public float defaultDamage { get { return mDefaultDamage; } }
			public float fireRate { get { return mFireRate; } }
			public float defaultRecoil { get { return mDefaultRecoil; } }

			public WeaponData(WeaponData other)
			{
				mDefaultSpread = other.mDefaultSpread;
			}

			// Clip size, reload speed
		}

		[SerializeField] private WeaponData mBaseData;
		[SerializeField] private Transform mBarrelAttach;
		[SerializeField] private Transform mScopeAttach;

		
	}
}
