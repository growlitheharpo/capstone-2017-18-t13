using FiringSquad.Gameplay;
using UnityEngine;

public static class StaticGameExtensions
{
	public static IDamageReceiver GetDamageReceiver(this Collision col)
	{
		return GetComponentUpwards<IDamageReceiver>(col.transform);
	}

	public static IDamageReceiver GetDamageReceiver(this RaycastHit col)
	{
		return GetComponentUpwards<IDamageReceiver>(col.transform);
	}

	public static IInteractable GetInteractableComponent(this Collision col)
	{
		return GetComponentUpwards<IInteractable>(col.transform);
	}

	public static IInteractable GetInteractableComponent(this RaycastHit col)
	{
		return GetComponentUpwards<IInteractable>(col.transform);
	}

	public static T GetComponentUpwards<T>(this GameObject g) where T : class
	{
		return GetComponentUpwards<T>(g.transform);
	}

	public static T GetComponentUpwards<T>(this Component g) where T : class
	{
		return GetComponentUpwards<T>(g.transform);
	}

	public static T GetComponentUpwards<T>(Transform t) where T : class
	{
		T c = null;
		while (c == null && t != null)
		{
			c = t.GetComponent<T>();
			t = t.parent;
		}

		return c;
	}
}
