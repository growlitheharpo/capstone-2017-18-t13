using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <inheritdoc />
	public class BoundFloatSliderField : BoundUIElement<float>
	{
		/// Inspector variables
		[SerializeField] private float mMinValue;
		[SerializeField] private float mMaxValue;

		/// Private variables
		private UIFillBarScript mBar;

		/// <inheritdoc />
		protected override void Awake()
		{
			base.Awake();
			mBar = GetComponentInChildren<UIFillBarScript>();
		}

		/// <inheritdoc />
		protected override void Start()
		{
			mBar.SetFillAmount(0.0f);
		}

		/// <inheritdoc />
		protected override void HandlePropertyChanged()
		{
			float rawVal = property.value;
			float fill = (rawVal - mMinValue) / mMaxValue;
			mBar.SetFillAmount(fill);
		}
	}
}
