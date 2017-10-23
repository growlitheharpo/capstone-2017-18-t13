using System.Collections;
using System.Linq;
using KeatsLib.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	public class UICircleTimer : MonoBehaviour
	{
		[SerializeField] private bool mPointAtCamera;

		private static Transform kPlayerRef;
		private UIImage mImage;

		private float mStart, mEnd;

		private void Start()
		{
			StartCoroutine(GrabPlayerReference());
			mImage = GetComponent<UIImage>();

			mImage.enabled = false;
		}

		private IEnumerator GrabPlayerReference()
		{
			while (kPlayerRef == null)
			{
				yield return null;
				CltPlayer script = FindObjectsOfType<CltPlayer>().FirstOrDefault(x => x.isCurrentPlayer);
				if (script != null)
					kPlayerRef = script.eye.transform;
			}
		}

		private void Update()
		{
			mImage.fillAmount = CalculateFill();

			if (mPointAtCamera && kPlayerRef != null)
				Rotate();
		}

		public void SetTimes(float start, float end, bool activate = true)
		{
			mStart = start;
			mEnd = end;
			mImage.fillAmount = CalculateFill();

			mImage.enabled = activate;
		}

		private float CalculateFill()
		{
			float currentTime = Time.time;
			return currentTime.Rescale(mStart, mEnd, 1.0f, 0.0f);
		}

		private void Rotate()
		{
			Vector3 direction = transform.position - kPlayerRef.position;
			Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
			rot = Quaternion.Euler(0.0f, rot.eulerAngles.y, 0.0f);
			transform.rotation = rot;
		}
	}
}
