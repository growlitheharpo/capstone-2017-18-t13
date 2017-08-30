using System.Collections;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

/// <summary>
/// A utility UI class attached to a prefab. This is useful for a labeled score values or health values, for instance.
/// </summary>
public class UINumberTextScript : MonoBehaviour
{
	[SerializeField] private string mLabel;
	[SerializeField] private UIText mLabelField;
	[SerializeField] private float mLerpTime;
	[SerializeField] private UIText mNumberField;
	[SerializeField] private string mNumberFormat;
	[SerializeField] private float mStartValue;

	private float mTargetVal, mCurrentVal;

	private void Start()
	{
		SetLabel(mLabel);
		mNumberField.text = mStartValue.ToString(mNumberFormat);
	}

	/// <summary>
	/// Change the displayed label to a new value.
	/// </summary>
	/// <param name="label">The new label to use.</param>
	public void SetLabel(string label)
	{
		mLabel = label;
		mLabelField.text = label + ":";
	}

	/// <summary>Updates the value on the screen to a new number.</summary>
	/// <param name="val">The new value of the score.</param>
	public void UpdateValue(float val)
	{
		StopAllCoroutines();
		mTargetVal = val;

		StartCoroutine(LerpValue());
	}

	private IEnumerator LerpValue()
	{
		float startVal = mCurrentVal;
		float currentTime = 0.0f;

		while (currentTime < mLerpTime)
		{
			mCurrentVal = Mathf.Lerp(startVal, mTargetVal, currentTime / mLerpTime);
			mNumberField.text = mCurrentVal.ToString(mNumberFormat);

			currentTime += Time.deltaTime;
			yield return null;
		}

		mCurrentVal = mTargetVal;
		mNumberField.text = mCurrentVal.ToString(mNumberFormat);
	}
}
