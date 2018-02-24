using System.Collections;
using System.Linq;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class for displaying the name of a weapon part, and if you already have it.
	/// </summary>
	public class WeaponPartWorldCanvas : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private UIText mPartText;
		[SerializeField] private UIText mAlreadyHasText;
		[SerializeField] private CanvasGroup mCanvasGroup;

		/// Private variables
		private Transform mPlayerRef;
		private BaseWeaponScript mPlayerWeapon;
		private WeaponPartScript mLinkedPart;
		private float mMaxAlpha;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			StartCoroutine(GrabPlayerReference());
			mAlreadyHasText.gameObject.SetActive(false);
			mCanvasGroup.gameObject.SetActive(false);
			mMaxAlpha = 1.0f;
		}

		/// <summary>
		/// Link this world canvas with a particular weapon part.
		/// Displays the name of this part, and allows checking for if the player currently has it.
		/// </summary>
		public void LinkToObject(WeaponPartScript part)
		{
			mCanvasGroup.gameObject.SetActive(true);
			mLinkedPart = part;
			mPartText.text = part.prettyName;
		}

		/// <summary>
		/// Set the maximum alpha this canvas can display (0-1).
		/// </summary>
		public void SetMaxAlpha(float a)
		{
			mMaxAlpha = a;
		}

		/// <summary>
		/// Grab a reference to the local player. Used for rotations and alpha fading.
		/// </summary>
		private IEnumerator GrabPlayerReference()
		{
			while (mPlayerRef == null || mPlayerWeapon == null)
			{
				yield return null;
				CltPlayer script = CltPlayer.localPlayerReference;

				if (script == null)
					continue;

				mPlayerRef = script.eye.transform;
				mPlayerWeapon = (BaseWeaponScript)script.weapon;
			}
		}

		/// <summary>
		/// Unity's Update function.
		/// </summary>
		private void Update()
		{
			if (mPlayerRef == null)
				return;

			DoAlpha();
			Rotate();
			UpdateDoesHave();
		}

		/// <summary>
		/// Rotate the canvas to face our player reference.
		/// </summary>
		private void Rotate()
		{
			Vector3 direction = transform.position - mPlayerRef.position;
			Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
			rot = Quaternion.Euler(0.0f, rot.eulerAngles.y, 0.0f);
			transform.rotation = rot;
		}

		/// <summary>
		/// Lerp the color between transparent and visible based on if the player is looking at us.
		/// </summary>
		private void DoAlpha()
		{
			Vector3 direction = transform.position - mPlayerRef.position;
			float dot = Vector3.Dot(direction.normalized, mPlayerRef.forward);
			dot = (Mathf.Pow(dot, 10.0f) - 0.6f) * 2.5f;

			mCanvasGroup.alpha = dot * mMaxAlpha;
		}

		/// <summary>
		/// Check whether or not the player has the part we are linked to.
		/// </summary>
		private void UpdateDoesHave()
		{
			if (mPlayerWeapon == null || mPlayerWeapon.currentParts == null)
				return;

			WeaponPartScript current = mPlayerWeapon.currentParts[mLinkedPart.attachPoint];
			mAlreadyHasText.gameObject.SetActive(current != null && current.partId == mLinkedPart.partId);
		}
	}
}
