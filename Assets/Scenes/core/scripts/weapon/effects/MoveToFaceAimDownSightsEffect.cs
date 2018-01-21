using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	[CreateAssetMenu(menuName = "reMod Data/ADS Effect - Move Weapon")]
	public class MoveToFaceAimDownSightsEffect : AimDownSightsEffect
	{
		/// Inspector variables
		[SerializeField] private Vector3 mAimDownSightsPosition;

		/// Private variables
		private Coroutine mMoveRoutine;
		private IWeapon mCurrentWeapon;
		private bool mActive;

		/// <inheritdoc />
		public override void ActivateEffect(IWeapon weapon, WeaponPartScript part)
		{
			if (mActive)
				return;

			mCurrentWeapon = weapon;
			mActive = true;
			base.ActivateEffect(weapon, part);

			EventManager.Notify(() => EventManager.LocalGUI.SetCrosshairVisible(false));
			EventManager.Notify(() => EventManager.LocalGUI.RequestNewFieldOfView(25.0f, 0.25f));

			Transform subView = weapon.transform.Find("View").GetChild(0);
			if (mMoveRoutine != null)
				part.StopCoroutine(mMoveRoutine);

			mMoveRoutine = part.StartCoroutine(Coroutines.LerpPosition(subView, mAimDownSightsPosition, 0.2f, Space.Self, Coroutines.MATHF_SMOOTHSTEP));
		}

		/// <inheritdoc />
		public override void DeactivateEffect(WeaponPartScript part, bool immediate)
		{
			if (!mActive)
				return;

			mActive = false;
			EventManager.Notify(() => EventManager.LocalGUI.SetCrosshairVisible(true));
			EventManager.Notify(() => EventManager.LocalGUI.RequestNewFieldOfView(-1.0f, 0.25f));

			Transform subView = mCurrentWeapon.transform.Find("View").GetChild(0);
			if (mMoveRoutine != null)
				part.StopCoroutine(mMoveRoutine);

			if (!immediate)
				mMoveRoutine = part.StartCoroutine(Coroutines.LerpPosition(subView, Vector3.zero, 0.2f, Space.Self, Coroutines.MATHF_SMOOTHSTEP));
			else
				subView.transform.localPosition = Vector3.zero;
		}
	}
}
