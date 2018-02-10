using JetBrains.Annotations;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptBarrel : WeaponPartScript
	{
		/// Inspector variables
		[SerializeField] private Transform mTip;
		[SerializeField] private int mProjectileCount = 1;
		[SerializeField] private int mShotsPerClick = -1;
		[SerializeField] private bool mOverrideRecoilCurve;
		[HideInInspector] [SerializeField] private AnimationCurve mRecoilCurve;
		
		/// <inheritdoc />
		public override Attachment attachPoint { get { return Attachment.Barrel; } }

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
	}
}
