using UnityEngine;
using UnityEngine.UI;

public class SliderFloatProvider : BaseFloatProvider
{
	[SerializeField] private Slider mSlider;

	public override float GetValue()
	{
		return mSlider.value;
	}

	public override void SetValue(float val)
	{
		mSlider.value = val;
	}
}
