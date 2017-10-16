using System;

/// <summary>
/// A class to bind data between classes, so that the listener (such as a UI element)
/// can get notifications when the value changes.
/// </summary>
public class BoundProperty
{
	protected object mValue;
	public event Action ValueChanged = () => { };
	public event Action BeingDestroyed = () => { };

	~BoundProperty()
	{
		Cleanup();
	}

	protected void OnValueChanged()
	{
		ValueChanged();
	}

	protected void OnDestroy()
	{
		BeingDestroyed();
	}

	/// <summary>
	/// This is just to be nice. A listener will not keep a publisher alive.
	/// </summary>
	/// <see cref="https://stackoverflow.com/a/298276"/>
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
			bool changed = !Equals(this.mValue, value);
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

	public BoundProperty()
	{
		value = default(T);
	}

	public BoundProperty(T value)
	{
		this.value = value;
	}

	public BoundProperty(T value, int property)
	{
		this.value = value;
		ServiceLocator.Get<IGameplayUIManager>()
			.BindProperty(property, this);
	}
}
