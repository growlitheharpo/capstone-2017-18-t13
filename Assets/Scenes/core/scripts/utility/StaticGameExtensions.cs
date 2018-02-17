using FiringSquad.Gameplay;
using UnityEngine;

namespace FiringSquad
{
	/// <summary>
	/// Useful static extensions for common interfaces.
	/// </summary>
	public static class StaticGameExtensions
	{
		/// <summary>
		/// Get the damage receiver on the collision object.
		/// </summary>
		public static IDamageReceiver GetDamageReceiver(this Collision col)
		{
			return col.transform.GetComponentInParent<IDamageReceiver>();
		}

		/// <summary>
		/// Get the damage receiver on the RaycastHit object.
		/// </summary>
		public static IDamageReceiver GetDamageReceiver(this RaycastHit col)
		{
			return col.transform.GetComponentInParent<IDamageReceiver>();
		}

		/// <summary>
		/// Get the damage zone on the collision object.
		/// </summary>
		public static IDamageZone GetDamageZone(this Collision col)
		{
			return col.transform.GetComponentInParent<IDamageZone>();
		}

		/// <summary>
		/// Get the damage zone on the RaycastHit object.
		/// </summary>
		public static IDamageZone GetDamageZone(this RaycastHit col)
		{
			return col.transform.GetComponentInParent<IDamageZone>();
		}

		/// <summary>
		/// Get the interactable interface on the collision object.
		/// </summary>
		public static IInteractable GetInteractableComponent(this Collision col)
		{
			return col.transform.GetComponentInParent<IInteractable>();
		}

		/// <summary>
		/// Get the interactable interface on the RaycastHit object.
		/// </summary>
		public static IInteractable GetInteractableComponent(this RaycastHit col)
		{
			return col.transform.GetComponentInParent<IInteractable>();
		}
	}
}
