using System;
using System.Collections.Generic;
using FiringSquad.Gameplay;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// Serializable utility class that stores a collection of weapon parts.
	/// </summary>
	[Serializable]
	public class WeaponDefaultsData
	{
		[SerializeField] private GameObject mScope;
		[SerializeField] private GameObject mBarrel;
		[SerializeField] private GameObject mMechanism;
		[SerializeField] private GameObject mGrip;

		public GameObject scope { get { return mScope; } }
		public GameObject barrel { get { return mBarrel; } }
		public GameObject mechanism { get { return mMechanism; } }
		public GameObject grip { get { return mGrip; } }

		/// <summary>
		/// Allows this class to be iterated over.
		/// </summary>
		public IEnumerator<GameObject> GetEnumerator()
		{
			yield return scope;
			yield return barrel;
			yield return mechanism;
			yield return grip;
		}

		public WeaponDefaultsData(WeaponDefaultsData copy)
		{
			mScope = copy.mScope;
			mBarrel = copy.mBarrel;
			mMechanism = copy.mMechanism;
			mGrip = copy.mGrip;
		}

		/// <summary>
		/// Allows access to weapon parts by their attachment.
		/// </summary>
		public GameObject this[BaseWeaponScript.Attachment index]
		{
			get
			{
				switch (index)
				{
					case BaseWeaponScript.Attachment.Scope:
						return mScope;
					case BaseWeaponScript.Attachment.Barrel:
						return mBarrel;
					case BaseWeaponScript.Attachment.Mechanism:
						return mMechanism;
					case BaseWeaponScript.Attachment.Grip:
						return mGrip;
					default:
						throw new ArgumentOutOfRangeException("index", index, null);
				}
			}
		}
	}
}
