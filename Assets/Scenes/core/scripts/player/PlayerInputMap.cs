using System;
using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// The map that binds the string axes in Unity to code.
	/// </summary>
	[Serializable]
	public class PlayerInputMap
	{
		[Header("Weapons and Interactions")] [SerializeField] private string mToggleMenuButton;
		[SerializeField] private string mFireWeaponButton;
		[SerializeField] private string mFireGravGunButton;
		[SerializeField] private string mActivateADSButton;
		[SerializeField] private string mReloadButton;
		[SerializeField] private string mInteractButton;
		[SerializeField] private string mPauseButton;

		[Header("Movement")] [SerializeField] private string mMoveBackFrontAxis;
		[SerializeField] private string mMoveSidewaysAxis;
		[SerializeField] private string mLookUpDownAxis;
		[SerializeField] private string mLookLeftRightAxis;
		[SerializeField] private string mJumpButton;
		[SerializeField] private string mCrouchButton;
		[SerializeField] private string mSprintButton;
		[SerializeField] private bool mStickySprint;

		public string toggleMenuButton { get { return mToggleMenuButton; } }
		public string fireWeaponButton { get { return mFireWeaponButton; } }
		public string fireGravGunButton { get { return mFireGravGunButton; } }
		public string activateADSButton { get { return mActivateADSButton; } }
		public string reloadButton { get { return mReloadButton; } }
		public string interactButton { get { return mInteractButton; } }
		public string pauseButton { get { return mPauseButton; } }

		public string moveBackFrontAxis { get { return mMoveBackFrontAxis; } }
		public string moveSidewaysAxis { get { return mMoveSidewaysAxis; } }
		public string lookUpDownAxis { get { return mLookUpDownAxis; } }
		public string lookLeftRightAxis { get { return mLookLeftRightAxis; } }
		public string jumpButton { get { return mJumpButton; } }
		public string crouchButton { get { return mCrouchButton; } }
		public string sprintButton { get { return mSprintButton; } }
		public bool stickySprint { get { return mStickySprint; } }
	}
}
