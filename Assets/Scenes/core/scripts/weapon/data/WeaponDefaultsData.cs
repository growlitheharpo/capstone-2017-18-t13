using System;
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
	}
}
