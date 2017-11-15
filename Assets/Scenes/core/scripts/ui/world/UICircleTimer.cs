using System.Collections;
using System.Linq;
using KeatsLib.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class for filling a UI circle based on a timer.
	/// </summary>
	public class UICircleTimer : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private bool mPointAtCamera;

		/// Private variables
		private static Transform kPlayerRef;
		private UIImage mImage;
		private float mStart, mEnd;

		/// <summary>
		/// Unity's Start function.
		/// </summary>
		private void Start()
		{
			StartCoroutine(GrabPlayerReference());
			mImage = GetComponent<UIImage>();

			mImage.enabled = false;
		}

		/// <summary>
		/// Grab a reference to the local player. Used for rotations.
		/// </summary>
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

		/// <summary>
		/// Unity's Update function.
		/// </summary>
		private void Update()
		{
			mImage.fillAmount = CalculateFill();

			if (mPointAtCamera && kPlayerRef != null)
				Rotate();
		}

		/// <summary>
		/// Set the times that this timer revolves around. The fill is determined based on the difference between the two.
		/// </summary>
		/// <param name="start">The start time.</param>
		/// <param name="end">The end time.</param>
		/// <param name="activate">Whether to immediately turn on the image effect.</param>
		public void SetTimes(float start, float end, bool activate = true)
		{
			mStart = start;
			mEnd = end;
			mImage.fillAmount = CalculateFill();

			mImage.enabled = activate;
		}

		/// <summary>
		/// Determine the fill amount based on the current time.
		/// </summary>
		private float CalculateFill()
		{
			float currentTime = Time.time;
			return currentTime.Rescale(mStart, mEnd, 1.0f, 0.0f);
		}

		/// <summary>
		/// Rotate the canvas to face our player reference.
		/// </summary>
		private void Rotate()
		{
			Vector3 direction = transform.position - kPlayerRef.position;
			Quaternion rot = Quaternion.LookRotation(direction, Vector3.up);
			rot = Quaternion.Euler(0.0f, rot.eulerAngles.y, 0.0f);
			transform.rotation = rot;
		}
	}
}
