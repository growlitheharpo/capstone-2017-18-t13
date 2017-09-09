using UnityEngine;

namespace Prototype2
{
	public class WeaponPartScriptMechanism : WeaponPartScript
	{
		public override BaseWeaponScript.Attachment attachPoint { get { return BaseWeaponScript.Attachment.Mechanism; } }

		[SerializeField] private GameObject mProjectilePrefab;
		public GameObject projectilePrefab { get { return mProjectilePrefab; } }
	}
}
