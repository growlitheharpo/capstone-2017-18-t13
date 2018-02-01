using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	/// <summary>
	/// Component for pads that provide weapon parts.
	/// Explodes and spawns a part upon receiving damage.
	/// </summary>
	public class WeaponPartPad : MonoBehaviour
	{
		/// <summary>
		/// Struct to bind a prefab to a weight (0 - 1).
		/// </summary>
		[Serializable]
		public struct PartWeightSet
		{
			[SerializeField] private GameObject mPrefab;
			[Range(0.0f, 1.0f)] [SerializeField] private float mWeight;

			public GameObject prefab { get { return mPrefab; } }
			public float weight { get { return mWeight; } }
		}

		/// Inspector variables
		[HideInInspector] [SerializeField] private List<PartWeightSet> mParts; // [HideInInspector] because this is drawn with custom editor
		[SerializeField] private Transform mSpawnPoint;
		[SerializeField] private float mRespawnTime;

		/// Private variables
		private Light mLight;
		private Material mPadMaterial;

		// Use this for initialization
		void Start() { }

		// Update is called once per frame
		void Update() { }
	}
}
