using System;
using FiringSquad.Gameplay.Weapons;
using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.NPC
{
	[Serializable]
	public class NpcTurretData
	{
		[Header("Targeting")]
		[SerializeField] private float mTargetingRange;
		[SerializeField] private float mTargetingCone;
		[SerializeField] private float mTargetingDampening;

		[Header("Other")]
		[SerializeField] private LayerMask mVisibilityMask;
		[SerializeField] private float mDefaultHealth;
		[SerializeField] private float mRespawnTime;
		[SerializeField] [EnumFlags] private BaseWeaponScript.Attachment mPartsToDrop;

		public float targetingRange { get { return mTargetingRange; } }
		public float targetingCone { get { return mTargetingCone; } }
		public float targetingSpeed { get { return mTargetingDampening; } }
		public float defaultHealth { get { return mDefaultHealth; } }
		public float respawnTime { get { return mRespawnTime; } }
		public LayerMask visibilityMask { get { return mVisibilityMask; } }

		public BaseWeaponScript.Attachment partsToDrop { get { return mPartsToDrop; } }
	}
}
