using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

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
		[FormerlySerializedAs("mDurabilitySprite")][SerializeField] private Sprite mAmmoTypeSprite;
		[SerializeField] private Animator mAnimator;

		/// <inheritdoc />
		public override Attachment attachPoint { get { return Attachment.Mechanism; } }

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

		/// <summary>
		/// The UI image to attach to the HUD when this mechanism is active.
		/// </summary>
		public Sprite ammoTypeSprite {get { return mAmmoTypeSprite; }}

		/// <summary>
		/// The animator for this mechanism.
		/// Has FireRate parameter and Fire trigger.
		/// Can be null if not assigned in editor.
		/// </summary>
		[CanBeNull]
		public Animator attachedAnimator { get { return mAnimator; } }
	}
}
