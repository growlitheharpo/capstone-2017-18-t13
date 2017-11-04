using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	public class WeaponPartScriptScope : WeaponPartScript
	{
		public override BaseWeaponScript.Attachment attachPoint { get { return BaseWeaponScript.Attachment.Scope; } }

		[SerializeField] private Vector3 mAimDownSightsPosition;
		private Coroutine mMoveRoutine;

		public void ActivateAimDownSightsEffect(IWeapon weapon)
		{
			Transform subView = weapon.transform.Find("View").GetChild(0);
			if (mMoveRoutine != null)
				StopCoroutine(mMoveRoutine);

			mMoveRoutine = StartCoroutine(Coroutines.LerpPosition(subView, mAimDownSightsPosition, 0.2f, Space.Self, Coroutines.MATHF_SMOOTHSTEP));
		}

		public void DeactivateAimDownSightsEffect(IWeapon weapon, bool immediate = false)
		{
			Transform subView = weapon.transform.Find("View").GetChild(0);
			if (mMoveRoutine != null)
				StopCoroutine(mMoveRoutine);

			if (!immediate)
				mMoveRoutine = StartCoroutine(Coroutines.LerpPosition(subView, Vector3.zero, 0.2f, Space.Self, Coroutines.MATHF_SMOOTHSTEP));
			else
				subView.transform.localPosition = Vector3.zero;
		}
	}
}
