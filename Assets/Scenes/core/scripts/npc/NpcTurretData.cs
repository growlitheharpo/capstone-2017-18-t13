using System;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.NPC
{
	/// <summary>
	/// Holds all of the raw data for the NPC turrets that is used for balancing
	/// and gameplay tweaks.
	/// </summary>
	[Serializable]
	public class NpcTurretData
	{
		/// Inspector variables
		[Header("Targeting")]
		[SerializeField] private float mTargetingRange;
		[SerializeField] private float mTargetingCone;
		[SerializeField] private float mTargetingDampening;
		[SerializeField] private float mWeaponHoldTime;
		[SerializeField] private float mWeaponUpTime;

		[Header("Other")]
		[SerializeField] private LayerMask mVisibilityMask;
		[SerializeField] private float mDefaultHealth;
		[SerializeField] private float mRespawnTime;
		[SerializeField] [EnumFlags] private Attachment mPartsToDrop;

		/// <summary> How far the turret is able to "see" targets. </summary>
		public float targetingRange { get { return mTargetingRange; } }

		/// <summary> How far to the side the turret is able to "see" targets. </summary>
		public float targetingCone { get { return mTargetingCone; } }
		
		/// <summary> The rotation speed of this turret when locked onto targets. </summary>
		public float targetingSpeed { get { return mTargetingDampening; } }
		
		/// <summary> The amount of time this turret can hold down its trigger. </summary>
		public float weaponHoldTime { get { return mWeaponHoldTime; } }
		
		/// <summary> The "cooldown" period after this turret has been holding its trigger. </summary>
		public float weaponUpTime { get { return mWeaponUpTime; } }
		
		/// <summary> The starting health for this turret. </summary>
		public float defaultHealth { get { return mDefaultHealth; } }

		/// <summary> The amount of time after dying before this turret respawns, in seconds. </summary>
		public float respawnTime { get { return mRespawnTime; } }

		/// <summary> Which physics layers block this turret's visibility. </summary>
		public LayerMask visibilityMask { get { return mVisibilityMask; } }

		/// <summary> Which parts this turret should drop when it dies. </summary>
		public Attachment partsToDrop { get { return mPartsToDrop; } }
	}
}
