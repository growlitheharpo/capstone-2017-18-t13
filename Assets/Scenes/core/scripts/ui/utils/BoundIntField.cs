using System.Collections;
using FiringSquad.Core;
using FiringSquad.Core.UI;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	public class BoundIntField : MonoBehaviour
	{
		[SerializeField] private string mBoundProperty;
		[SerializeField] private string mDisplayFormat;

		private IGameplayUIManager mUIManagerRef;
		private BoundProperty<int> mProperty;
		private UIText mTextElement;
		private int mPropertyHash;
		private bool mSearching;

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
			mSearching = true;

			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			while (mProperty == null)
			{
				mProperty = mUIManagerRef.GetProperty<int>(mPropertyHash);
				yield return null;
			}

			AttachProperty();
		}

		private void Update()
		{
			if (mProperty == null && !mSearching)
				StartCoroutine(CheckForProperty());
		}

		private void AttachProperty()
		{
			mProperty.ValueChanged += HandlePropertyChanged;
			mProperty.BeingDestroyed += CleanupProperty;
			HandlePropertyChanged();
			mSearching = false;
		}

		private void CleanupProperty()
		{
			mProperty.ValueChanged -= HandlePropertyChanged;
			mProperty.BeingDestroyed -= CleanupProperty;
			mProperty = null;
		}

		private void HandlePropertyChanged()
		{
			mTextElement.text = mProperty.value.ToString(mDisplayFormat);
		}
	}
}
