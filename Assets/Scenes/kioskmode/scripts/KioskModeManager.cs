using FiringSquad.Core;
using FiringSquad.Core.State;
using UnityEngine;
using UnityEngine.SceneManagement;
using UIButton = UnityEngine.UI.Button;

namespace FiringSquad.Gameplay.UI
{
	public class KioskModeManager : MonoBehaviour
	{
		/// <summary>
		/// Unity's start function
		/// </summary>
		void Start()
		{

		}

		/// <summary>
		/// Unity's update function
		/// </summary>
		void Update()
		{
			// Get mouse or key input
			if (Input.anyKey)
			{
				LaunchMainMenu();
			}
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

