using System.Collections;
using FiringSquad.Core;
using FiringSquad.Core.UI;
using UnityEngine;
using UIText = UnityEngine.UI.Text;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to bind some form of graphic or display element to a BoundProperty in code.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class BoundUIElement<T> : MonoBehaviour
	{
		/// Inspector variables
		[SerializeField] private string mBoundProperty;

		/// Private variables
		private IGameplayUIManager mUIManagerRef;
		private BoundProperty<T> mProperty;
		private int mPropertyHash;
		private bool mSearching;

		/// <summary>
		/// The property that we are bound to.
		/// </summary>
		protected BoundProperty<T> property { get { return mProperty; } }

		/// <summary>
		/// Unity's Awake function.
		/// Sets things up to bind our property assigned in-editor.
		/// </summary>
		protected virtual void Awake()
		{
			mPropertyHash = mBoundProperty.GetHashCode();
			mProperty = null;
		}

		/// <summary>
		/// Unity's Start function.
		/// Begins loop to bind our property.
		/// </summary>
		protected virtual void Start()
		{
			mUIManagerRef = ServiceLocator.Get<IGameplayUIManager>();
			StartCoroutine(CheckForProperty());
		}

		/// <summary>
		/// Attempt to grab our property from the service so that we can bind it.
		/// </summary>
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

		/// <summary>
		/// Unity's Update function.
		/// Starts a re-bind loop if the property is null.
		/// </summary>
		protected virtual void Update()
		{
			if (mProperty == null && !mSearching)
				StartCoroutine(CheckForProperty());
		}

		/// <summary>
		/// Attach and bind a property to our handler for it.
		/// </summary>
		private void AttachProperty()
		{
			mProperty.ValueChanged += HandlePropertyChanged;
			mProperty.BeingDestroyed += CleanupProperty;
			HandlePropertyChanged();
			mSearching = false;
		}

		/// <summary>
		/// Handle a property being destroyed and start to look for a new one.
		/// </summary>
		private void CleanupProperty()
		{
			mProperty.ValueChanged -= HandlePropertyChanged;
			mProperty.BeingDestroyed -= CleanupProperty;
			mProperty = null;
		}

		/// <summary>
		/// Override this in base classes to express how to utilize the bound property.
		/// </summary>
		protected abstract void HandlePropertyChanged();
	}
}
