using System.Collections;
using System.Linq;
using FiringSquad.Gameplay.Weapons;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	public class WeaponPartWorldCanvas : MonoBehaviour
	{
		[SerializeField] private UIText mPartText;
		[SerializeField] private UIText mAlreadyHasText;
		[SerializeField] private CanvasGroup mCanvasGroup;

		private Transform mPlayerRef;
		private BaseWeaponScript mPlayerWeapon;
		private WeaponPartScript mLinkedPart;
		private float mMaxAlpha;

		private void Awake()
		{
			StartCoroutine(GrabPlayerReference());
			mAlreadyHasText.gameObject.SetActive(false);
			mCanvasGroup.gameObject.SetActive(false);
			mMaxAlpha = 1.0f;
		}

		public void LinkToObject(WeaponPartScript part)
		{
			mCanvasGroup.gameObject.SetActive(true);
			mLinkedPart = part;
			mPartText.text = part.prettyName;
		}

		public void SetMaxAlpha(float a)
		{
			mMaxAlpha = a;
		}

		private IEnumerator GrabPlayerReference()
		{
			while (mPlayerRef == null || mPlayerWeapon == null)
			{
				yield return null;
				CltPlayer script = FindObjectsOfType<CltPlayer>().FirstOrDefault(x => x.isCurrentPlayer);

				if (script == null)
					continue;

				mPlayerRef = script.eye.transform;
				mPlayerWeapon = (BaseWeaponScript)script.weapon;
			}
		}

		// Update is called once per frame
		private void Update()
		{
			if (mPlayerRef == null)
				return;

			DoAlpha();
			Rotate();
			UpdateDoesHave();
		}

		private void Rotate()
		{
			Vector3 direction = transform.position - mPlayerRef.position;
			Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
			rot = Quaternion.Euler(0.0f, rot.eulerAngles.y, 0.0f);
			transform.rotation = rot;
		}

		private void DoAlpha()
		{
			Vector3 direction = transform.position - mPlayerRef.position;
			float dot = Vector3.Dot(direction.normalized, mPlayerRef.forward);
			dot = (Mathf.Pow(dot, 10.0f) - 0.6f) * 2.5f;

			mCanvasGroup.alpha = dot * mMaxAlpha;
		}

		private void UpdateDoesHave()
		{
			if (mPlayerWeapon == null || mPlayerWeapon.currentParts == null)
				return;

			WeaponPartScript current = mPlayerWeapon.currentParts[mLinkedPart.attachPoint];
			mAlreadyHasText.gameObject.SetActive(current != null && current.partId == mLinkedPart.partId);
		}
	}
}
