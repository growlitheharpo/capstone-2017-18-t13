using KeatsLib.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	public class BoundColorPercentField : BoundUIElement<float>
	{
		[SerializeField] private float mEmptyValue = 0.0f;
		[SerializeField] private float mFullValue = 1.0f;
		[SerializeField] private Color mFullColor;
		[SerializeField] private Color mEmptyColor;
		private UIImage mImage;

		protected override void Awake()
		{
			base.Awake();
			mImage = GetComponent<UIImage>();
		}

		protected override void HandlePropertyChanged()
		{
			float val = property.value.Rescale(mEmptyValue, mFullValue);
			mImage.color = Color.Lerp(mEmptyColor, mFullColor, val);
		}
	}
}

