using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptMechanism : WeaponPartScript
	{
		/// Inspector variables
		[SerializeField] private float mAudioOverrideWeaponType;
		[SerializeField] private GameObject mProjectilePrefab;
		[Tooltip("If set to true, the weapon will NOT use the classic FPS hitscan method " +
				"for aiming. Instead, it will shoot directly from the end of the weapon.")]
		[SerializeField] private bool mFireFromBarrelTip;

		/// <inheritdoc />
		public override BaseWeaponScript.Attachment attachPoint { get { return BaseWeaponScript.Attachment.Mechanism; } }

		/// <summary>
		/// Which audio weapon type to use for this mechanism.
		/// </summary>
		public float audioOverrideWeaponType { get { return mAudioOverrideWeaponType; } }

		/// <summary>
		/// The prefab for the projectile fired form this weapon.
		/// </summary>
		public GameObject projectilePrefab { get { return mProjectilePrefab; } }

		/// <summary>
		/// Whether or not to force the weapon to fire from barrel instead of the player's eye.
		/// </summary>
		public bool overrideHitscanMethod { get { return mFireFromBarrelTip; } }
	}
}
