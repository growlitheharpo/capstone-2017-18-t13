using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// A UI helper class for providing a float value to a UI manager.
	/// Specialized for a UnityEngine.UI.Slider.
	/// </summary>
	public class SliderFloatProvider : BaseFloatProvider
	{
		/// Inspector variables
		[SerializeField] private Slider mSlider;

		/// <inheritdoc />
		public override float GetValue()
		{
			return mSlider.value;
		}

		/// <inheritdoc />
		public override void SetValue(float val)
		{
			mSlider.value = val;
		}

		/// <summary>
		/// Handle the value changing.
		/// </summary>
		public new void ValueChanged()
		{
			base.ValueChanged();
		}
	}
}
