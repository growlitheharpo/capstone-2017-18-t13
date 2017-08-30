using UnityEngine;

public abstract class BaseFloatProvider : MonoBehaviour
{
	public abstract float GetValue();
	public abstract void SetValue(float val);
}
