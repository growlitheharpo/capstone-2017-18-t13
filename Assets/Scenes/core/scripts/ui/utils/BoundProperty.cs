using System;
using FiringSquad.Core.UI;

namespace FiringSquad.Core
{
	/// <summary>
	/// A class to bind data between classes, so that the listener (such as a UI element)
	/// can get notifications when the value changes.
	/// </summary>
	public class BoundProperty
	{
		protected object mValue;

		/// <summary>
		/// Event invoked when the value of this property is updated.
		/// </summary>
		public event Action ValueChanged = () => { };
		
		/// <summary>
		/// Event invoked when this property is being destroyed.
		/// Allows listeners to clean up.
		/// </summary>
		public event Action BeingDestroyed = () => { };

		~BoundProperty()
		{
			Cleanup();
		}

		/// <summary>
		/// Notify that our value changed.
		/// </summary>
		protected void OnValueChanged()
		{
			ValueChanged();
		}

		/// <summary>
		/// Notify that we are being destroyed.
		/// </summary>
		protected void OnDestroy()
		{
			BeingDestroyed();
		}

		/// <summary>
		/// This is just to be nice. A listener will not keep a publisher alive.
		/// </summary>
		/// <see>
		/// <cref>https://stackoverflow.com/a/298276</cref>
		/// </see>
		public void Cleanup()
		{
			OnDestroy();

			var delegates = ValueChanged.GetInvocationList();
			foreach (Delegate d in delegates)
				ValueChanged -= (Action)d;

			ServiceLocator.Get<IGameplayUIManager>()
				.UnbindProperty(this);
		}
	}

	/// <inheritdoc />
	/// <typeparam name="T">The type of object to store.</typeparam>
	public class BoundProperty<T> : BoundProperty
	{
		public T value
		{
			get { return (T)mValue; }
			set
			{
				bool changed = !Equals(mValue, value);
				if (!changed)
					return;

				mValue = value;
				OnValueChanged();
			}
		}

		~BoundProperty()
		{
			Cleanup();
		}

		/// <summary>
		/// A class to bind data between classes, so that the listener (such as a UI element)
		/// can get notifications when the value changes.
		/// </summary>
		public BoundProperty()
		{
			value = default(T);
		}

		/// <summary>
		/// A class to bind data between classes, so that the listener (such as a UI element)
		/// can get notifications when the value changes.
		/// </summary>
		/// <param name="value">The value to initialize with.</param>
		public BoundProperty(T value)
		{
			this.value = value;
		}

		/// <summary>
		/// A class to bind data between classes, so that the listener (such as a UI element)
		/// can get notifications when the value changes.
		/// </summary>
		/// <param name="value">The value to initialize with.</param>
		/// <param name="property">The property hash code to bind to.</param>
		public BoundProperty(T value, int property)
		{
			this.value = value;
			ServiceLocator.Get<IGameplayUIManager>()
				.BindProperty(property, this);
		}
	}
}
