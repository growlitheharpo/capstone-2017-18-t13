using System.Collections;
using FiringSquad.Core;
using FiringSquad.Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// UI class to bind the fill value of a UI image to a BoundProperty in code.
	/// </summary>
	public abstract class BaseBoundFillPercentField<T> : MonoBehaviour
	{
		[SerializeField] private string mPercentProperty;
		[SerializeField] private string mFullProperty;
		[SerializeField] private T mFullValue;

		private IUIManager mUIManagerRef;
		private int mCurrentHash, mFullHash;
		private bool mSearching;
		private Image mImage;

		/// <summary>
		/// The property we use to determine our current value.
		/// </summary>
		private BoundProperty<T> mCurrentValue;
		
		/// <summary>
		/// The property we use to determine the "full" value, or null if hard-coded.
		/// </summary>
		private BoundProperty<T> mFullValueProperty;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		protected virtual void Awake()
		{
			mCurrentHash = mPercentProperty.GetHashCode();
			if (!string.IsNullOrEmpty(mFullProperty))
				mFullHash = mFullProperty.GetHashCode();

			mImage = GetComponent<Image>();
		}

		/// <summary>
		/// Unity's Start function.
		/// Begins our loop to bind our properties.
		/// </summary>
		private void Start()
		{
			mUIManagerRef = ServiceLocator.Get<IUIManager>();
			StartCoroutine(CheckForProperties());
		}

		/// <summary>
		/// Unity's Update function
		/// </summary>
		private void Update()
		{
			bool haveCurrent = mCurrentValue != null;
			bool haveFull = string.IsNullOrEmpty(mFullProperty) || mFullValueProperty != null;
			if ((!haveCurrent || !haveFull) && !mSearching)
				StartCoroutine(CheckForProperties());
		}
		
		/// <summary>
		/// Attempt to grab our properties from the service so that we can bind them.
		/// </summary>
		private IEnumerator CheckForProperties()
		{
			mSearching = true;

			yield return null;
			yield return null;
			while (mCurrentValue == null || mFullValueProperty == null && !string.IsNullOrEmpty(mFullProperty))
			{
				mCurrentValue = mUIManagerRef.GetProperty<T>(mCurrentHash);
				if (!string.IsNullOrEmpty(mFullProperty))
					mFullValueProperty = mUIManagerRef.GetProperty<T>(mFullHash);

				yield return null;
			}

			AttachProperties();
		}

		/// <summary>
		/// Attach and bind our properties.
		/// </summary>
		private void AttachProperties()
		{
			mCurrentValue.ValueChanged += CurrentValueChanged;
			mCurrentValue.BeingDestroyed += CleanupCurrentProperty;

			if (mFullValueProperty != null)
			{
				mFullValueProperty.ValueChanged += FullValueChanged;
				mFullValueProperty.BeingDestroyed += CleanupFullProperty;
			}
		}

		/// <summary>
		/// Handle the current value changing.
		/// </summary>
		private void CurrentValueChanged()
		{
			T full = mFullValueProperty == null ? mFullValue : mFullValueProperty.value;
			T current = mCurrentValue.value;
			RecalculateFill(mImage, full, current);
		}

		/// <summary>
		/// Handle the "full" value changing.
		/// </summary>
		private void FullValueChanged()
		{
			T full = mFullValueProperty == null ? mFullValue : mFullValueProperty.value;
			T current = mCurrentValue.value;
			RecalculateFill(mImage, full, current);
		}

		/// <summary>
		/// Recalculate the image fill based on the current values of the properties.
		/// </summary>
		/// <param name="image">The image to adjust the fill of.</param>
		/// <param name="full">The current full value.</param>
		/// <param name="current">The current value.</param>
		protected abstract void RecalculateFill(Image image, T full, T current);

		/// <summary>
		/// Cleanup references to the current value property.
		/// </summary>
		private void CleanupCurrentProperty()
		{
			mCurrentValue.ValueChanged -= CurrentValueChanged;
			mCurrentValue.BeingDestroyed -= CleanupCurrentProperty;
			mCurrentValue = null;
		}

		/// <summary>
		/// Cleanup references to the full value property.
		/// </summary>
		private void CleanupFullProperty()
		{
			mFullValueProperty.ValueChanged -= FullValueChanged;
			mFullValueProperty.BeingDestroyed -= CleanupFullProperty;
			mFullValueProperty = null;
		}
	}
}
