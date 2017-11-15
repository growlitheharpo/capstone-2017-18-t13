using UnityEngine;

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

		/// <summary>
		/// Lateral movement speed.
		/// </summary>
		public float speed { get { return mSpeed; } }

		/// <summary>
		/// Time to enter or exit full-crouch.
		/// </summary>
		public float crouchSpeed { get { return mCrouchSpeed; } }
		
		/// <summary>
		/// The percent of our original height when in full crouch.
		/// </summary>
		public float crouchHeight { get { return mCrouchPercent; } }
		
		/// <summary>
		/// Amount of force to apply on jump.
		/// </summary>
		public float jumpForce { get { return mJumpForce; } }

		/// <summary>
		/// Force to stick the character to the ground with when going over ramps.
		/// </summary>
		public float stickToGroundForce { get { return mStickToGroundForce; } }

		/// <summary>
		/// Extra gravity multiplier to prevent floatiness.
		/// </summary>
		public float gravityMultiplier { get { return mGravityMultiplier; } }

		/// <summary>
		/// Default mouse look speed.
		/// </summary>
		public float lookSpeed { get { return mLookSpeed; } }
		
		/// <summary>
		/// Lateral move speed multiplier when sprinting.
		/// </summary>
		public float sprintMultiplier { get { return mSprintMultiplier; } }

		/// <summary>
		/// Lateral move speed multiplier when crouched.
		/// </summary>
		public float crouchMoveMultiplier { get { return mCrouchMoveMultiplier; } }

		/// <summary>
		/// Lateral move speed multiplier when in ADS
		/// </summary>
		public float aimDownSightsMoveMultiplier { get { return mAimDownSightsMoveMultiplier; } }

		/// <summary>
		/// Mouse look speed multiplier when in ADS.
		/// </summary>
		public float aimDownSightsLookMultiplier { get { return mAimDownSightsLookMultiplier; } }
	}
}
