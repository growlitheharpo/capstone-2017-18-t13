using FiringSquad.Data;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	public class WeaponPartScriptMechanism : WeaponPartScript
	{
		public override BaseWeaponScript.Attachment attachPoint { get { return BaseWeaponScript.Attachment.Mechanism; } }

		[SerializeField] private AudioProfile mAudioOverride;
		public AudioProfile audioOverride { get { return mAudioOverride; } }

		[SerializeField] private GameObject mProjectilePrefab;
		public GameObject projectilePrefab { get { return mProjectilePrefab; } }

		[Tooltip("If set to true, the weapon will NOT use the classic FPS hitscan method " +
				"for aiming. Instead, it will shoot directly from the end of the weapon.")] [SerializeField] private bool mFireFromBarrelTip;

		public bool overrideHitscanMethod { get { return mFireFromBarrelTip; } }
	}
}
