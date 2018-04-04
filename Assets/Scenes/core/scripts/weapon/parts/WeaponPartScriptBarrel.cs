using JetBrains.Annotations;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptBarrel : WeaponPartScript
	{
		/// Inspector variables
		[SerializeField] private Transform mTip;
		[SerializeField] private float mAudioOverrideBarrelType;
		[SerializeField] private int mProjectileCount = 1;
		[SerializeField] private int mShotsPerClick = -1;
		[SerializeField] private Sprite mCrosshairSprite;
		[SerializeField] private Animator mAnimator;
		[SerializeField] private bool mOverrideRecoilCurve;
		[HideInInspector] [SerializeField] private AnimationCurve mRecoilCurve;

		/// <inheritdoc />
		public override Attachment attachPoint { get { return Attachment.Barrel; } }

		/// <summary>
		/// Which audio weapon type to use for this mechanism.
		/// </summary>
		public float audioOverrideBarrelType { get { return mAudioOverrideBarrelType; } }

		/// <summary>
		/// The Transform of the tip of this barrel, where effects should originate from.
		/// </summary>
		public Transform barrelTip { get { return mTip; } }

		/// <summary>
		/// The number of projectiles spawned per shot with this weapon.
		/// </summary>
		public int projectileCount { get { return mProjectileCount; } }

		/// <summary>
		/// The number of shots this weapon can fire before the player must re-click the trigger.
		/// -1 indicates an infinite number, or a fully-automatic weapon.
		/// </summary>
		public int shotsPerClick { get { return mShotsPerClick; } }

		/// <summary>
		/// The new recoil curve that this barrel creates.
		/// If null, use the default recoil curve.
		/// </summary>
		[CanBeNull] public AnimationCurve recoilCurve { get { return mOverrideRecoilCurve ? mRecoilCurve : null; } }

		/// <summary>
		/// The UI image to use as our crosshair when this barrel is active.
		/// </summary>
		public Sprite crosshairSprite { get { return mCrosshairSprite; } }

		/// <summary>
		/// The animator for this barrel.
		/// Has FireRate parameter and Fire trigger.
		/// Can be null if not assigned in editor.
		/// </summary>
		[CanBeNull]
		public Animator attachedAnimator { get { return mAnimator; } }
	}
}
