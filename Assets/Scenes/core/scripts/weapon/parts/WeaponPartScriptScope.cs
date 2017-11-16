using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.Weapons
{
	/// <inheritdoc />
	public class WeaponPartScriptScope : WeaponPartScript
	{
		/// Inspector variables
		[SerializeField] private Vector3 mAimDownSightsPosition;

		/// Private variables
		private Coroutine mMoveRoutine;

		/// <inheritdoc />
		public override BaseWeaponScript.Attachment attachPoint { get { return BaseWeaponScript.Attachment.Scope; } }

		/// <summary>
		/// Activate the Aim Down Sights effect for this script.
		/// TODO: Have a switch in here for multiple effect types.
		/// </summary>
		/// <param name="weapon">The weapon we are attached to.</param>
		public void ActivateAimDownSightsEffect(IWeapon weapon)
		{
			Transform subView = weapon.transform.Find("View").GetChild(0);
			if (mMoveRoutine != null)
				StopCoroutine(mMoveRoutine);

			mMoveRoutine = StartCoroutine(Coroutines.LerpPosition(subView, mAimDownSightsPosition, 0.2f, Space.Self, Coroutines.MATHF_SMOOTHSTEP));
		}

		/// <summary>
		/// Deativate the Aim Down Sights effect for this script.
		/// TODO: Have a switch in here for multiple effect types.
		/// </summary>
		/// <param name="weapon">The weapon we are attached to.</param>
		/// <param name="immediate">Whether or not to jump immediately to the "exit" state instead of lerping.</param>
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
