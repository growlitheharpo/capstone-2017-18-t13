using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu UI manager.
/// </summary>
public class UIManager : MonoBehaviour
{
	[SerializeField] private GameObject mMainElementHolder;
	[SerializeField] private ActionProvider mProto3Button;
	[SerializeField] private ActionProvider mQuitButton;

	private void Start()
	{
		mProto3Button.OnClick += LaunchProto3;
		mQuitButton.OnClick += ClickQuit;
	}
	
	private void LaunchProto3()
	{
		mMainElementHolder.SetActive(false);

		ServiceLocator.Get<IGamestateManager>()
			.RequestSceneChange(GamestateManager.BASE_WORLD)
			.RequestSceneChange(GamestateManager.PROTOTYPE3_SCENE, LoadSceneMode.Additive);
	}

	private void ClickQuit()
	{
		ServiceLocator.Get<IGamestateManager>()
			.RequestShutdown();
	}
}
