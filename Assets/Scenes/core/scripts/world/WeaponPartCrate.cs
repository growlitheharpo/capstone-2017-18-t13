using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class WeaponPartCrate : NetworkBehaviour
	{
		[Serializable]
		public struct PartWeightSet
		{
			public GameObject mPrefab;
			[Range(0.0f, 1.0f)] public float mWeight;
		}

		[HideInInspector] [SerializeField] List<PartWeightSet> mParts;
		[SerializeField] private GameObject mBreakVFX;
		public float mRespawnTime;

		// Use this for initialization
		void Start() { }

		// Update is called once per frame
		void Update() { }
	}
}
