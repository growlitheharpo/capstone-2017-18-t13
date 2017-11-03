using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	public class StageCapturePointer : MonoBehaviour
	{
		[SerializeField] private Sprite mOffscreenSprite;
		[SerializeField] private Sprite mOnscreenSprite;

		private UIImage mImage;
		private Color mImageFarColor, mImageCloseColor;
		private RectTransform mRealTransform;
		private StageCaptureArea mTarget;

		private void Awake()
		{
			mRealTransform = GetComponent<RectTransform>();
			mImage = GetComponent<UIImage>();
			mImageFarColor = mImage.color;
			mImageCloseColor = new Color(mImageFarColor.r, mImageFarColor.g, mImageFarColor.b, 0.0f);
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
				// To get the proper anchor, take the position, adjust it as if the screen center was the center
				// of our coordinate system, flip the value if our dot product is negative (we're facing away),
				//normalize it, then bring it back into screen coordinates
				Vector2 screenCenter = Vector2.one * 0.5f;
				Vector2 adjustedPos = (viewportPos * Mathf.Sign(dot) - screenCenter).normalized;

				Vector2 anchorPos = adjustedPos / 2.0f + screenCenter;
				mRealTransform.anchorMin = new Vector2(anchorPos.x, anchorPos.y);
				mRealTransform.anchorMax = new Vector2(anchorPos.x, anchorPos.y);
				mRealTransform.localRotation = Quaternion.AngleAxis(Mathf.Atan2(adjustedPos.y, adjustedPos.x) * Mathf.Rad2Deg + 90.0f, Vector3.forward);

				mImage.sprite = mOffscreenSprite;
				mImage.color = mImageFarColor;
			}
			else
			{
				mRealTransform.anchorMin = new Vector2(viewportPos.x, viewportPos.y);
				mRealTransform.anchorMax = new Vector2(viewportPos.x, viewportPos.y);
				mRealTransform.localRotation = Quaternion.identity;

				mImage.sprite = mOnscreenSprite;
				mImage.color = Color.Lerp(mImageCloseColor, mImageFarColor, Vector3.Distance(targetPos, Camera.main.transform.position) / 3.0f - 5.0f);
			}
		}
	}
}
