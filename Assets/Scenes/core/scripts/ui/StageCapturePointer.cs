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
			Vector3 cameraForward = Camera.main.transform.forward;
			Vector2 viewportPos = Camera.main.WorldToViewportPoint(targetPos);

			float dot = Vector3.Dot((targetPos - Camera.main.transform.position).normalized, cameraForward);

			if (Mathf.Abs(viewportPos.x - 0.5f) > 0.45f || Mathf.Abs(viewportPos.y - 0.5f) > 0.45f || dot < 0.05f) // off screen
			{
				Vector2 center = Vector2.one * 0.5f;
				Vector2 adjustedPos = (viewportPos - center).normalized / 2.0f + center;

				mRealTransform.anchorMin = new Vector2(adjustedPos.x, adjustedPos.y);
				mRealTransform.anchorMax = new Vector2(adjustedPos.x, adjustedPos.y);
			}
			else
			{
				mRealTransform.anchorMin = new Vector2(viewportPos.x, viewportPos.y);
				mRealTransform.anchorMax = new Vector2(viewportPos.x, viewportPos.y);
				mRealTransform.localRotation = Quaternion.identity;
			}
		}
	}
}
