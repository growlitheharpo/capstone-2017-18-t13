using UnityEngine;

/// <summary>
/// A UI helper class for providing a float value to a UI manager.
/// </summary>
public abstract class BaseFloatProvider : MonoBehaviour
{
	/// <summary>
	/// Get the value currently represented in the UI.
	/// </summary>
	public abstract float GetValue();

	/// <summary>
	/// Set what the value represented in the UI should be.
	/// </summary>
	public abstract void SetValue(float val);
}
