using UnityEngine;

/// <summary>
/// Data utility to describe the motion of a character.
/// </summary>
[CreateAssetMenu(menuName = "Characters/Movement Data")]
public class CharacterMovementData : ScriptableObject
{
	[SerializeField] private float mForwardSpeed;
	[SerializeField] private float mBackwardSpeed;
	[SerializeField] private float mStrafeSpeed;
	[SerializeField] private float mCrouchSpeed;
	[Tooltip("The percent of the normal height to go to during crouching.")]
	[SerializeField] private float mCrouchPercent;
	[SerializeField] private float mJumpForce;
	[SerializeField] private float mLookSpeed;

	public float forwardSpeed { get { return mForwardSpeed; } }
	public float backwardSpeed { get { return mBackwardSpeed; } }
	public float strafeSpeed { get { return mStrafeSpeed; } }
	public float crouchSpeed { get { return mCrouchSpeed; } }
	public float crouchHeight { get { return mCrouchPercent; } }
	public float jumpForce { get { return mJumpForce; } }
	public float lookSpeed { get { return mLookSpeed; } }
}
