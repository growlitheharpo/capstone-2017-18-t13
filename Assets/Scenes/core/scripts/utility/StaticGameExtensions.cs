using FiringSquad.Gameplay;
using UnityEngine;

namespace FiringSquad
{
	public static class StaticGameExtensions
	{
		public static IDamageReceiver GetDamageReceiver(this Collision col)
		{
			return col.transform.GetComponentInParent<IDamageReceiver>();
		}

		public static IDamageReceiver GetDamageReceiver(this RaycastHit col)
		{
			return col.transform.GetComponentInParent<IDamageReceiver>();
		}

		public static IInteractable GetInteractableComponent(this Collision col)
		{
			return col.transform.GetComponentInParent<IInteractable>();
		}

		public static IInteractable GetInteractableComponent(this RaycastHit col)
		{
			return col.transform.GetComponentInParent<IInteractable>();
		}
	}
}
