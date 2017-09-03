using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A UI helper class for providing a float value to a UI manager.
/// Specialized for a UnityEngine.UI.Text.
/// </summary>
public class TextFloatProvider : BaseFloatProvider
{
	[SerializeField] private Text mInputfield;
	private float mPreviousValidValue;

	/// <inheritdoc />
	public override float GetValue()
	{
		float result;
		if (!float.TryParse(mInputfield.text, NumberStyles.Any, new NumberFormatInfo(), out result))
			return mPreviousValidValue;

		mPreviousValidValue = result;
		return result;
	}

	/// <inheritdoc />
	public override void SetValue(float val)
	{
		mPreviousValidValue = val;
		mInputfield.text = val.ToString(CultureInfo.InvariantCulture);
	}
}
