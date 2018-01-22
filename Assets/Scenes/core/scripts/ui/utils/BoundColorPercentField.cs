using KeatsLib.Collections;
using UnityEngine;
using UIImage = UnityEngine.UI.Image;

namespace FiringSquad.Gameplay.UI
{
	/// <inheritdoc />
	public class BoundColorPercentField : BoundUIElement<float>
	{
		/// Inspector variables
		[SerializeField] private float mEmptyValue;
		[SerializeField] private float mFullValue = 1.0f;
		[SerializeField] private Color mFullColor;
		[SerializeField] private Color mEmptyColor;

		/// Private variables
		private UIImage mImage;

		/// <inheritdoc />
		protected override void Awake()
		{
			base.Awake();
			mImage = GetComponent<UIImage>();
		}

		/// <inheritdoc />
		protected override void HandlePropertyChanged()
		{
			float val = property.value.Rescale(mEmptyValue, mFullValue);
			mImage.color = Color.Lerp(mEmptyColor, mFullColor, val);
		}
	}
}

