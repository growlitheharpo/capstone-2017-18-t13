using FiringSquad.Debug;
using UnityEngine;
using Input = KeatsLib.Unity.Input;

namespace FiringSquad.Gameplay
{
	public class Proto1PlayerDiedUI : MonoBehaviour
	{
		[SerializeField] private ActionProvider mResetButton;
		[SerializeField] private ActionProvider mQuitButton;
		[SerializeField] private GameObject mView;

		// Use this for initialization
		private void Start()
		{
			mView.SetActive(false);

			mQuitButton.OnClick += HandleQuit;
			mResetButton.OnClick += DoReset;
			EventManager.OnPlayerDied += HandlePlayerDeath;
		}

		private void OnDestroy()
		{
			mQuitButton.OnClick -= HandleQuit;
			mResetButton.OnClick -= DoReset;
			EventManager.OnPlayerDied -= HandlePlayerDeath;
		}

		private void HandlePlayerDeath(ICharacter player)
		{
			DebugMenu menu = FindObjectOfType<DebugMenu>();
			if (menu != null && menu.currentlyActive)
				EventManager.UIToggle();

			ServiceLocator.Get<IInput>()
				.SetInputLevel(Input.InputLevel.None);

			mView.SetActive(true);
		}

		private void DoReset()
		{
			EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.PROTOTYPE1_SETUP_SCENE));
		}

		private void HandleQuit()
		{
			EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.MENU_SCENE));
		}
	}
}
