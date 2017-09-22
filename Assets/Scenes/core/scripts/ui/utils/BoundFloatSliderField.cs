using System.Collections;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

public class BoundFloatSliderField : MonoBehaviour
{
	[SerializeField] private string mBoundProperty;
	[SerializeField] private float mMinValue;
	[SerializeField] private float mMaxValue;

	private IGameplayUIManager mUIManagerRef;
	private BoundProperty<float> mProperty;
	private UIFillBarScript mBar;
	private int mPropertyHash;

	private void Awake()
	{
		mBar = GetComponentInChildren<UIFillBarScript>();
		mPropertyHash = mBoundProperty.GetHashCode();
		mProperty = null;
	}

	private void Start()
	{
		mUIManagerRef = ServiceLocator.Get<IGameplayUIManager>();
		StartCoroutine(CheckForProperty());

		mBar.SetFillAmount(0.0f);
	}

	private IEnumerator CheckForProperty()
	{
		yield return null;

		while (mProperty == null)
		{
			mProperty = mUIManagerRef.GetProperty<float>(mPropertyHash);
			yield return null;
		}

		AttachProperty();
	}

	private void AttachProperty()
	{
		mProperty.ValueChanged += HandlePropertyChanged;
		mProperty.BeingDestroyed += CleanupProperty;
		HandlePropertyChanged();
	}

	private void CleanupProperty()
	{
		mProperty.ValueChanged -= HandlePropertyChanged;
		mProperty.BeingDestroyed -= CleanupProperty;
		StartCoroutine(CheckForProperty());
	}

	private void HandlePropertyChanged()
	{
		float rawVal = mProperty.value;
		float fill = (rawVal - mMinValue) / mMaxValue;
		mBar.SetFillAmount(fill);
	}
}
