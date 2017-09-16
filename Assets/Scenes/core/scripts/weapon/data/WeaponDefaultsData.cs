﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FiringSquad.Data
{
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
		/// <returns></returns>
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
	}
}
