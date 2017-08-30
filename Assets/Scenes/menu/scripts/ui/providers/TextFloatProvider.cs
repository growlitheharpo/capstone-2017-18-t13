using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class TextFloatProvider : BaseFloatProvider
{
	[SerializeField] private Text mInputfield;
	private float mPreviousValidValue;

	public override float GetValue()
	{
		float result;
		if (!float.TryParse(mInputfield.text, NumberStyles.Any, new NumberFormatInfo(), out result))
			return mPreviousValidValue;

		mPreviousValidValue = result;
		return result;
	}

	public override void SetValue(float val)
	{
		mPreviousValidValue = val;
		mInputfield.text = val.ToString(CultureInfo.InvariantCulture);
	}
}
