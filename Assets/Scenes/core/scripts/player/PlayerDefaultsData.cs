﻿using System;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// Collection of useful/necessary data for balancing and tweaking gameplay.
	/// </summary>
	[Serializable]
	public class PlayerDefaultsData
	{
		[Header("Weapons")] [SerializeField] private WeaponPartCollection mDefaultWeaponParts;

		[Header("Base Data")] [SerializeField] private float mInteractDistance;
		[SerializeField] private float mDefaultHealth;

		[SerializeField] private GameObject mThirdPersonView;
		[SerializeField] private GameObject mFirstPersonView;
		[SerializeField] private Transform mFirstPersonWeaponBone;

		[SerializeField] private Color mBlueTeamColor;
		[SerializeField] private Color mOrangeTeamColor;

		/// <summary> The default collection of parts for this player. </summary>
		public WeaponPartCollection defaultWeaponParts { get { return mDefaultWeaponParts; } }

		/// <summary> The range of the invisible interact "gun". </summary>
		public float interactDistance { get { return mInteractDistance; } }

		/// <summary> The health that this player should spawn with. </summary>
		public float defaultHealth { get { return mDefaultHealth; } }

		/// <summary>
		/// The GameObject holding the player's third person view.
		/// </summary>
		public GameObject thirdPersonView { get { return mThirdPersonView; } }

		/// <summary>
		/// The GameObject holding the player's first person view (hand)
		/// </summary>
		public GameObject firstPersonView { get { return mFirstPersonView; } }

		/// <summary>
		/// The Transform bone under which the weapon view will be moved.
		/// </summary>
		public Transform firstPersonWeaponBone { get { return mFirstPersonWeaponBone; } }

		/// <summary>
		/// The color to apply if we are a member of the blue team.
		/// </summary>
		public Color blueTeamColor { get { return mBlueTeamColor; } }

		/// <summary>
		/// The color to apply if we are a member of the orange team.
		/// </summary>
		public Color orangeTeamColor { get { return mOrangeTeamColor; } }
	}
}
