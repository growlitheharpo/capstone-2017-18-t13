using Prototype2;
using UnityEngine;

public static class StaticGameExtensions
{
	public static IDamageReceiver GetDamageReceiver(this Collision col)
	{
		IDamageReceiver c = null;
		Transform t = col.transform;

		while (c == null && t != null)
		{
			c = t.GetComponent<IDamageReceiver>();
			t = t.parent;
		}

		return c;
	}

	public static IDamageReceiver GetDamageReceiver(this RaycastHit col)
	{
		IDamageReceiver c = null;
		Transform t = col.transform;

		while (c == null && t != null)
		{
			c = t.GetComponent<IDamageReceiver>();
			t = t.parent;
		}

		return c;
	}
}
