using UnityEngine;

namespace Prototype2
{
	/// <summary>
	/// Component strictly for handling the player's weapon
	/// and how it behaves.
	/// </summary>
	/// <inheritdoc />
	public class PlayerWeaponScript : BaseWeaponScript
	{
		[SerializeField] private ParticleSystem mShotParticles;
		private Vector3 mPlayerEyeOffset;

		private const float CAMERA_FOLLOW_FACTOR = 10.0f;
		
		private void Start()
		{
			//mAimRoot = Camera.main.transform;
			//mPlayerEyeOffset = mAimRoot.InverseTransformPoint(transform.position);
			mPlayerEyeOffset = Camera.main.transform.InverseTransformPoint(transform.position);
			EventManager.OnConfirmPartAttach += AttachNewPart;
		}

		private void OnDestroy()
		{
			EventManager.OnConfirmPartAttach -= AttachNewPart;
		}
		
		/// <summary>
		/// Play any SFX and VFX associated with the weapon based on its current mods.
		/// </summary>
		protected override void PlayShotEffect()
		{
			mShotParticles.Stop();
			mShotParticles.time = 0.0f;
			mShotParticles.Play();
		}
		
		protected override void Update()
		{
			base.Update();
			FollowCamera();
		}

		/// <summary>
		/// Lerp our position and rotation to match the camera.
		/// </summary>
		private void FollowCamera()
		{
			Vector3 location = transform.position;
			Vector3 targetLocation = bearer.eye.TransformPoint(mPlayerEyeOffset);

			Quaternion rotation = transform.rotation;
			Quaternion targetRotation = bearer.eye.rotation;

			transform.position = Vector3.Lerp(location, targetLocation, Time.deltaTime * CAMERA_FOLLOW_FACTOR);
			transform.rotation = Quaternion.Lerp(rotation, targetRotation, Time.deltaTime * CAMERA_FOLLOW_FACTOR);
		}
	}
}
