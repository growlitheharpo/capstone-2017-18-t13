using KeatsLib.Unity;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	public class BoundFloatSliderField : BoundUIElement<float>
	{
		[SerializeField] private float mMinValue;
		[SerializeField] private float mMaxValue;
		private UIFillBarScript mBar;

		protected override void Awake()
		{
			base.Awake();
			mBar = GetComponentInChildren<UIFillBarScript>();
		}

		protected override void Start()
		{
			mBar.SetFillAmount(0.0f);
		}
		
		protected override void HandlePropertyChanged()
		{
			float rawVal = property.value;
			float fill = (rawVal - mMinValue) / mMaxValue;
			mBar.SetFillAmount(fill);
		}
	}
}
