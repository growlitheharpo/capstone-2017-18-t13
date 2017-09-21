using FiringSquad.Debug;
using UnityEngine;
using Input = KeatsLib.Unity.Input;

namespace FiringSquad.Gameplay
{
	public class PlayerDiedUI : MonoBehaviour
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
			if (FindObjectOfType<DebugMenu>().currentlyActive)
				EventManager.UIToggle();

			ServiceLocator.Get<IInput>()
				.SetInputLevel(Input.InputLevel.None);

			mView.SetActive(true);
		}

		private void DoReset()
		{
			mView.SetActive(false);
			EventManager.Notify(EventManager.ResetLevel);
		}

		private void HandleQuit()
		{
			EventManager.Notify(() => EventManager.RequestSceneChange(GamestateManager.MENU_SCENE));
		}
	}
}
