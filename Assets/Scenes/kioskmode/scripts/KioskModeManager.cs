using FiringSquad.Core;
using FiringSquad.Core.State;
using KeatsLib.Unity;
using UnityEngine;
using UnityEngine.Video;

namespace FiringSquad.Gameplay.UI
{
	public class KioskModeManager : MonoBehaviour
	{
		private VideoPlayer mPlayer;

		/// <summary>
		/// Unity's Awake function
		/// </summary>
		private void Awake()
		{
			mPlayer = GetComponent<VideoPlayer>();
			mPlayer.loopPointReached += OnLoopPointReached;

			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = false;
		}

		/// <summary>
		/// Event Handler: called when the video reaches its end
		/// </summary>
		/// <param name="source"></param>
		private void OnLoopPointReached(VideoPlayer source)
		{
			//StartCoroutine(Coroutines.InvokeAfterSeconds(5.0f, mPlayer.Play));
			LaunchMainMenu();
		}

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
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			ServiceLocator.Get<IGamestateManager>()
				.RequestSceneChange(GamestateManager.MENU_SCENE);
		}
	}
}

