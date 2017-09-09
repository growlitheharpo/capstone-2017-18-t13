using System.Collections;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

public class BoundIntField : MonoBehaviour
{
	[SerializeField] private string mBoundProperty;
	[SerializeField] private string mDisplayFormat;
	[SerializeField] private Prototype2.UIManager mUIManager;

	private BoundProperty<int> mProperty;
	private UIText mTextElement;
	private int mPropertyHash;

	private void Awake()
	{
		mTextElement = GetComponent<UIText>();
		mPropertyHash = mBoundProperty.GetHashCode();
		mProperty = null;
	}

	private void Start()
	{
		StartCoroutine(CheckForProperty());
	}

	private IEnumerator CheckForProperty()
	{
		while (mProperty == null)
		{
			Debug.Log("Checking for property...");
			BoundProperty testOut;
			if (mUIManager.propertyMap.TryGetValue(mPropertyHash, out testOut))
				mProperty = testOut as BoundProperty<int>;

			yield return null;
		}

		AttachProperty();
	}

	private void AttachProperty()
	{
		mProperty.ValueChanged += HandlePropertyChanged;
		HandlePropertyChanged();
	}

	private void HandlePropertyChanged()
	{
		mTextElement.text = mProperty.value.ToString(mDisplayFormat);
	}
}
