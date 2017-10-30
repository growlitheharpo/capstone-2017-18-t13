using System.Collections;
using FiringSquad.Core;
using FiringSquad.Core.UI;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	public abstract class BoundUIElement<T> : MonoBehaviour
	{
		[SerializeField] private string mBoundProperty;

		private IGameplayUIManager mUIManagerRef;
		private BoundProperty<T> mProperty;
		protected BoundProperty<T> property { get { return mProperty; } }

		private int mPropertyHash;
		private bool mSearching;

		protected virtual void Awake()
		{
			mPropertyHash = mBoundProperty.GetHashCode();
			mProperty = null;
		}

		protected virtual void Start()
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
				mProperty = mUIManagerRef.GetProperty<T>(mPropertyHash);
				yield return null;
			}

			AttachProperty();
		}

		protected virtual void Update()
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

		protected abstract void HandlePropertyChanged();
	}
}
