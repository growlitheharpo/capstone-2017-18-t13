using FiringSquad.Gameplay;
using UnityEngine;

namespace FiringSquad.Gameplay
{
	public class WeaponPartScriptBarrel : WeaponPartScript
	{
		public override BaseWeaponScript.Attachment attachPoint { get { return BaseWeaponScript.Attachment.Barrel; } }

		[SerializeField] private Transform mTip;
		public Transform barrelTip { get { return mTip; } }

		[SerializeField] private int mProjectileCount = 1;
		public int projectileCount { get { return mProjectileCount; } }

		[SerializeField] private int mShotsPerClick = -1;
		public int shotsPerClick { get { return mShotsPerClick; } }

		[SerializeField] private bool mOverrideRecoilCurve;

		[HideInInspector] [SerializeField] private AnimationCurve mRecoilCurve;
		public AnimationCurve recoilCurve { get { return mOverrideRecoilCurve ? mRecoilCurve : null; } }
	}
}
