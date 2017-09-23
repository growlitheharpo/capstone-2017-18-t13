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
	}
}
