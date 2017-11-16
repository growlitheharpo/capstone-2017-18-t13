using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	/// <summary>
	/// Disables the player's HUD when the game starts, then enables it when the player spawns.
	/// </summary>
	public class PlayerHudToggler : MonoBehaviour
	{
		/// <summary>
		/// Unity's Awake function.
		/// </summary>
		private void Awake()
		{
			EventManager.Local.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Cleanup listeners and event handlers.
		/// </summary>
		private void OnDestroy()
		{
			EventManager.Local.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
		}

		/// <summary>
		/// EVENT HANDLER: Local.OnLocalPlayerSpawned
		/// </summary>
		private void OnLocalPlayerSpawned(CltPlayer obj)
		{
			gameObject.SetActive(true);
			Destroy(this);
		}
	}
}
