using UnityEngine;

/// <summary>
/// Static class with Animation-related utility functions to avoid cluttering code.
/// </summary>
public static class AnimationUtility
{
	public static void PlayAnimationInChildren(GameObject obj, string trigger)
	{
		PlayAnimation(obj.GetComponentInChildren<Animator>(), trigger);
	}

	public static void PlayAnimation(GameObject obj, string trigger)
	{
		PlayAnimation(obj.GetComponent<Animator>(), trigger);
	}

	public static void PlayAnimation(Transform obj, string trigger)
	{
		PlayAnimation(obj.GetComponent<Animator>(), trigger);
	}

	public static void PlayAnimation(Animator obj, string trigger)
	{
		if (obj != null)
			obj.SetTrigger(trigger);
	}

	public static void SetVariableInChildren(GameObject obj, string name, float val)
	{
		SetVariable(obj.GetComponentInChildren<Animator>(), name, val);
	}

	public static void SetVariable(GameObject obj, string name, float val)
	{
		SetVariable(obj.GetComponent<Animator>(), name, val);
	}

	public static void SetVariable(Transform obj, string name, float val)
	{
		SetVariable(obj.GetComponent<Animator>(), name, val);
	}

	public static void SetVariable(Animator obj, string name, float val)
	{
		if (obj != null)
			obj.SetFloat(name, val);
	}

	public static void SetVariableInChildren(GameObject obj, string name, int val)
	{
		SetVariable(obj.GetComponentInChildren<Animator>(), name, val);
	}

	public static void SetVariable(GameObject obj, string name, int val)
	{
		SetVariable(obj.GetComponent<Animator>(), name, val);
	}

	public static void SetVariable(Transform obj, string name, int val)
	{
		SetVariable(obj.GetComponent<Animator>(), name, val);
	}

	public static void SetVariable(Animator obj, string name, int val)
	{
		if (obj != null)
			obj.SetInteger(name, val);
	}

	public static void SetVariableInChildren(GameObject obj, string name, bool val)
	{
		SetVariable(obj.GetComponentInChildren<Animator>(), name, val);
	}

	public static void SetVariable(GameObject obj, string name, bool val)
	{
		SetVariable(obj.GetComponent<Animator>(), name, val);
	}

	public static void SetVariable(Transform obj, string name, bool val)
	{
		SetVariable(obj.GetComponent<Animator>(), name, val);
	}

	public static void SetVariable(Animator obj, string name, bool val)
	{
		if (obj != null)
			obj.SetBool(name, val);
	}

	public static float GetFloat(GameObject obj, string name)
	{
		return GetFloat(obj.GetComponent<Animator>(), name);
	}

	public static float GetFloat(Transform obj, string name)
	{
		return GetFloat(obj.GetComponent<Animator>(), name);
	}

	public static float GetFloat(Animator obj, string name)
	{
		return obj.GetFloat(name);
	}

	public static bool GetBool(GameObject obj, string name)
	{
		return GetBool(obj.GetComponent<Animator>(), name);
	}

	public static bool GetBool(Transform obj, string name)
	{
		return GetBool(obj.GetComponent<Animator>(), name);
	}

	public static bool GetBool(Animator obj, string name)
	{
		return obj.GetBool(name);
	}

	public static int GetInt(GameObject obj, string name)
	{
		return GetInt(obj.GetComponent<Animator>(), name);
	}

	public static int GetInt(Transform obj, string name)
	{
		return GetInt(obj.GetComponent<Animator>(), name);
	}

	public static int GetInt(Animator obj, string name)
	{
		return obj.GetInteger(name);
	}
}
