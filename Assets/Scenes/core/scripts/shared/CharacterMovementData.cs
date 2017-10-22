﻿using UnityEngine;

namespace FiringSquad.Data
{
	/// <summary>
	/// Data utility to describe the motion of a character.
	/// </summary>
	[CreateAssetMenu(menuName = "Characters/Movement Data")]
	public class CharacterMovementData : ScriptableObject
	{
		[SerializeField] private float mSpeed;
		[SerializeField] private float mCrouchSpeed;
		[Tooltip("The percent of the normal heightto go to during crouching.")] [SerializeField] private float mCrouchPercent;
		[SerializeField] private float mCrouchMoveMultiplier;
		[SerializeField] private float mJumpForce;
		[SerializeField] private float mStickToGroundForce;
		[SerializeField] private float mGravityMultiplier;
		[SerializeField] private float mLookSpeed;
		[SerializeField] private float mSprintMultiplier;
		[SerializeField] private float mAimDownSightsMoveMultiplier;
		[SerializeField] private float mAimDownSightsLookMultiplier;


		public float speed { get { return mSpeed; } }
		public float crouchSpeed { get { return mCrouchSpeed; } }
		public float crouchHeight { get { return mCrouchPercent; } }
		public float jumpForce { get { return mJumpForce; } }
		public float stickToGroundForce { get { return mStickToGroundForce; } }
		public float gravityMultiplier { get { return mGravityMultiplier; } }
		public float lookSpeed { get { return mLookSpeed; } }
		public float sprintMultiplier { get { return mSprintMultiplier; } }
		public float crouchMoveMultiplier { get { return mCrouchMoveMultiplier; } }
		public float aimDownSightsMoveMultiplier { get { return mAimDownSightsMoveMultiplier; } }
		public float aimDownSightsLookMultiplier { get { return mAimDownSightsLookMultiplier; } }
	}
}
