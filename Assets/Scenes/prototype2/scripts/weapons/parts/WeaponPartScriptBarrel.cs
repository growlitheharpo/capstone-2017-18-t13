using UnityEngine;

namespace Prototype2
{
	public class WeaponPartScriptBarrel : WeaponPartScript
	{
		public override BaseWeaponScript.Attachment attachPoint { get { return BaseWeaponScript.Attachment.Barrel; } }

		[SerializeField] private Transform mTip;
		public Transform barrelTip { get { return mTip; } }
	}
}
