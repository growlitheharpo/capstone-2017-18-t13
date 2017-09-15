using System;

public class BoundProperty
{
	protected object mValue;
	public event Action ValueChanged = () => { };

	protected void OnValueChanged()
	{
		ValueChanged();
	}

	public void Cleanup()
	{
		var delegates = ValueChanged.GetInvocationList();
		foreach (Delegate d in delegates)
			ValueChanged -= (Action)d;
	}
}

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
		EventManager.Notify(() => EventManager.BoundPropertyCreated(this, property));
	}
}
