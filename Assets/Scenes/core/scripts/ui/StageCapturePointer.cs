using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	public class StageCapturePointer : MonoBehaviour
	{
		private RectTransform mRealTransform;
		private StageCaptureArea mTarget;

		private void Awake()
		{
			mRealTransform = GetComponent<RectTransform>();
		}

		public void EnableAndPoint(StageCaptureArea target)
		{
			// take the viewport -0.5, * 2.0f, the magnitude should be 1
			gameObject.SetActive(true);
			mTarget = target;
			MoveAndFaceTarget();
		}

		private void Update()
		{
			if (mTarget == null)
				return;

			MoveAndFaceTarget();
		}

		public void StopPointing()
		{
			gameObject.SetActive(false);
			mTarget = null;
		}

		private void MoveAndFaceTarget()
		{
			Vector3 targetPos = mTarget.transform.position;
			Vector3 viewportPos = Camera.main.WorldToViewportPoint(targetPos);
		}
	}
}
