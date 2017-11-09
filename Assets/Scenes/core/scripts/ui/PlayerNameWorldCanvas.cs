using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	public class PlayerNameWorldCanvas : MonoBehaviour
	{
		[SerializeField] private Text mDisplayText;
		[SerializeField] private CanvasGroup mCanvasGroup;

		private Transform mLocalPlayerRef;
		private float mMaxAlpha;

		private void Awake()
		{
			StartCoroutine(GrabPlayerReference());
		}

		private IEnumerator GrabPlayerReference()
		{
			while (mLocalPlayerRef == null)
			{
				yield return null;
				CltPlayer script = FindObjectsOfType<CltPlayer>().FirstOrDefault(x => x.isCurrentPlayer);

				if (script == null)
					continue;

				mLocalPlayerRef = script.eye.transform;
			}
		}

		private void Update()
		{
			if (mLocalPlayerRef == null)
				return;

			DoAlpha();
			DoRotate();
		}

		private void DoAlpha()
		{
			Vector3 direction = transform.position - mLocalPlayerRef.position;
			float dot = Vector3.Dot(direction.normalized, mLocalPlayerRef.forward);
			dot = (Mathf.Pow(dot, 10.0f) - 0.6f) * 2.5f;

			mCanvasGroup.alpha = dot * mMaxAlpha;
		}

		private void DoRotate()
		{
			Vector3 direction = transform.position - mLocalPlayerRef.position;
			Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
			rot = Quaternion.Euler(0.0f, rot.eulerAngles.y, 0.0f);
			transform.rotation = rot;
		}

		public void SetPlayerName(string newName)
		{
			mDisplayText.text = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(newName);
		}
	}
}
