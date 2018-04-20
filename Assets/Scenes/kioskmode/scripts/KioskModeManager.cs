using FiringSquad.Core;
using FiringSquad.Core.State;
using UnityEngine;

namespace FiringSquad.Gameplay.UI
{
	public class KioskModeManager : MonoBehaviour
	{
		/// <summary>
		/// Unity's update function
		/// </summary>
		private void Update()
		{
			// Get mouse or key input
			if (Input.anyKey || Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f)
				LaunchMainMenu();
		}

		/// <summary>
		/// Launch the main menu
		/// </summary>
		private void LaunchMainMenu()
		{
			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.MENU_SCENE);
		}
	}
}

