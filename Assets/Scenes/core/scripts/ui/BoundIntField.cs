using System.Collections;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

public class BoundIntField : MonoBehaviour
{
	[SerializeField] private string mBoundProperty;
	[SerializeField] private string mDisplayFormat;
	
	private IGameplayUIManager mUIManagerRef;
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
		mUIManagerRef = ServiceLocator.Get<IGameplayUIManager>();
		StartCoroutine(CheckForProperty());
	}

	private IEnumerator CheckForProperty()
	{
		while (mProperty == null)
		{
			mProperty = mUIManagerRef.GetProperty<int>(mPropertyHash);
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
