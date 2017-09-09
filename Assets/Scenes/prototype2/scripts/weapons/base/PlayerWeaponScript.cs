using System.Collections;
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
			mAimRoot = Camera.main.transform;
			mPlayerEyeOffset = mAimRoot.InverseTransformPoint(transform.position);
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

		/// <summary>
		/// Play any SFX, VFX, and Animations for reloading.
		/// </summary>
		protected override void PlayReloadEffect()
		{
			//presumably, reloading will be handled by animation.
			//normally, we'll wait until we get a callback from it. For now,
			//we'll fake it.
			StartCoroutine(DELETEME_WaitForReload());
		}

		private IEnumerator DELETEME_WaitForReload()
		{
			yield return new WaitForSeconds(1.5f);
			OnReloadComplete();
		}

		private void OnGUI()
		{
			if (mShotTime < 1000.0f)
				return;

			float width = Screen.width, height = Screen.height;
			GUILayout.BeginArea(new Rect(width * 0.7f, height * 0.5f, width * 0.2f, height * 0.5f));

			GUILayout.Label("RELOADING");

			GUILayout.EndArea();
		}

		protected override void Update()
		{
			base.Update();
			FollowCamera();

			if (Input.GetKeyDown(KeyCode.R))
				Reload();
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
