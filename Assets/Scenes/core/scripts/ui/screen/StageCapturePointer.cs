using KeatsLib.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to handle the little "pointer" UI element that directs
	/// players to the stage.
	/// </summary>
	public class StageCapturePointer : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private Sprite mOffscreenSprite;
		[SerializeField] private Sprite mOnscreenSprite;
		[SerializeField] private UIText mLabelText;

		/// Private variables
		private UIImage mImage;
		//private Color mImageFarColor, mImageCloseColor;
		private RectTransform mRealTransform;
		private StageCaptureArea mTarget;

		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			mRealTransform = GetComponent<RectTransform>();
			mImage = GetComponent<UIImage>();
		}

		/// <summary>
		/// Unity's Update function.
		/// </summary>
		private void Update()
		{
			if (mTarget == null)
				return;

			MoveAndFaceTarget();
		}

		/// <summary>
		/// Enable the pointer and direct it towards a particular stage.
		/// </summary>
		public void EnableAndPoint(StageCaptureArea target)
		{
			// take the viewport -0.5, * 2.0f, the magnitude should be 1
			gameObject.SetActive(true);
			mLabelText.gameObject.SetActive(true);
			mTarget = target;
			MoveAndFaceTarget();
		}

		/// <summary>
		/// Disable the pointer immediately.
		/// </summary>
		public void StopPointing()
		{
			gameObject.SetActive(false);
			mLabelText.gameObject.SetActive(false);
			mTarget = null;
		}

		/// <summary>
		/// Moves the pointer and adjusts it to match the current position and direction of the player
		/// relative to the stage.
		/// </summary>
		private void MoveAndFaceTarget()
		{
			Vector3 targetPos = mTarget.transform.position + Vector3.up * 3.0f;
			Vector3 cameraForward = Camera.main.transform.forward;
			Vector2 viewportPos = Camera.main.WorldToViewportPoint(targetPos);

			float dot = Vector3.Dot((targetPos - Camera.main.transform.position).normalized, cameraForward);
			bool offScreen = Mathf.Abs(viewportPos.x - 0.5f) > 0.45f || Mathf.Abs(viewportPos.y - 0.5f) > 0.45f || dot < 0.05f;

			if (offScreen)
				UpdateOffScreen(viewportPos, dot);
			else
				UpdateOnScreen(viewportPos, targetPos);
		}

		/// <summary>
		/// Update the pointer when the target is off screen.
		/// </summary>
		/// <param name="viewportPos">The position of the target in viewport space.</param>
		/// <param name="dot">The dot product of the direction to the target and the camera's forward.</param>
		private void UpdateOffScreen(Vector2 viewportPos, float dot)
		{
			// To get the proper anchor, take the position, adjust it as if the screen center was the center
			// of our coordinate system, flip the value if our dot product is negative (we're facing away),
			//normalize it, then bring it back into screen coordinates
			Vector2 screenCenter = Vector2.one * 0.5f;
			Vector2 adjustedPos = (viewportPos * Mathf.Sign(dot) - screenCenter).normalized;

			Vector2 anchorPos = adjustedPos / 2.0f + screenCenter;
			float angle = Mathf.Atan2(adjustedPos.y, adjustedPos.x) * Mathf.Rad2Deg + 90.0f;
			mRealTransform.anchorMin = new Vector2(anchorPos.x, anchorPos.y);
			mRealTransform.anchorMax = new Vector2(anchorPos.x, anchorPos.y);
			mRealTransform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);

			mImage.sprite = mOffscreenSprite;
			//mImage.color = mImageFarColor;

			mLabelText.enabled = true;
			mLabelText.transform.localRotation = Quaternion.AngleAxis(-angle, Vector3.forward);
		}

		/// <summary>
		/// Update the pointer when the target is on screen.
		/// </summary>
		/// <param name="viewportPos">The position of the target in viewport space.</param>
		/// <param name="worldPos">The position of the target in world space.</param>
		private void UpdateOnScreen(Vector2 viewportPos, Vector3 worldPos)
		{
			viewportPos.x = viewportPos.x.Rescale(0.1f, 0.9f);
			viewportPos.y = viewportPos.y.Rescale(0.1f, 0.9f);
			mRealTransform.anchorMin = new Vector2(viewportPos.x, viewportPos.y);
			mRealTransform.anchorMax = new Vector2(viewportPos.x, viewportPos.y);
			mRealTransform.localRotation = Quaternion.identity;

			mImage.sprite = mOnscreenSprite;
			
			mLabelText.enabled = false;
		}
	}
}
