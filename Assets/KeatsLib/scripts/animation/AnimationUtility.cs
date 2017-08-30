using UnityEngine;

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
}
